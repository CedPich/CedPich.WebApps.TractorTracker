using Microsoft.EntityFrameworkCore;
using TractorTracker.Domain.Entities;
using TractorTracker.Infrastructure.Persistence.Configurations;

namespace TractorTracker.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<PositionRecord> PositionRecords => Set<PositionRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.ApplyConfiguration(new MachineConfiguration());
        modelBuilder.ApplyConfiguration(new PositionRecordConfiguration());
    }
}
