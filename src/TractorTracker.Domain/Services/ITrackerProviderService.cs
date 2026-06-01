namespace TractorTracker.Domain.Services;

public record TrackerPosition(
    string DeviceId,
    double Latitude,
    double Longitude,
    DateTimeOffset RecordedAt,
    double? SpeedKmh,
    double? HeadingDegrees);

public interface ITrackerProviderService
{
    Task<IReadOnlyList<TrackerPosition>> GetPositionsSinceAsync(string deviceId, DateTimeOffset since, CancellationToken ct = default);
    Task<TrackerPosition?> GetLatestPositionAsync(string deviceId, CancellationToken ct = default);
}
