using TractorTracker.Application.DTOs;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Application.UseCases;

public class GetDailyWorkHours(IPositionRepository positions)
{
    public async Task<IReadOnlyList<DailyWorkHoursDto>> ExecuteAsync(
        Guid machineId,
        DateOnly from,
        DateOnly to,
        TimeSpan pauseThreshold,
        CancellationToken ct = default)
    {
        var start = new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end   = new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var records = await positions.GetHistoryAsync(machineId, start, end, ct);

        return records
            .GroupBy(r => DateOnly.FromDateTime(r.RecordedAt.UtcDateTime))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var ordered = g.OrderBy(r => r.RecordedAt).ToList();

                // Somme des intervalles entre points consécutifs, hors pauses
                var activeHours = 0.0;
                for (var i = 1; i < ordered.Count; i++)
                {
                    var gap = ordered[i].RecordedAt - ordered[i - 1].RecordedAt;
                    if (gap <= pauseThreshold)
                        activeHours += gap.TotalHours;
                }

                return new DailyWorkHoursDto(g.Key, Math.Round(activeHours, 2), ordered.Count);
            })
            .ToList();
    }
}
