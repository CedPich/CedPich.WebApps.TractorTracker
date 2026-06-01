using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TractorTracker.Domain.Services;

namespace TractorTracker.Infrastructure.TrackerProviders.Ticatag;

public class TicatagClient(HttpClient http, IOptions<TicatagOptions> options) : ITrackerProviderService
{
    private readonly TicatagOptions _opts = options.Value;

    public async Task<IReadOnlyList<TrackerPosition>> GetPositionsSinceAsync(
        string deviceId, DateTimeOffset since, CancellationToken ct = default)
    {
        var sinceUnix = since.ToUnixTimeSeconds();
        var response = await http.GetAsync(
            $"/v1/devices/{deviceId}/locations?from={sinceUnix}", ct);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<TicatagLocationResponse[]>(ct) ?? [];
        return raw.Select(r => new TrackerPosition(
            deviceId,
            r.Lat,
            r.Lng,
            DateTimeOffset.FromUnixTimeSeconds(r.Timestamp),
            r.Speed,
            r.Heading)).ToList();
    }

    public async Task<TrackerPosition?> GetLatestPositionAsync(string deviceId, CancellationToken ct = default)
    {
        var response = await http.GetAsync($"/v1/devices/{deviceId}/location", ct);
        if (!response.IsSuccessStatusCode) return null;

        var raw = await response.Content.ReadFromJsonAsync<TicatagLocationResponse>(ct);
        if (raw is null) return null;

        return new TrackerPosition(
            deviceId, raw.Lat, raw.Lng,
            DateTimeOffset.FromUnixTimeSeconds(raw.Timestamp),
            raw.Speed, raw.Heading);
    }

    private record TicatagLocationResponse(
        double Lat, double Lng, long Timestamp,
        double? Speed, double? Heading);
}
