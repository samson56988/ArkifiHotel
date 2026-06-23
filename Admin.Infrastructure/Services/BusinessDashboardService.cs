using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessDashboardService : IBusinessDashboardService
{
    private const int MaxRangeDays = 366;

    private readonly AdminDbContext _db;

    public BusinessDashboardService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<BusinessDashboardDto?> GetDashboardAsync(
        Guid businessId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var business = await _db.BusinessRegistrations
            .AsNoTracking()
            .Where(b => b.Id == businessId)
            .Select(b => new { b.BusinessName })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (business is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodStart = from ?? new DateOnly(today.Year, today.Month, 1);
        var periodEnd = to ?? today;

        if (periodEnd < periodStart)
        {
            periodEnd = periodStart;
        }

        var rangeDays = periodEnd.DayNumber - periodStart.DayNumber + 1;
        if (rangeDays > MaxRangeDays)
        {
            periodStart = periodEnd.AddDays(-(MaxRangeDays - 1));
        }

        var periodEndExclusive = periodEnd.AddDays(1);
        var periodStartDt = ToUtcOffset(periodStart);
        var periodEndExclusiveDt = ToUtcOffset(periodEndExclusive);

        var previousEnd = periodStart.AddDays(-1);
        var previousStart = previousEnd.AddDays(-(periodEnd.DayNumber - periodStart.DayNumber));
        var previousStartDt = ToUtcOffset(previousStart);
        var previousEndExclusiveDt = ToUtcOffset(previousEnd.AddDays(1));

        var bookingRevenue = await SumCompletedBookingPaymentsAsync(
            businessId, periodStartDt, periodEndExclusiveDt, cancellationToken).ConfigureAwait(false);
        var restaurantRevenue = await SumCompletedRestaurantPaymentsAsync(
            businessId, periodStartDt, periodEndExclusiveDt, cancellationToken).ConfigureAwait(false);

        var prevBookingRevenue = await SumCompletedBookingPaymentsAsync(
            businessId, previousStartDt, previousEndExclusiveDt, cancellationToken).ConfigureAwait(false);
        var prevRestaurantRevenue = await SumCompletedRestaurantPaymentsAsync(
            businessId, previousStartDt, previousEndExclusiveDt, cancellationToken).ConfigureAwait(false);

        var totalRevenue = bookingRevenue + restaurantRevenue;
        var prevTotalRevenue = prevBookingRevenue + prevRestaurantRevenue;

        var bookingByDay = await GetDailyBookingRevenueAsync(
            businessId, periodStartDt, periodEndExclusiveDt, cancellationToken).ConfigureAwait(false);
        var restaurantByDay = await GetDailyRestaurantRevenueAsync(
            businessId, periodStartDt, periodEndExclusiveDt, cancellationToken).ConfigureAwait(false);
        var revenueTrend = BuildRevenueTrend(periodStart, periodEnd, bookingByDay, restaurantByDay);

        var rooms = await _db.Rooms
            .AsNoTracking()
            .Where(r => r.BusinessRegistrationId == businessId && !r.IsArchived)
            .Select(r => new RoomSnapshot(r.Id, r.Name, r.Quantity))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var bookingStays = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.BusinessRegistrationId == businessId && b.Status != BookingStatus.Cancelled)
            .Select(b => new BookingStaySnapshot(b.Id, b.RoomId, b.CheckInDate, b.CheckOutDate, b.Status))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var staysByRoom = bookingStays
            .GroupBy(b => b.RoomId)
            .ToDictionary(g => g.Key, g => g.Select(x => (x.CheckInDate, x.CheckOutDate)).ToList());

        var todayNext = today.AddDays(1);
        var totalUnits = rooms.Sum(r => r.Quantity);
        var bookedToday = 0;
        foreach (var room in rooms)
        {
            staysByRoom.TryGetValue(room.Id, out var stays);
            stays ??= [];
            bookedToday += RoomBookingAvailability.GetPeakOccupancyInRange(stays, today, todayNext);
        }

        var occupancyToday = totalUnits > 0
            ? Math.Round((decimal)bookedToday / totalUnits * 100m, 1)
            : 0m;

        var compareDay = today.AddMonths(-1);
        var compareNextDay = compareDay.AddDays(1);
        var bookedCompareDay = 0;
        foreach (var room in rooms)
        {
            staysByRoom.TryGetValue(room.Id, out var stays);
            stays ??= [];
            bookedCompareDay += RoomBookingAvailability.GetPeakOccupancyInRange(stays, compareDay, compareNextDay);
        }

        var occupancyCompare = totalUnits > 0
            ? Math.Round((decimal)bookedCompareDay / totalUnits * 100m, 1)
            : 0m;

        var activeStaysToday = bookingStays.Count(b =>
            b.CheckInDate <= today && today < b.CheckOutDate
            && b.Status is BookingStatus.Confirmed or BookingStatus.Completed);

        var pendingRestaurantOrders = await _db.RestaurantOrders
            .AsNoTracking()
            .CountAsync(o => o.BusinessRegistrationId == businessId && o.Status == RestaurantOrderStatus.Pending, cancellationToken)
            .ConfigureAwait(false);

        var pendingEventHallRequests = await _db.EventHallRequests
            .AsNoTracking()
            .CountAsync(r => r.BusinessRegistrationId == businessId && r.Status == EventHallRequestStatus.Pending, cancellationToken)
            .ConfigureAwait(false);

        var recentBookings = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.BusinessRegistrationId == businessId)
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .Select(b => new DashboardRecentBookingDto
            {
                Id = b.Id,
                GuestName = b.GuestName,
                RoomName = b.Room.Name,
                CheckInDate = b.CheckInDate,
                Nights = b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber,
                Status = b.Status == BookingStatus.Pending
                    ? "Pending"
                    : b.Status == BookingStatus.Confirmed
                        ? "Confirmed"
                        : b.Status == BookingStatus.Cancelled
                            ? "Cancelled"
                            : "Completed",
                TotalAmount = b.TotalAmount,
                Currency = b.Currency,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var gatewaySplit = await BuildGatewaySplitAsync(
            businessId, periodStartDt, periodEndExclusiveDt, cancellationToken).ConfigureAwait(false);

        var roomRevenue = await _db.BookingPayments
            .AsNoTracking()
            .Where(p => p.BusinessRegistrationId == businessId
                        && p.Status == BookingPaymentStatus.Completed
                        && p.CreatedAt >= periodStartDt
                        && p.CreatedAt < periodEndExclusiveDt)
            .GroupBy(p => p.Booking.RoomId)
            .Select(g => new { RoomId = g.Key, Revenue = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var revenueByRoom = roomRevenue.ToDictionary(x => x.RoomId, x => x.Revenue);
        var topRooms = rooms
            .Select(room =>
            {
                revenueByRoom.TryGetValue(room.Id, out var revenue);
                staysByRoom.TryGetValue(room.Id, out var stays);
                stays ??= [];
                var peak = RoomBookingAvailability.GetPeakOccupancyInRange(stays, periodStart, periodEndExclusive);
                var occupancy = room.Quantity > 0
                    ? Math.Round((decimal)peak / room.Quantity * 100m, 1)
                    : 0m;

                return new DashboardTopRoomDto
                {
                    RoomId = room.Id,
                    RoomName = room.Name,
                    Revenue = revenue,
                    OccupancyPercent = occupancy,
                    Currency = "NGN",
                };
            })
            .OrderByDescending(r => r.Revenue)
            .ThenByDescending(r => r.OccupancyPercent)
            .Take(5)
            .ToList();

        return new BusinessDashboardDto
        {
            BusinessName = business.BusinessName,
            Currency = "NGN",
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalRevenue = totalRevenue,
            BookingRevenue = bookingRevenue,
            RestaurantRevenue = restaurantRevenue,
            TotalRevenueChangePercent = ComputeChangePercent(totalRevenue, prevTotalRevenue),
            OccupancyRatePercent = occupancyToday,
            OccupancyChangePercent = ComputeChangePercent(occupancyToday, occupancyCompare),
            TotalRoomUnits = totalUnits,
            RoomTypesCount = rooms.Count,
            ActiveStaysToday = activeStaysToday,
            PendingRestaurantOrders = pendingRestaurantOrders,
            PendingEventHallRequests = pendingEventHallRequests,
            RevenueTrend = revenueTrend,
            RecentBookings = recentBookings,
            GatewaySplit = gatewaySplit,
            TopRooms = topRooms,
        };
    }

    private async Task<decimal> SumCompletedBookingPaymentsAsync(
        Guid businessId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return await _db.BookingPayments
            .AsNoTracking()
            .Where(p => p.BusinessRegistrationId == businessId
                        && p.Status == BookingPaymentStatus.Completed
                        && p.CreatedAt >= from
                        && p.CreatedAt < to)
            .SumAsync(p => p.Amount, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<decimal> SumCompletedRestaurantPaymentsAsync(
        Guid businessId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return await _db.RestaurantOrderPayments
            .AsNoTracking()
            .Where(p => p.BusinessRegistrationId == businessId
                        && p.Status == BookingPaymentStatus.Completed
                        && p.CreatedAt >= from
                        && p.CreatedAt < to)
            .SumAsync(p => p.Amount, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<Dictionary<DateOnly, decimal>> GetDailyBookingRevenueAsync(
        Guid businessId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var rows = await _db.BookingPayments
            .AsNoTracking()
            .Where(p => p.BusinessRegistrationId == businessId
                        && p.Status == BookingPaymentStatus.Completed
                        && p.CreatedAt >= from
                        && p.CreatedAt < to)
            .Select(p => new { p.CreatedAt, p.Amount })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .GroupBy(r => DateOnly.FromDateTime(r.CreatedAt.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
    }

    private async Task<Dictionary<DateOnly, decimal>> GetDailyRestaurantRevenueAsync(
        Guid businessId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var rows = await _db.RestaurantOrderPayments
            .AsNoTracking()
            .Where(p => p.BusinessRegistrationId == businessId
                        && p.Status == BookingPaymentStatus.Completed
                        && p.CreatedAt >= from
                        && p.CreatedAt < to)
            .Select(p => new { p.CreatedAt, p.Amount })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .GroupBy(r => DateOnly.FromDateTime(r.CreatedAt.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
    }

    private static List<DashboardRevenueTrendPointDto> BuildRevenueTrend(
        DateOnly periodStart,
        DateOnly periodEnd,
        Dictionary<DateOnly, decimal> bookingByDay,
        Dictionary<DateOnly, decimal> restaurantByDay)
    {
        var points = new List<DashboardRevenueTrendPointDto>();

        for (var day = periodStart; day <= periodEnd; day = day.AddDays(1))
        {
            bookingByDay.TryGetValue(day, out var booking);
            restaurantByDay.TryGetValue(day, out var restaurant);

            points.Add(new DashboardRevenueTrendPointDto
            {
                Date = day,
                BookingRevenue = booking,
                RestaurantRevenue = restaurant,
                TotalRevenue = booking + restaurant,
            });
        }

        return points;
    }

    private async Task<IReadOnlyList<DashboardGatewaySplitDto>> BuildGatewaySplitAsync(
        Guid businessId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var bookingRows = await _db.BookingPayments
            .AsNoTracking()
            .Where(p => p.BusinessRegistrationId == businessId
                        && p.Status == BookingPaymentStatus.Completed
                        && p.CreatedAt >= from
                        && p.CreatedAt < to)
            .Select(p => new { p.Amount, p.Method, p.Gateway })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var restaurantRows = await _db.RestaurantOrderPayments
            .AsNoTracking()
            .Where(p => p.BusinessRegistrationId == businessId
                        && p.Status == BookingPaymentStatus.Completed
                        && p.CreatedAt >= from
                        && p.CreatedAt < to)
            .Select(p => new { p.Amount, p.Method, p.Gateway })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        void AddRows(IEnumerable<(decimal Amount, BookingPaymentMethod Method, PaymentGatewayProvider Gateway)> rows)
        {
            foreach (var row in rows)
            {
                var label = ResolvePaymentChannelLabel(row.Method, row.Gateway);
                totals.TryGetValue(label, out var current);
                totals[label] = current + row.Amount;
            }
        }

        AddRows(bookingRows.Select(r => (r.Amount, r.Method, r.Gateway)));
        AddRows(restaurantRows.Select(r => (r.Amount, r.Method, r.Gateway)));

        var grandTotal = totals.Values.Sum();
        if (grandTotal <= 0)
        {
            return [];
        }

        return totals
            .OrderByDescending(kv => kv.Value)
            .Select(kv => new DashboardGatewaySplitDto
            {
                Label = kv.Key,
                Amount = kv.Value,
                Percent = Math.Round(kv.Value / grandTotal * 100m, 1),
            })
            .ToList();
    }

    private static string ResolvePaymentChannelLabel(BookingPaymentMethod method, PaymentGatewayProvider gateway)
    {
        if (gateway != PaymentGatewayProvider.None)
        {
            return gateway switch
            {
                PaymentGatewayProvider.Paystack => "Paystack",
                PaymentGatewayProvider.Flutterwave => "Flutterwave",
                PaymentGatewayProvider.Monify => "Monnify",
                _ => gateway.ToString(),
            };
        }

        return method switch
        {
            BookingPaymentMethod.Cash => "Cash",
            BookingPaymentMethod.BankTransfer => "Bank transfer",
            BookingPaymentMethod.Gateway => "Online",
            _ => "Other",
        };
    }

    private static DateTimeOffset ToUtcOffset(DateOnly date) =>
        new(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

    private static decimal? ComputeChangePercent(decimal current, decimal previous)
    {
        if (previous == 0m)
        {
            return current > 0m ? 100m : null;
        }

        return Math.Round((current - previous) / previous * 100m, 1);
    }

    private sealed record RoomSnapshot(Guid Id, string Name, int Quantity);

    private sealed record BookingStaySnapshot(
        Guid Id,
        Guid RoomId,
        DateOnly CheckInDate,
        DateOnly CheckOutDate,
        BookingStatus Status);
}
