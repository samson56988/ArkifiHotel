namespace Admin.Data.Constants;

/// <summary>Permission module codes for organization users.</summary>
public static class OrganizationModuleCodes
{
    public const string Dashboard = "dashboard";
    public const string Rooms = "rooms";
    public const string Locations = "locations";
    public const string Amenities = "amenities";
    public const string Facilities = "facilities";
    public const string EventHalls = "event_halls";
    public const string RestaurantMenu = "restaurant_menu";
    public const string RestaurantOrders = "restaurant_orders";
    public const string Bookings = "bookings";
    public const string PaymentConfiguration = "payment_configuration";
    public const string Customers = "customers";
    public const string BookingPayments = "booking_payments";
    public const string Subscription = "subscription";
    public const string Profile = "profile";
    public const string SocialProfile = "social_profile";
    public const string StorefrontDesigner = "storefront_designer";
    public const string Team = "team";
    public const string Audit = "audit";

    public static readonly IReadOnlyList<string> Shared =
    [
        Dashboard,
        Rooms,
        Locations,
        Amenities,
        Facilities,
        Bookings,
        PaymentConfiguration,
        Customers,
        BookingPayments,
        Subscription,
        Profile,
        SocialProfile,
        StorefrontDesigner,
    ];

    public static readonly IReadOnlyList<string> HotelOnly =
    [
        EventHalls,
        RestaurantMenu,
        RestaurantOrders,
    ];

    public static readonly IReadOnlyList<string> SuperAdminOnly = [Team, Audit];

    public static IReadOnlyList<string> ForBusinessType(bool isShortlet)
    {
        if (isShortlet)
        {
            return Shared;
        }

        var list = new List<string>(Shared.Count + HotelOnly.Count);
        list.AddRange(Shared);
        list.AddRange(HotelOnly);
        return list;
    }

    public static bool IsValidForBusiness(string moduleCode, bool isShortlet)
    {
        if (SuperAdminOnly.Contains(moduleCode, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        return ForBusinessType(isShortlet).Contains(moduleCode, StringComparer.OrdinalIgnoreCase);
    }
}
