using TractorTracker.Domain.Entities;

namespace TractorTracker.Domain.Repositories;

public interface IPositionRepository
{
    Task<PositionRecord?> GetLatestAsync(Guid machineId, CancellationToken ct = default);
    Task<IReadOnlyList<PositionRecord>> GetHistoryAsync(Guid machineId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<DateTimeOffset?> GetLastRecordedAtAsync(Guid machineId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<PositionRecord> records, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
