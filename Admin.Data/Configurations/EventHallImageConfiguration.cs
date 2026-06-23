using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class EventHallImageConfiguration : IEntityTypeConfiguration<EventHallImage>
{
    public void Configure(EntityTypeBuilder<EventHallImage> builder)
    {
        builder.ToTable("EventHallImages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RelativePath).IsRequired().HasMaxLength(500);
        builder.Property(e => e.OriginalFileName).HasMaxLength(260);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.EventHallId);

        builder.HasOne(e => e.EventHall)
            .WithMany(h => h.Images)
            .HasForeignKey(e => e.EventHallId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
