using Microsoft.EntityFrameworkCore;
using TractorTracker.Domain.Entities;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Infrastructure.Persistence.Repositories;

public class PushSubscriptionRepository(AppDbContext db) : IPushSubscriptionRepository
{
    public Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken ct = default) =>
        db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint, ct);

    public async Task<IReadOnlyList<PushSubscription>> GetAllAsync(CancellationToken ct = default) =>
        await db.PushSubscriptions.ToListAsync(ct);

    public async Task AddAsync(PushSubscription subscription, CancellationToken ct = default) =>
        await db.PushSubscriptions.AddAsync(subscription, ct);

    public Task RemoveAsync(PushSubscription subscription, CancellationToken ct = default)
    {
        db.PushSubscriptions.Remove(subscription);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
