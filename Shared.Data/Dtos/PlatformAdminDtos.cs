namespace Shared.Data.Dtos;

public sealed class PlatformLoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public sealed class PlatformLoginData
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public PlatformStaffAccountDto Account { get; set; } = null!;
}

public sealed class PlatformStaffAccountDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}

public class PlatformBusinessSummaryDto
{
    public Guid Id { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string BusinessType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public string PlanName { get; set; } = string.Empty;

    public string PlanTier { get; set; } = string.Empty;

    public DateTimeOffset? SubscriptionExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? AdminNotes { get; set; }
}

public sealed class PlatformBusinessDetailDto : PlatformBusinessSummaryDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public int LocationCount { get; set; }

    public int RoomCount { get; set; }

    public int BookingCount { get; set; }

    public int StaffCount { get; set; }
}

public sealed class UpdatePlatformBusinessRequest
{
    public string? Status { get; set; }

    public string? AdminNotes { get; set; }
}

public sealed class ListPlatformActivityQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;

    public Guid? BusinessId { get; set; }

    public string? EntityType { get; set; }

    public string? Action { get; set; }

    public DateTimeOffset? FromUtc { get; set; }

    public DateTimeOffset? ToUtc { get; set; }
}

public sealed class PlatformActivityLogDto
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? Summary { get; set; }

    public string? ActorName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class PlatformSubscriptionPaymentDto
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string Status { get; set; } = string.Empty;

    public string? PaymentReference { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class PlatformDashboardStatsDto
{
    public int TotalBusinesses { get; set; }

    public int ActiveBusinesses { get; set; }

    public int HotelCount { get; set; }

    public int ShortletCount { get; set; }

    public int ProSubscriptions { get; set; }

    public int RecentActivityCount { get; set; }
}
