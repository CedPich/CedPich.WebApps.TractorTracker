using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TractorTracker.Application.Interfaces;
using TractorTracker.Application.UseCases;
using TractorTracker.Domain.Repositories;
using TractorTracker.Domain.Services;
using TractorTracker.Infrastructure.BackgroundServices;
using TractorTracker.Infrastructure.Persistence;
using TractorTracker.Infrastructure.Persistence.Repositories;
using TractorTracker.Infrastructure.TrackerProviders.Ticatag;

namespace TractorTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(
                config.GetConnectionString("Default"),
                npgsql => npgsql.UseNetTopologySuite()));

        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();

        services.Configure<TicatagOptions>(config.GetSection(TicatagOptions.SectionName));
        services.AddHttpClient<ITrackerProviderService, TicatagClient>(client =>
        {
            var baseUrl = config[$"{TicatagOptions.SectionName}:BaseUrl"] ?? "https://api.ticatag.com";
            client.BaseAddress = new Uri(baseUrl);
            var apiKey = config[$"{TicatagOptions.SectionName}:ApiKey"] ?? string.Empty;
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        });

        services.AddScoped<ITrackerSyncService, SyncTrackerData>();

        services.Configure<TrackerPollingOptions>(config.GetSection(TrackerPollingOptions.SectionName));
        services.AddHostedService<TrackerPollingService>();

        return services;
    }
}
