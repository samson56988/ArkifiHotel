using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class RoomImageConfiguration : IEntityTypeConfiguration<RoomImage>
{
    public void Configure(EntityTypeBuilder<RoomImage> builder)
    {
        builder.ToTable("RoomImages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RelativePath)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(e => e.OriginalFileName)
            .HasMaxLength(256);

        builder.Property(e => e.SortOrder)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => e.RoomId);

        builder.HasOne(e => e.Room)
            .WithMany(r => r.Images)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
