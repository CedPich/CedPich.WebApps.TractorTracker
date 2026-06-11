namespace TractorTracker.Api;

public class AppOptions
{
    public const string SectionName = "App";
    public Guid MachineId { get; set; }
    public VapidOptions Vapid { get; set; } = new();
}

public class VapidOptions
{
    public string Subject { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}
