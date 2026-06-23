using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Infrastructure.Options;
using Admin.Infrastructure.Payments;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessSubscriptionService : IBusinessSubscriptionService
{
    private readonly AdminDbContext _db;
    private readonly PaymentGatewayRouter _gatewayRouter;
    private readonly PaystackOptions _paystack;

    public BusinessSubscriptionService(
        AdminDbContext db,
        PaymentGatewayRouter gatewayRouter,
        IOptions<PaystackOptions> paystack)
    {
        _db = db;
        _gatewayRouter = gatewayRouter;
        _paystack = paystack.Value;
    }

    public async Task<BusinessSubscriptionDto?> GetCurrentAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var business = await LoadBusinessAsync(businessId, cancellationToken).ConfigureAwait(false);
        return business is null ? null : Map(business);
    }

    public async Task<IReadOnlyList<SubscriptionPlanOptionDto>> GetPlanOptionsAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        var business = await LoadBusinessAsync(businessId, cancellationToken).ConfigureAwait(false);
        if (business is null)
        {
            return Array.Empty<SubscriptionPlanOptionDto>();
        }

        var plans = await _db.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        return plans
            .Select(plan =>
            {
                var evaluation = SubscriptionPlanChangeHelper.Evaluate(business, business.SubscriptionPlan, plan, now);
                var dto = SubscriptionPlanService.Map(plan);
                return new SubscriptionPlanOptionDto
                {
                    Id = dto.Id,
                    Code = dto.Code,
                    Name = dto.Name,
                    Description = dto.Description,
                    Tier = dto.Tier,
                    BillingInterval = dto.BillingInterval,
                    PriceAmount = dto.PriceAmount,
                    Currency = dto.Currency,
                    YearlyDiscountPercent = dto.YearlyDiscountPercent,
                    SortOrder = dto.SortOrder,
                    CanSelect = evaluation.Allowed,
                    RequiresPayment = evaluation.RequiresPayment,
                    ChangeType = evaluation.ChangeType.ToString(),
                    DisabledReason = evaluation.DisabledReason,
                };
            })
            .ToList();
    }

    public async Task<(InitSubscriptionPaymentResultDto? Data, string? Error)> InitializePaymentAsync(
        Guid businessId,
        string planCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_paystack.SecretKey))
        {
            return (null, "Paystack is not configured on the platform.");
        }

        var business = await _db.BusinessRegistrations
            .Include(b => b.SubscriptionPlan)
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (business is null)
        {
            return (null, "Business not found.");
        }

        var target = await ResolveActivePlanAsync(planCode, cancellationToken).ConfigureAwait(false);
        if (target is null)
        {
            return (null, "Select a valid subscription plan.");
        }

        var now = DateTimeOffset.UtcNow;
        var evaluation = SubscriptionPlanChangeHelper.Evaluate(business, business.SubscriptionPlan, target, now);
        if (!evaluation.Allowed)
        {
            return (null, evaluation.DisabledReason ?? "This plan change is not allowed.");
        }

        if (!evaluation.RequiresPayment)
        {
            return (null, "This plan change does not require payment. Use change plan instead.");
        }

        if (target.PriceAmount <= 0)
        {
            return (null, "This plan does not require payment.");
        }

        var reference = await BusinessReferenceCodeGenerator.GenerateUniqueAsync(
            business.BusinessName,
            async (code, ct) => await _db.BusinessSubscriptionPayments
                .AnyAsync(p => p.PaymentReference == code, ct)
                .ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);

        var payment = new BusinessSubscriptionPayment
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = businessId,
            TargetPlanId = target.Id,
            PaymentReference = reference,
            Amount = target.PriceAmount,
            Currency = target.Currency,
            Status = BusinessSubscriptionPaymentStatus.Pending,
            CreatedAt = now,
        };

        _db.BusinessSubscriptionPayments.Add(payment);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var handler = _gatewayRouter.Get(PaymentGatewayProvider.Paystack);
        var init = await handler.InitializeAsync(
            new PaymentInitializeContext
            {
                Provider = PaymentGatewayProvider.Paystack,
                SecretKey = _paystack.SecretKey,
                Reference = reference,
                Amount = target.PriceAmount,
                Currency = target.Currency,
                CustomerEmail = business.ContactEmail,
                CustomerName = $"{business.FirstName} {business.LastName}".Trim(),
                CustomerPhone = business.PhoneNumber,
                RedirectUrl = _paystack.CallbackUrl,
                Description = $"ArkifiStay {target.Name} subscription",
            },
            cancellationToken).ConfigureAwait(false);

        if (!init.Success || string.IsNullOrWhiteSpace(init.PaymentUrl))
        {
            payment.Status = BusinessSubscriptionPaymentStatus.Failed;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return (null, init.Message ?? "Could not start Paystack checkout.");
        }

        return (new InitSubscriptionPaymentResultDto
        {
            PaymentReference = reference,
            PaymentUrl = init.PaymentUrl,
            Amount = target.PriceAmount,
            Currency = target.Currency,
            PlanCode = target.Code,
            PlanName = target.Name,
        }, null);
    }

    public async Task<(BusinessSubscriptionDto? Data, string? Error)> VerifyPaymentAsync(
        Guid businessId,
        string paymentReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_paystack.SecretKey))
        {
            return (null, "Paystack is not configured on the platform.");
        }

        var reference = paymentReference.Trim();
        if (reference.Length < 4)
        {
            return (null, "Payment reference is missing.");
        }

        var payment = await _db.BusinessSubscriptionPayments
            .Include(p => p.TargetPlan)
            .Include(p => p.BusinessRegistration)
            .ThenInclude(b => b.SubscriptionPlan)
            .FirstOrDefaultAsync(
                p => p.BusinessRegistrationId == businessId && p.PaymentReference == reference,
                cancellationToken)
            .ConfigureAwait(false);

        if (payment is null)
        {
            return (null, "No subscription payment matches this reference.");
        }

        var business = payment.BusinessRegistration;

        if (payment.Status == BusinessSubscriptionPaymentStatus.Completed)
        {
            return (Map(business), null);
        }

        if (payment.Status == BusinessSubscriptionPaymentStatus.Failed)
        {
            return (null, "This payment was not completed.");
        }

        var handler = _gatewayRouter.Get(PaymentGatewayProvider.Paystack);
        var verify = await handler.VerifyAsync(
            new PaymentVerifyContext
            {
                Provider = PaymentGatewayProvider.Paystack,
                SecretKey = _paystack.SecretKey,
                Reference = reference,
                ExpectedAmount = payment.Amount,
                Currency = payment.Currency,
            },
            cancellationToken).ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;

        if (!verify.Success)
        {
            payment.Status = BusinessSubscriptionPaymentStatus.Failed;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return (null, verify.Message ?? "Payment could not be verified.");
        }

        var changeType = SubscriptionPlanChangeHelper.Evaluate(
            business,
            business.SubscriptionPlan,
            payment.TargetPlan,
            now).ChangeType;

        business.SubscriptionPlanId = payment.TargetPlanId;
        business.SubscriptionExpiresAt = SubscriptionPlanChangeHelper.ComputeNewExpiry(
            now,
            payment.TargetPlan,
            changeType,
            business.SubscriptionExpiresAt);
        business.UpdatedAt = now;

        payment.Status = BusinessSubscriptionPaymentStatus.Completed;
        payment.CompletedAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        business = await LoadBusinessAsync(businessId, cancellationToken).ConfigureAwait(false);
        return business is null ? (null, "Business not found.") : (Map(business), null);
    }

    public async Task<(BusinessSubscriptionDto? Data, string? Error)> ChangePlanWithoutPaymentAsync(
        Guid businessId,
        string planCode,
        CancellationToken cancellationToken = default)
    {
        var business = await _db.BusinessRegistrations
            .Include(b => b.SubscriptionPlan)
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (business is null)
        {
            return (null, "Business not found.");
        }

        var target = await ResolveActivePlanAsync(planCode, cancellationToken).ConfigureAwait(false);
        if (target is null)
        {
            return (null, "Select a valid subscription plan.");
        }

        var now = DateTimeOffset.UtcNow;
        var evaluation = SubscriptionPlanChangeHelper.Evaluate(business, business.SubscriptionPlan, target, now);
        if (!evaluation.Allowed)
        {
            return (null, evaluation.DisabledReason ?? "This plan change is not allowed.");
        }

        if (evaluation.RequiresPayment)
        {
            return (null, "This plan change requires payment via Paystack.");
        }

        business.SubscriptionPlanId = target.Id;
        business.SubscriptionExpiresAt = SubscriptionPlanChangeHelper.ComputeNewExpiry(
            now,
            target,
            evaluation.ChangeType,
            business.SubscriptionExpiresAt);
        business.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        business = await LoadBusinessAsync(businessId, cancellationToken).ConfigureAwait(false);
        return business is null ? (null, "Business not found.") : (Map(business), null);
    }

    public async Task<IReadOnlyList<BusinessSubscriptionPaymentHistoryDto>> GetPaymentHistoryAsync(
        Guid businessId,
        CancellationToken cancellationToken = default)
    {
        return await _db.BusinessSubscriptionPayments
            .AsNoTracking()
            .Where(p => p.BusinessRegistrationId == businessId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new BusinessSubscriptionPaymentHistoryDto
            {
                Id = p.Id,
                PaymentReference = p.PaymentReference,
                PlanName = p.TargetPlan.Name,
                PlanCode = p.TargetPlan.Code,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status == BusinessSubscriptionPaymentStatus.Completed
                    ? "Completed"
                    : p.Status == BusinessSubscriptionPaymentStatus.Failed
                        ? "Failed"
                        : "Pending",
                CreatedAt = p.CreatedAt,
                CompletedAt = p.CompletedAt,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<BusinessRegistration?> LoadBusinessAsync(
        Guid businessId,
        CancellationToken cancellationToken)
    {
        return await _db.BusinessRegistrations
            .AsNoTracking()
            .Include(b => b.SubscriptionPlan)
            .FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<SubscriptionPlan?> ResolveActivePlanAsync(
        string planCode,
        CancellationToken cancellationToken)
    {
        var code = planCode.Trim().ToLowerInvariant();
        return await _db.SubscriptionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == code && p.IsActive, cancellationToken)
            .ConfigureAwait(false);
    }

    internal static BusinessSubscriptionDto Map(BusinessRegistration business)
    {
        var now = DateTimeOffset.UtcNow;
        var status = SubscriptionAccessHelper.GetAccessStatus(business, now);
        int? daysRemaining = null;
        if (business.SubscriptionExpiresAt.HasValue)
        {
            var remaining = (int)Math.Ceiling((business.SubscriptionExpiresAt.Value - now).TotalDays);
            daysRemaining = Math.Max(remaining, 0);
        }

        return new BusinessSubscriptionDto
        {
            BusinessId = business.Id,
            BusinessType = business.BusinessType == BusinessType.Shortlet ? "Shortlet" : "Hotel",
            Plan = SubscriptionPlanService.Map(business.SubscriptionPlan),
            ExpiresAt = business.SubscriptionExpiresAt,
            Status = status switch
            {
                SubscriptionAccessStatus.GracePeriod => "GracePeriod",
                SubscriptionAccessStatus.Expired => "Expired",
                _ => "Active",
            },
            GracePeriodDays = SubscriptionAccessHelper.StorefrontGracePeriodDays,
            IsStorefrontAccessible = SubscriptionAccessHelper.IsStorefrontAccessible(business, now),
            DaysRemaining = daysRemaining,
        };
    }
}
