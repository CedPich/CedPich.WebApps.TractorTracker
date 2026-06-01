using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
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
        var machine = await machines.GetByTrackerDeviceIdAsync(payload.DeviceId, ct);
        if (machine is null)
        {
            logger.LogWarning("Received webhook for unknown device {DeviceId}", payload.DeviceId);
            return Ok();
        }

        var record = new PositionRecord(
            machine.Id,
            GeoFactory.CreatePoint(new Coordinate(payload.Lng, payload.Lat)),
            DateTimeOffset.FromUnixTimeSeconds(payload.Timestamp),
            payload.Speed,
            payload.Heading);

        await positions.AddRangeAsync([record], ct);
        await positions.SaveChangesAsync(ct);

        return Ok();
    }
}

public record TicatagWebhookPayload(
    string DeviceId,
    double Lat,
    double Lng,
    long Timestamp,
    double? Speed,
    double? Heading);
