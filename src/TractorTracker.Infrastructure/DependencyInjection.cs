using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TractorTracker.Domain.Repositories;
using TractorTracker.Infrastructure.Persistence;
using TractorTracker.Infrastructure.Persistence.Repositories;

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

        return services;
    }
}
