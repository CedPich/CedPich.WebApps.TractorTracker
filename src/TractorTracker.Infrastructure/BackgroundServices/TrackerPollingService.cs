using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TractorTracker.Application.Interfaces;
using TractorTracker.Domain.Repositories;

namespace TractorTracker.Infrastructure.BackgroundServices;

public class TrackerPollingOptions
{
    public const string SectionName = "TrackerPolling";
    public int IntervalMinutes { get; set; } = 5;
}

public class TrackerPollingService(
    IServiceScopeFactory scopeFactory,
    IOptions<TrackerPollingOptions> options,
    ILogger<TrackerPollingService> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(options.Value.IntervalMinutes);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);
            await SyncAllMachinesAsync(stoppingToken);
        }
    }

    private async Task SyncAllMachinesAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var machines = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
            var syncService = scope.ServiceProvider.GetRequiredService<ITrackerSyncService>();

            var all = await machines.GetAllAsync(ct);
            foreach (var machine in all)
            {
                try { await syncService.SyncMachineAsync(machine.Id, ct); }
                catch (Exception ex) { logger.LogError(ex, "Sync failed for machine {MachineId}", machine.Id); }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "TrackerPollingService error");
        }
    }
}
