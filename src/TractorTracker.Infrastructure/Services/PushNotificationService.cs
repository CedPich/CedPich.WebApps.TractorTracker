using Microsoft.Extensions.Logging;
using TractorTracker.Domain.Repositories;
using WebPush;

namespace TractorTracker.Infrastructure.Services;

public class PushNotificationService(
    IPushSubscriptionRepository subscriptions,
    WebPushClient webPushClient,
    VapidDetails vapidDetails,
    ILogger<PushNotificationService> logger)
{
    public async Task SendAsync(string title, string body, CancellationToken ct = default)
    {
        var all = await subscriptions.GetAllAsync(ct);
        if (all.Count == 0) return;

        var payload = System.Text.Json.JsonSerializer.Serialize(new { title, body });
        var stale = new List<Domain.Entities.PushSubscription>();

        foreach (var sub in all)
        {
            try
            {
                var pushSub = new PushSubscription(sub.Endpoint, sub.P256Dh, sub.Auth);
                await webPushClient.SendNotificationAsync(pushSub, payload, vapidDetails, ct);
            }
            catch (WebPushException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Gone
                                           || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogInformation("Souscription expirée, suppression : {Endpoint}", sub.Endpoint);
                stale.Add(sub);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Échec envoi push vers {Endpoint}", sub.Endpoint);
            }
        }

        foreach (var sub in stale)
            await subscriptions.RemoveAsync(sub, ct);

        if (stale.Count > 0)
            await subscriptions.SaveChangesAsync(ct);
    }
}
