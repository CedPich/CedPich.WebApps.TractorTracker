using Microsoft.EntityFrameworkCore;
using TractorTracker.Domain.Entities;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Infrastructure.Persistence.Repositories;

public class PositionRepository(AppDbContext db) : IPositionRepository
{
    public Task<PositionRecord?> GetLatestAsync(Guid machineId, CancellationToken ct = default) =>
        db.PositionRecords
            .Where(p => p.MachineId == machineId)
            .OrderByDescending(p => p.RecordedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<PositionRecord>> GetHistoryAsync(
        Guid machineId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default) =>
        await db.PositionRecords
            .Where(p => p.MachineId == machineId && p.RecordedAt >= from && p.RecordedAt <= to)
            .OrderBy(p => p.RecordedAt)
            .ToListAsync(ct);

    public Task<DateTimeOffset?> GetLastRecordedAtAsync(Guid machineId, CancellationToken ct = default) =>
        db.PositionRecords
            .Where(p => p.MachineId == machineId)
            .Select(p => (DateTimeOffset?)p.RecordedAt)
            .MaxAsync(ct);

    public async Task AddRangeAsync(IEnumerable<PositionRecord> records, CancellationToken ct = default) =>
        await db.PositionRecords.AddRangeAsync(records, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
