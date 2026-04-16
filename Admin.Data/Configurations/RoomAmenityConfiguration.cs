using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class RoomAmenityConfiguration : IEntityTypeConfiguration<RoomAmenity>
{
    public void Configure(EntityTypeBuilder<RoomAmenity> builder)
    {
        builder.ToTable("RoomAmenities");

        builder.HasKey(e => new { e.RoomId, e.AmenityId });

        builder.HasOne(e => e.Room)
            .WithMany(r => r.RoomAmenities)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Amenity)
            .WithMany(a => a.RoomAmenities)
            .HasForeignKey(e => e.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
