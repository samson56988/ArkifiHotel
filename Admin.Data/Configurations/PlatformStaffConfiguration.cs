using Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public sealed class PlatformStaffConfiguration : IEntityTypeConfiguration<PlatformStaff>
{
    public void Configure(EntityTypeBuilder<PlatformStaff> builder)
    {
        builder.ToTable("PlatformStaff");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.HashedPassword)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .IsRequired();
    }
}
