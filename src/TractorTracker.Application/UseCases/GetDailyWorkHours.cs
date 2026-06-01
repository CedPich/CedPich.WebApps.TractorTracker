using TractorTracker.Application.DTOs;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Application.UseCases;

public class GetDailyWorkHours(IPositionRepository positions)
{
    public async Task<IReadOnlyList<DailyWorkHoursDto>> ExecuteAsync(
        Guid machineId,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
    {
        var start = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var records = await positions.GetHistoryAsync(machineId, start, end, ct);

        return records
            .GroupBy(r => DateOnly.FromDateTime(r.RecordedAt.UtcDateTime))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var ordered = g.OrderBy(r => r.RecordedAt).ToList();
                var span = ordered.Count > 1
                    ? (ordered.Last().RecordedAt - ordered.First().RecordedAt).TotalHours
                    : 0;
                return new DailyWorkHoursDto(g.Key, Math.Round(span, 2), ordered.Count);
            })
            .ToList();
    }
}
