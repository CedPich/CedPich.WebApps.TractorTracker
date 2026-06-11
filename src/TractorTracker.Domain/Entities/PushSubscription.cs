namespace TractorTracker.Domain.Entities;

public class PushSubscription
{
    public Guid Id { get; private set; }
    public string Endpoint { get; private set; } = default!;
    public string P256Dh { get; private set; } = default!;
    public string Auth { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private PushSubscription() { }

    public PushSubscription(string endpoint, string p256Dh, string auth)
    {
        Id = Guid.NewGuid();
        Endpoint = endpoint;
        P256Dh = p256Dh;
        Auth = auth;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
