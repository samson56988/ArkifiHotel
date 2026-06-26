using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Infrastructure.Helpers;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Dtos;

namespace Admin.Infrastructure.Services;

public sealed class PublicEventHallRequestService : IPublicEventHallRequestService
{
    private readonly AdminDbContext _db;

    public PublicEventHallRequestService(AdminDbContext db)
    {
        _db = db;
    }

    public async Task<(GuestEventHallRequestResultDto? Data, string? Error)> CreateRequestAsync(
        string slug,
        GuestCreateEventHallRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalized = BusinessSlugHelper.Normalize(slug);
        if (string.IsNullOrEmpty(normalized))
        {
            return (null, "Storefront not found.");
        }

        var business = await _db.BusinessRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Slug == normalized, cancellationToken)
            .ConfigureAwait(false);

        if (business is null || !SubscriptionAccessHelper.IsStorefrontAccessible(business))
        {
            return (null, "Storefront not found.");
        }

        if (request.LocationId == Guid.Empty || request.EventHallId == Guid.Empty)
        {
            return (null, "Location and event hall are required.");
        }

        if (!ValidateGuestName(request.GuestName, out var guestName))
        {
            return (null, "Enter a valid guest name.");
        }

        if (!ValidateEmail(request.GuestEmail, out var guestEmail))
        {
            return (null, "Enter a valid email address.");
        }

        if (!ValidatePhone(request.GuestPhone, out var guestPhone))
        {
            return (null, "Enter a valid phone number.");
        }

        if (request.EventEndDate.HasValue && request.EventEndDate.Value < request.EventDate)
        {
            return (null, "End date cannot be before start date.");
        }

        var locationOk = await _db.BusinessLocations
            .AsNoTracking()
            .AnyAsync(
                l => l.Id == request.LocationId && l.BusinessRegistrationId == business.Id,
                cancellationToken)
            .ConfigureAwait(false);

        if (!locationOk)
        {
            return (null, "Invalid branch for this hotel.");
        }

        var hall = await _db.EventHalls
            .AsNoTracking()
            .FirstOrDefaultAsync(
                h => h.Id == request.EventHallId
                    && h.BusinessRegistrationId == business.Id
                    && h.LocationId == request.LocationId
                    && !h.IsArchived,
                cancellationToken)
            .ConfigureAwait(false);

        if (hall is null)
        {
            return (null, "Event hall not found.");
        }

        if (!ValidateEventPurpose(request.EventPurpose, out var eventPurpose))
        {
            return (null, "Describe the purpose of your event (e.g. wedding, conference).");
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes?.Length > 2000)
        {
            notes = notes[..2000];
        }

        var entity = new EventHallRequest
        {
            Id = Guid.NewGuid(),
            BusinessRegistrationId = business.Id,
            LocationId = request.LocationId,
            EventHallId = request.EventHallId,
            GuestName = guestName,
            GuestEmail = guestEmail,
            GuestPhone = guestPhone,
            EventDate = request.EventDate,
            EventEndDate = request.EventEndDate,
            EventPurpose = eventPurpose,
            Notes = notes,
            Status = EventHallRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        _db.EventHallRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (
            new GuestEventHallRequestResultDto
            {
                RequestId = entity.Id,
                Status = "Pending",
                Message =
                    "Your request has been submitted. Our events team will review availability and contact you shortly — no payment is required at this stage.",
            },
            null);
    }

    private static bool ValidateGuestName(string? name, out string trimmed)
    {
        trimmed = name?.Trim() ?? string.Empty;
        return trimmed.Length is >= 2 and <= 200;
    }

    private static bool ValidateEmail(string? email, out string trimmed)
    {
        trimmed = email?.Trim() ?? string.Empty;
        if (trimmed.Length is < 5 or > 320)
        {
            return false;
        }

        var at = trimmed.IndexOf('@');
        return at > 0 && at < trimmed.Length - 1 && trimmed.IndexOf('.', at + 1) >= 0;
    }

    private static bool ValidatePhone(string? phone, out string trimmed)
    {
        trimmed = phone?.Trim() ?? string.Empty;
        var digits = trimmed.Count(char.IsDigit);
        return digits >= 7 && trimmed.Length <= 30;
    }

    private static bool ValidateEventPurpose(string? purpose, out string trimmed)
    {
        trimmed = purpose?.Trim() ?? string.Empty;
        return trimmed.Length is >= 3 and <= 200;
    }
}
