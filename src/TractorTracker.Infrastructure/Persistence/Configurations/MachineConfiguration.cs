using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TractorTracker.Domain.Entities;

namespace TractorTracker.Infrastructure.Persistence.Configurations;

public class MachineConfiguration : IEntityTypeConfiguration<Machine>
{
    public void Configure(EntityTypeBuilder<Machine> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
        builder.Property(m => m.TrackerDeviceId).HasMaxLength(100).IsRequired();
        builder.HasIndex(m => m.TrackerDeviceId).IsUnique();
    }
}
