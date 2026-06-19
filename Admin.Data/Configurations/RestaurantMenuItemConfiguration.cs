using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class RestaurantMenuItemConfiguration : IEntityTypeConfiguration<RestaurantMenuItem>
{
    public void Configure(EntityTypeBuilder<RestaurantMenuItem> builder)
    {
        builder.ToTable("RestaurantMenuItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Price).HasPrecision(12, 2);
        builder.Property(e => e.TagsJson).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.ImageRelativePath).HasMaxLength(500);
        builder.Property(e => e.ImageOriginalFileName).HasMaxLength(260);
        builder.Property(e => e.SortOrder).IsRequired();
        builder.Property(e => e.IsArchived).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsAvailable).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.CategoryId);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
