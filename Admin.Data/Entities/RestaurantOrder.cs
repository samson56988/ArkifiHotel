using Admin.Data.Enums;

namespace Admin.Data.Entities;

public class RestaurantOrder
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid? LocationId { get; set; }

    public BusinessLocation? Location { get; set; }

    public RestaurantGuestType GuestType { get; set; }

    public string? RoomNumber { get; set; }

    public string GuestPhone { get; set; } = null!;

    public string OrderNumber { get; set; } = null!;

    public RestaurantOrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<RestaurantOrderLine> Lines { get; set; } = new List<RestaurantOrderLine>();

    public ICollection<RestaurantOrderPayment> Payments { get; set; } = new List<RestaurantOrderPayment>();
}
