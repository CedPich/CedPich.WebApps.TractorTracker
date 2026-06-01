using TractorTracker.Domain.Entities;

namespace TractorTracker.Domain.Repositories;

public interface IMachineRepository
{
    Task<Machine?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Machine?> GetByTrackerDeviceIdAsync(string deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<Machine>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Machine machine, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
