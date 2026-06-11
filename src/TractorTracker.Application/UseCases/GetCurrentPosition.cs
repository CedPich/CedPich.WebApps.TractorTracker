using TractorTracker.Application.DTOs;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Application.UseCases;

public class GetCurrentPosition(IPositionRepository positions)
{
    public async Task<PositionDto?> ExecuteAsync(Guid machineId, CancellationToken ct = default)
    {
        var r = await positions.GetLatestAsync(machineId, ct);
        if (r is null) return null;

        return new PositionDto(r.Latitude, r.Longitude, r.RecordedAt,
            r.SpeedKmh, r.HeadingDegrees, r.AltitudeMeters, r.Satellites, r.FormattedAddress);
    }
}
