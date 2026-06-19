using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class BusinessRestaurantOrderService : IBusinessRestaurantOrderService
{
    private readonly AdminDbContext _db;

    public BusinessRestaurantOrderService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<RestaurantOrderListResultDto> ListAsync(
        Guid businessId,
        int page,
        int pageSize,
        string? status,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _db.RestaurantOrders
            .AsNoTracking()
            .Where(o => o.BusinessRegistrationId == businessId);

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<RestaurantOrderStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(o => o.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        var rows = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.GuestType,
                o.RoomNumber,
                o.GuestPhone,
                o.Status,
                o.TotalAmount,
                o.Currency,
                o.CreatedAt,
                ItemCount = o.Lines.Sum(l => l.Quantity),
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new RestaurantOrderListResultDto
        {
            Items = rows.Select(o => new RestaurantOrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                GuestType = o.GuestType == RestaurantGuestType.RoomGuest ? "roomGuest" : "inRestaurant",
                RoomNumber = o.RoomNumber,
                GuestPhone = o.GuestPhone,
                Status = o.Status.ToString(),
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                ItemCount = o.ItemCount,
                CreatedAt = o.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
        };
    }

    public async Task<RestaurantOrderDetailDto?> GetAsync(
        Guid businessId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _db.RestaurantOrders
            .AsNoTracking()
            .Include(o => o.Lines)
            .Include(o => o.Location)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BusinessRegistrationId == businessId, cancellationToken)
            .ConfigureAwait(false);

        if (order is null)
        {
            return null;
        }

        return new RestaurantOrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            GuestType = order.GuestType == RestaurantGuestType.RoomGuest ? "roomGuest" : "inRestaurant",
            RoomNumber = order.RoomNumber,
            GuestPhone = order.GuestPhone,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            LocationName = order.Location?.Name,
            CreatedAt = order.CreatedAt,
            Lines = order.Lines
                .OrderBy(l => l.ItemName)
                .Select(l => new GuestRestaurantOrderLineDto
                {
                    ItemName = l.ItemName,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal,
                })
                .ToList(),
        };
    }
}
