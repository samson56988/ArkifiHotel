namespace Shared.Data.Dtos;

public sealed class GuestCreateRestaurantOrderLineRequest
{
    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }
}

public sealed class GuestCreateRestaurantOrderRequest
{
    public Guid LocationId { get; set; }

    /// <summary>inRestaurant or roomGuest</summary>
    public string GuestType { get; set; } = "inRestaurant";

    public string? RoomNumber { get; set; }

    public string GuestPhone { get; set; } = string.Empty;

    public string GuestEmail { get; set; } = string.Empty;

    public IReadOnlyList<GuestCreateRestaurantOrderLineRequest> Items { get; set; } =
        Array.Empty<GuestCreateRestaurantOrderLineRequest>();
}

public sealed class GuestRestaurantOrderCheckoutDto
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string PaymentReference { get; set; } = string.Empty;

    public string PaymentUrl { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "NGN";
}

public sealed class GuestRestaurantOrderLineDto
{
    public string ItemName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}

public sealed class GuestRestaurantOrderLookupDto
{
    public string OrderNumber { get; set; } = string.Empty;

    public string PropertyName { get; set; } = string.Empty;

    public string GuestType { get; set; } = string.Empty;

    public string? RoomNumber { get; set; }

    public string GuestPhone { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public IReadOnlyList<GuestRestaurantOrderLineDto> Lines { get; set; } =
        Array.Empty<GuestRestaurantOrderLineDto>();
}

public sealed class GuestRestaurantOrderVerifyResultDto
{
    public bool PaymentSuccessful { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Message { get; set; }

    public GuestRestaurantOrderLookupDto? Order { get; set; }
}

public sealed class RestaurantOrderListItemDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string GuestType { get; set; } = string.Empty;

    public string? RoomNumber { get; set; }

    public string GuestPhone { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public int ItemCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class RestaurantOrderDetailDto
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string GuestType { get; set; } = string.Empty;

    public string? RoomNumber { get; set; }

    public string GuestPhone { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public string? LocationName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public IReadOnlyList<GuestRestaurantOrderLineDto> Lines { get; set; } =
        Array.Empty<GuestRestaurantOrderLineDto>();
}

public sealed class RestaurantOrderListResultDto
{
    public IReadOnlyList<RestaurantOrderListItemDto> Items { get; set; } =
        Array.Empty<RestaurantOrderListItemDto>();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }
}

public sealed class SetRestaurantMenuItemAvailabilityRequest
{
    public bool IsAvailable { get; set; }
}
