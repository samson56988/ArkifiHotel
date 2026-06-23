namespace Shared.Data.Dtos;

public sealed class BusinessDashboardDto
{
    public string BusinessName { get; set; } = string.Empty;

    public string Currency { get; set; } = "NGN";

    public decimal TotalRevenue { get; set; }

    public decimal BookingRevenue { get; set; }

    public decimal RestaurantRevenue { get; set; }

    public decimal? TotalRevenueChangePercent { get; set; }

    public decimal OccupancyRatePercent { get; set; }

    public decimal? OccupancyChangePercent { get; set; }

    public int TotalRoomUnits { get; set; }

    public int RoomTypesCount { get; set; }

    public int ActiveStaysToday { get; set; }

    public int PendingRestaurantOrders { get; set; }

    public int PendingEventHallRequests { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public IReadOnlyList<DashboardRevenueTrendPointDto> RevenueTrend { get; set; } = [];

    public IReadOnlyList<DashboardRecentBookingDto> RecentBookings { get; set; } = [];

    public IReadOnlyList<DashboardGatewaySplitDto> GatewaySplit { get; set; } = [];

    public IReadOnlyList<DashboardTopRoomDto> TopRooms { get; set; } = [];
}

public sealed class DashboardRecentBookingDto
{
    public Guid Id { get; set; }

    public string GuestName { get; set; } = string.Empty;

    public string RoomName { get; set; } = string.Empty;

    public DateOnly CheckInDate { get; set; }

    public int Nights { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "NGN";
}

public sealed class DashboardGatewaySplitDto
{
    public string Label { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal Percent { get; set; }
}

public sealed class DashboardTopRoomDto
{
    public Guid RoomId { get; set; }

    public string RoomName { get; set; } = string.Empty;

    public decimal Revenue { get; set; }

    public decimal OccupancyPercent { get; set; }

    public string Currency { get; set; } = "NGN";
}

public sealed class DashboardRevenueTrendPointDto
{
    public DateOnly Date { get; set; }

    public decimal BookingRevenue { get; set; }

    public decimal RestaurantRevenue { get; set; }

    public decimal TotalRevenue { get; set; }
}
