using Microsoft.EntityFrameworkCore;
using TractorTracker.Domain.Entities;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Infrastructure.Persistence.Repositories;

public class MachineRepository(AppDbContext db) : IMachineRepository
{
    public Task<Machine?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Machines.FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<Machine?> GetByTrackerDeviceIdAsync(string deviceId, CancellationToken ct = default) =>
        db.Machines.FirstOrDefaultAsync(m => m.TrackerDeviceId == deviceId, ct);

    public async Task<IReadOnlyList<Machine>> GetAllAsync(CancellationToken ct = default) =>
        await db.Machines.ToListAsync(ct);

    public async Task AddAsync(Machine machine, CancellationToken ct = default) =>
        await db.Machines.AddAsync(machine, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
