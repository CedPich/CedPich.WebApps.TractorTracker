namespace TractorTracker.Infrastructure.TrackerProviders.Ticatag;

public class TicatagOptions
{
    public const string SectionName = "Ticatag";
    public string BaseUrl { get; set; } = "https://api.ticatag.com";
    public string ApiKey { get; set; } = string.Empty;
}
