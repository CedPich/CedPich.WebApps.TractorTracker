namespace TractorTracker.Application.Interfaces;

public interface ITrackerSyncService
{
    Task SyncMachineAsync(Guid machineId, CancellationToken ct = default);
}
