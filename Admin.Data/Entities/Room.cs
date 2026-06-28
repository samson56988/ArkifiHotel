namespace Admin.Data.Entities;

public class Room
{
    public Guid Id { get; set; }

    public Guid BusinessRegistrationId { get; set; }

    public BusinessRegistration BusinessRegistration { get; set; } = null!;

    public Guid? LocationId { get; set; }

    public BusinessLocation? Location { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>Short marketing line shown on shortlet listing cards (e.g. "Floor-to-ceiling windows & city skyline").</summary>
    public string? Tagline { get; set; }

    public string? Description { get; set; }

    public int MaxOccupancy { get; set; }

    /// <summary>Bedrooms — used for shortlet listings.</summary>
    public int? BedroomCount { get; set; }

    /// <summary>Bathrooms — used for shortlet listings.</summary>
    public int? BathroomCount { get; set; }

    /// <summary>When true, the listing shows a "Guest favorite" badge on the shortlet storefront.</summary>
    public bool IsGuestFavorite { get; set; }

    public decimal BasePricePerNight { get; set; }

    /// <summary>Optional weekly package rate for shortlet stays (7+ nights).</summary>
    public decimal? BasePricePerWeek { get; set; }

    /// <summary>How many physical rooms of this type exist (e.g. 4 Executive rooms).</summary>
    public int Quantity { get; set; } = 1;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>When true, the room is hidden from the default inventory list until restored or permanently deleted.</summary>
    public bool IsArchived { get; set; }

    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();

    public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
}
