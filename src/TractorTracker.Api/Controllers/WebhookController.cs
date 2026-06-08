using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using System.Text.Json.Serialization;
using TractorTracker.Domain.Entities;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Api.Controllers;

[ApiController]
[Route("api/webhook")]
public class WebhookController(
    IMachineRepository machines,
    IPositionRepository positions,
    ILogger<WebhookController> logger) : ControllerBase
{
    private static readonly GeometryFactory GeoFactory = new(new PrecisionModel(), 4326);

    [HttpPost("ticatag")]
    public async Task<IActionResult> TicatagWebhook(
        [FromBody] TicatagWebhookPayload payload,
        CancellationToken ct)
    {
        if (payload.HookEvent != "location_changed")
            return Ok(); // ignorer les autres types d'événements

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
            return Ok(); // 200 pour éviter les retries Ticatag
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

// ---- DTOs de désérialisation ----

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
