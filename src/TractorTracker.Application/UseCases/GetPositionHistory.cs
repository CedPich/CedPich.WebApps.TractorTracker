using TractorTracker.Application.DTOs;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Application.UseCases;

public class GetPositionHistory(IPositionRepository positions)
{
    public async Task<IReadOnlyList<PositionDto>> ExecuteAsync(
        Guid machineId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct = default)
    {
        var records = await positions.GetHistoryAsync(machineId, from, to, ct);
        return records
            .Select(r => new PositionDto(r.Latitude, r.Longitude, r.RecordedAt,
                r.SpeedKmh, r.HeadingDegrees, r.AltitudeMeters, r.Satellites, r.FormattedAddress))
            .ToList();
    }
}
