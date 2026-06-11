using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TractorTracker.Domain.Entities;

namespace TractorTracker.Infrastructure.Persistence.Configurations;

public class PositionRecordConfiguration : IEntityTypeConfiguration<PositionRecord>
{
    public void Configure(EntityTypeBuilder<PositionRecord> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Location).HasColumnType("geometry(Point,4326)").IsRequired();
        builder.Property(p => p.FormattedAddress).HasMaxLength(512);
        builder.HasIndex(p => new { p.MachineId, p.RecordedAt });
        builder.HasIndex(p => p.RecordedAt);
    }
}
