using TractorTracker.Application.DTOs;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Application.UseCases;

public class GetCurrentPosition(IPositionRepository positions)
{
    public async Task<PositionDto?> ExecuteAsync(Guid machineId, CancellationToken ct = default)
    {
        var record = await positions.GetLatestAsync(machineId, ct);
        if (record is null) return null;

        return new PositionDto(record.Latitude, record.Longitude, record.RecordedAt, record.SpeedKmh, record.HeadingDegrees);
    }
}
