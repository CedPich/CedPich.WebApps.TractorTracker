using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using System.Text.Json;
using System.Text.Json.Serialization;
using TractorTracker.Domain.Entities;
using TractorTracker.Domain.Repositories;
using TractorTracker.Infrastructure.Services;

namespace TractorTracker.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController(
    IMachineRepository machines,
    IPositionRepository positions,
    PushNotificationService push,
    IOptions<AppOptions> options,
    ILogger<WebhookController> logger,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private static readonly GeometryFactory GeoFactory = new(new PrecisionModel(), 4326);
    private readonly ILogger _rawLogger = loggerFactory.CreateLogger("Webhook.Raw");
    private bool RawLogEnabled => options.Value.EnableWebhookRawLog;

    [HttpPost("ticatag")]
    public async Task<IActionResult> TicatagWebhook(CancellationToken ct)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawJson = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        if (RawLogEnabled)
            _rawLogger.LogInformation("{RawJson}", rawJson);

        TicatagWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<TicatagWebhookPayload>(rawJson);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Payload webhook invalide");
            return Ok();
        }

        if (payload is null || payload.HookEvent != "location_changed")
            return Ok();

        var serialNumber = payload.Device?.SerialNumber;
        if (string.IsNullOrEmpty(serialNumber))
        {
            logger.LogWarning("Webhook reçu sans serial_number");
            return Ok();
        }

        var machine = await machines.GetByTrackerDeviceIdAsync(serialNumber, ct);
        if (machine is null)
        {
            logger.LogWarning("Webhook reçu pour un appareil inconnu : {SerialNumber}", serialNumber);
            return Ok();
        }

        var ev = payload.Event;

        var threshold = TimeSpan.FromHours(options.Value.InactivityThresholdHours);
        var lastAt = payload.LastLocation?.Timestamp ?? await positions.GetLastRecordedAtAsync(machine.Id, ct);
        var isResumingAfterInactivity = lastAt is null || (ev.Timestamp - lastAt.Value) >= threshold;

        var record = new PositionRecord(
            machine.Id,
            GeoFactory.CreatePoint(new Coordinate(ev.Longitude, ev.Latitude)),
            ev.Timestamp,
            speedKmh: ev.Speed,
            headingDegrees: ev.Angle,
            altitudeMeters: ev.Altitude,
            satellites: ev.Satellites,
            formattedAddress: ev.FormattedAddress);

        await positions.AddRangeAsync([record], ct);
        await positions.SaveChangesAsync(ct);

        logger.LogInformation("Position enregistrée pour {MachineName} : {Lat},{Lng} à {Timestamp}",
            machine.Name, ev.Latitude, ev.Longitude, ev.Timestamp);

        if (isResumingAfterInactivity)
        {
            logger.LogInformation("Reprise d'activité après inactivité, envoi notification push");
            await push.SendAsync("TractorTracker", $"{machine.Name} est en marche 🚜", ct);
        }

        return Ok();
    }
}

public record TicatagWebhookPayload(
    [property: JsonPropertyName("hook_event")] string HookEvent,
    [property: JsonPropertyName("device")] TicatagDevice? Device,
    [property: JsonPropertyName("event")] TicatagLocationEvent Event,
    [property: JsonPropertyName("last_location")] TicatagLastLocation? LastLocation);

public record TicatagDevice(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("serial_number")] string? SerialNumber);

public record TicatagLocationEvent(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("altitude")] double? Altitude,
    [property: JsonPropertyName("satellites")] int? Satellites,
    [property: JsonPropertyName("speed")] double? Speed,
    [property: JsonPropertyName("angle")] double? Angle,
    [property: JsonPropertyName("formatted_address")] string? FormattedAddress);

public record TicatagLastLocation(
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp);
