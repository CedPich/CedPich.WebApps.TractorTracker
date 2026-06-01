namespace TractorTracker.Domain.Entities;

public class Machine
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string TrackerDeviceId { get; private set; } = string.Empty;
    public DateTimeOffset? LastSyncAt { get; private set; }

    private Machine() { }

    public Machine(Guid id, string name, string trackerDeviceId)
    {
        Id = id;
        Name = name;
        TrackerDeviceId = trackerDeviceId;
    }

    public void UpdateLastSync(DateTimeOffset syncAt) => LastSyncAt = syncAt;
}
