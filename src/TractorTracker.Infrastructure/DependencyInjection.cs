using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TractorTracker.Domain.Repositories;
using TractorTracker.Infrastructure.Persistence;
using TractorTracker.Infrastructure.Persistence.Repositories;
using TractorTracker.Infrastructure.Services;
using WebPush;

namespace TractorTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(
                config.GetConnectionString("Default"),
                npgsql =>
                {
                    npgsql.UseNetTopologySuite();
                    npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                }));

        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();

        var vapidSubject = config["App:Vapid:Subject"] ?? "mailto:admin@example.com";
        var vapidPublic  = config["App:Vapid:PublicKey"] ?? string.Empty;
        var vapidPrivate = config["App:Vapid:PrivateKey"] ?? string.Empty;

        services.AddSingleton(new VapidDetails(vapidSubject, vapidPublic, vapidPrivate));
        services.AddSingleton<WebPushClient>();
        services.AddScoped<PushNotificationService>();

        return services;
    }
}
