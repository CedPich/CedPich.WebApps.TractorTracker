using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using System.Text.Json;
using System.Text.Json.Serialization;
using TractorTracker.Domain.Entities;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController(
    IMachineRepository machines,
    IPositionRepository positions,
    ILogger<WebhookController> logger,
    ILoggerFactory loggerFactory) : ControllerBase
{
    private static readonly GeometryFactory GeoFactory = new(new PrecisionModel(), 4326);
    private readonly ILogger _rawLogger = loggerFactory.CreateLogger("Webhook.Raw");

    [HttpPost("ticatag")]
    public async Task<IActionResult> TicatagWebhook(CancellationToken ct)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawJson = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

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
        var record = new PositionRecord(
            machine.Id,
            GeoFactory.CreatePoint(new Coordinate(ev.Longitude, ev.Latitude)),
            ev.Timestamp);

        await positions.AddRangeAsync([record], ct);
        await positions.SaveChangesAsync(ct);

        logger.LogInformation("Position enregistrée pour {MachineName} : {Lat},{Lng} à {Timestamp}",
            machine.Name, ev.Latitude, ev.Longitude, ev.Timestamp);

        return Ok();
    }
}

public record TicatagWebhookPayload(
    [property: JsonPropertyName("hook_event")] string HookEvent,
    [property: JsonPropertyName("device")] TicatagDevice? Device,
    [property: JsonPropertyName("event")] TicatagLocationEvent Event);

public record TicatagDevice(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("serial_number")] string? SerialNumber);

public record TicatagLocationEvent(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("accuracy")] double? Accuracy,
    [property: JsonPropertyName("formatted_address")] string? FormattedAddress);
