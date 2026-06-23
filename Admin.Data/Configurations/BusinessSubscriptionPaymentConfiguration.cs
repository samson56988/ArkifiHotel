using Admin.Data.Entities;
using Admin.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Admin.Data.Configurations;

public class BusinessSubscriptionPaymentConfiguration : IEntityTypeConfiguration<BusinessSubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<BusinessSubscriptionPayment> builder)
    {
        builder.ToTable("BusinessSubscriptionPayments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentReference)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(e => e.Amount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(8)
            .HasDefaultValue("NGN");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(BusinessSubscriptionPaymentStatus.Pending);

        builder.HasOne(e => e.BusinessRegistration)
            .WithMany()
            .HasForeignKey(e => e.BusinessRegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.TargetPlan)
            .WithMany()
            .HasForeignKey(e => e.TargetPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.PaymentReference)
            .IsUnique();

        builder.HasIndex(e => new { e.BusinessRegistrationId, e.Status });
    }
}
