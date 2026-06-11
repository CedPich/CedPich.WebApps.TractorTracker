using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TractorTracker.Domain.Entities;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Api.Controllers;

[ApiController]
[Route("api/push")]
public class PushController(
    IPushSubscriptionRepository subscriptions,
    IOptions<AppOptions> options) : ControllerBase
{
    [HttpGet("vapid-public-key")]
    public IActionResult GetPublicKey() =>
        Ok(new { publicKey = options.Value.Vapid.PublicKey });

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Endpoint) || string.IsNullOrEmpty(req.P256Dh) || string.IsNullOrEmpty(req.Auth))
            return BadRequest();

        var existing = await subscriptions.GetByEndpointAsync(req.Endpoint, ct);
        if (existing is null)
        {
            await subscriptions.AddAsync(new PushSubscription(req.Endpoint, req.P256Dh, req.Auth), ct);
            await subscriptions.SaveChangesAsync(ct);
        }

        return Ok();
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest req, CancellationToken ct)
    {
        var existing = await subscriptions.GetByEndpointAsync(req.Endpoint, ct);
        if (existing is not null)
        {
            await subscriptions.RemoveAsync(existing, ct);
            await subscriptions.SaveChangesAsync(ct);
        }

        return Ok();
    }
}

public record SubscribeRequest(string Endpoint, string P256Dh, string Auth);
public record UnsubscribeRequest(string Endpoint);
