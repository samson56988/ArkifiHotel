using Admin.Data.Entities;
using Admin.Services.Abstractions;

namespace Admin.Infrastructure.Helpers;

public static class OrganizationQueryScope
{
    public static IQueryable<Room> ApplyRoomScope(IQueryable<Room> query, IOrganizationUserContext context)
    {
        if (context.IsSuperAdmin || context.HasAllLocationAccess)
        {
            return query;
        }

        if (context.LocationIds.Count == 0)
        {
            return query.Where(_ => false);
        }

        var allowed = context.LocationIds;
        return query.Where(r => r.LocationId.HasValue && allowed.Contains(r.LocationId.Value));
    }

    public static IQueryable<Booking> ApplyBookingScope(IQueryable<Booking> query, IOrganizationUserContext context)
    {
        if (context.IsSuperAdmin || context.HasAllLocationAccess)
        {
            return query;
        }

        if (context.LocationIds.Count == 0)
        {
            return query.Where(_ => false);
        }

        var allowed = context.LocationIds;
        return query.Where(b => b.LocationId.HasValue && allowed.Contains(b.LocationId.Value));
    }

    public static IQueryable<BookingPayment> ApplyBookingPaymentScope(
        IQueryable<BookingPayment> query,
        IOrganizationUserContext context)
    {
        if (context.IsSuperAdmin || context.HasAllLocationAccess)
        {
            return query;
        }

        if (context.LocationIds.Count == 0)
        {
            return query.Where(_ => false);
        }

        var allowed = context.LocationIds;
        return query.Where(p => p.Booking.LocationId.HasValue && allowed.Contains(p.Booking.LocationId!.Value));
    }

    public static IQueryable<BusinessLocation> ApplyLocationScope(
        IQueryable<BusinessLocation> query,
        IOrganizationUserContext context)
    {
        if (context.IsSuperAdmin || context.HasAllLocationAccess)
        {
            return query;
        }

        if (context.LocationIds.Count == 0)
        {
            return query.Where(_ => false);
        }

        var allowed = context.LocationIds;
        return query.Where(l => allowed.Contains(l.Id));
    }

    public static IQueryable<EventHall> ApplyEventHallScope(IQueryable<EventHall> query, IOrganizationUserContext context)
    {
        if (context.IsSuperAdmin || context.HasAllLocationAccess)
        {
            return query;
        }

        if (context.LocationIds.Count == 0)
        {
            return query.Where(_ => false);
        }

        var allowed = context.LocationIds;
        return query.Where(h => allowed.Contains(h.LocationId));
    }

    public static IQueryable<PropertyFacility> ApplyFacilityScope(
        IQueryable<PropertyFacility> query,
        IOrganizationUserContext context)
    {
        if (context.IsSuperAdmin || context.HasAllLocationAccess)
        {
            return query;
        }

        if (context.LocationIds.Count == 0)
        {
            return query.Where(_ => false);
        }

        var allowed = context.LocationIds;
        return query.Where(f => f.LocationId.HasValue && allowed.Contains(f.LocationId.Value));
    }
}
