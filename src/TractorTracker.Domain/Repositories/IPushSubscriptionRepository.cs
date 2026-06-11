using TractorTracker.Domain.Entities;

namespace TractorTracker.Domain.Repositories;

public interface IPushSubscriptionRepository
{
    Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken ct = default);
    Task<IReadOnlyList<PushSubscription>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(PushSubscription subscription, CancellationToken ct = default);
    Task RemoveAsync(PushSubscription subscription, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
