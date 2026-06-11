using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TractorTracker.Domain.Entities;

namespace TractorTracker.Infrastructure.Persistence.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Endpoint).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.P256Dh).IsRequired().HasMaxLength(512);
        builder.Property(x => x.Auth).IsRequired().HasMaxLength(256);
        builder.HasIndex(x => x.Endpoint).IsUnique();
    }
}
