using System.Globalization;
using System.Net.Mail;
using System.Text;
using Admin.Data;
using Admin.Data.Entities;
using Admin.Data.Enums;
using Admin.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Data.Emails;
using Shared.Services.Abstractions;

namespace Admin.Infrastructure.Services;

public sealed class CustomerConfirmationEmailService : ICustomerConfirmationEmailService
{
    private const string PlatformSupportEmail = "info@arkifi.store";

    private readonly AdminDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;
    private readonly ILogger<CustomerConfirmationEmailService> _logger;

    public CustomerConfirmationEmailService(
        AdminDbContext db,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer,
        ILogger<CustomerConfirmationEmailService> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Room)
            .Include(b => b.Location)
            .Include(b => b.BusinessRegistration)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            .ConfigureAwait(false);

        if (booking is null)
        {
            return;
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            return;
        }

        if (!TryNormalizeEmail(booking.GuestEmail, out var guestEmail))
        {
            _logger.LogWarning("Booking {BookingId} has no valid guest email; confirmation email skipped.", bookingId);
            return;
        }

        var nights = booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber;
        var propertyName = booking.BusinessRegistration.BusinessName;
        var supportEmail = string.IsNullOrWhiteSpace(booking.BusinessRegistration.ContactEmail)
            ? PlatformSupportEmail
            : booking.BusinessRegistration.ContactEmail.Trim();

        var locationRow = string.IsNullOrWhiteSpace(booking.Location?.Name)
            ? string.Empty
            : $"""<tr><td style="padding:4px 0;"><strong>Branch:</strong> {Encode(booking.Location.Name)}</td></tr>""";

        var html = _templateRenderer.Render(
            "BookingConfirmation",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["GuestName"] = booking.GuestName,
                ["PropertyName"] = propertyName,
                ["ConfirmationCode"] = booking.ConfirmationCode,
                ["RoomName"] = booking.Room.Name,
                ["LocationRow"] = locationRow,
                ["CheckInDate"] = FormatDate(booking.CheckInDate),
                ["CheckOutDate"] = FormatDate(booking.CheckOutDate),
                ["Nights"] = nights.ToString(CultureInfo.InvariantCulture),
                ["TotalAmount"] = FormatMoney(booking.TotalAmount, booking.Currency),
                ["SupportEmail"] = supportEmail,
                ["Year"] = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture),
            });

        var text = new StringBuilder()
            .AppendLine($"Hi {booking.GuestName},")
            .AppendLine()
            .AppendLine($"Your booking at {propertyName} is confirmed.")
            .AppendLine($"Confirmation code: {booking.ConfirmationCode}")
            .AppendLine($"Room: {booking.Room.Name}")
            .AppendLine($"Check-in: {FormatDate(booking.CheckInDate)}")
            .AppendLine($"Check-out: {FormatDate(booking.CheckOutDate)}")
            .AppendLine($"Total paid: {FormatMoney(booking.TotalAmount, booking.Currency)}")
            .AppendLine()
            .AppendLine($"Questions? Contact {supportEmail}.")
            .ToString();

        await SendSafeAsync(
            guestEmail,
            booking.GuestName,
            $"Booking confirmed — {propertyName} ({booking.ConfirmationCode})",
            html,
            text,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task SendRestaurantOrderConfirmationAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _db.RestaurantOrders
            .AsNoTracking()
            .Include(o => o.Lines)
            .Include(o => o.BusinessRegistration)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            .ConfigureAwait(false);

        if (order is null)
        {
            return;
        }

        if (order.Status != RestaurantOrderStatus.Paid)
        {
            return;
        }

        if (!TryNormalizeEmail(order.GuestEmail, out var guestEmail))
        {
            _logger.LogWarning("Restaurant order {OrderId} has no valid guest email; confirmation email skipped.", orderId);
            return;
        }

        var propertyName = order.BusinessRegistration.BusinessName;
        var supportEmail = string.IsNullOrWhiteSpace(order.BusinessRegistration.ContactEmail)
            ? PlatformSupportEmail
            : order.BusinessRegistration.ContactEmail.Trim();

        var guestTypeLabel = order.GuestType == RestaurantGuestType.RoomGuest ? "Room guest" : "In restaurant";
        var roomRow = order.GuestType == RestaurantGuestType.RoomGuest && !string.IsNullOrWhiteSpace(order.RoomNumber)
            ? $"""<tr><td style="padding:4px 0;"><strong>Room:</strong> {Encode(order.RoomNumber)}</td></tr>"""
            : string.Empty;

        var linesHtml = string.Join(
            string.Empty,
            order.Lines.OrderBy(l => l.ItemName).Select(l =>
                $"<li>{l.Quantity}× {Encode(l.ItemName)} — {Encode(FormatMoney(l.LineTotal, order.Currency))}</li>"));

        var html = _templateRenderer.Render(
            "RestaurantOrderConfirmation",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["PropertyName"] = propertyName,
                ["OrderNumber"] = order.OrderNumber,
                ["GuestTypeLabel"] = guestTypeLabel,
                ["RoomRow"] = roomRow,
                ["GuestPhone"] = order.GuestPhone,
                ["TotalAmount"] = FormatMoney(order.TotalAmount, order.Currency),
                ["OrderLinesHtml"] = linesHtml,
                ["SupportEmail"] = supportEmail,
                ["Year"] = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture),
            });

        var text = new StringBuilder()
            .AppendLine($"Your order at {propertyName} is confirmed.")
            .AppendLine($"Order number: {order.OrderNumber}")
            .AppendLine($"Guest type: {guestTypeLabel}")
            .AppendLine($"Phone: {order.GuestPhone}")
            .AppendLine($"Total paid: {FormatMoney(order.TotalAmount, order.Currency)}")
            .AppendLine()
            .AppendLine("Items:")
            .AppendLine(string.Join(
                Environment.NewLine,
                order.Lines.OrderBy(l => l.ItemName).Select(l =>
                    $"- {l.Quantity}× {l.ItemName} — {FormatMoney(l.LineTotal, order.Currency)}")))
            .AppendLine()
            .AppendLine($"Questions? Contact {supportEmail}.")
            .ToString();

        await SendSafeAsync(
            guestEmail,
            null,
            $"Order confirmed — {propertyName} ({order.OrderNumber})",
            html,
            text,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task SendSafeAsync(
        string toEmail,
        string? toName,
        string subject,
        string html,
        string text,
        CancellationToken cancellationToken)
    {
        try
        {
            await _emailSender.SendAsync(
                new EmailMessage
                {
                    ToEmail = toEmail,
                    ToName = toName,
                    Subject = subject,
                    HtmlBody = html,
                    TextBody = text,
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Customer confirmation email failed for {Email}.", toEmail);
        }
    }

    private static bool TryNormalizeEmail(string? raw, out string normalized)
    {
        normalized = (raw ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > 320)
        {
            return false;
        }

        try
        {
            _ = new MailAddress(normalized);
            return normalized.Contains('@', StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static string FormatDate(DateOnly date) =>
        date.ToString("dddd, d MMMM yyyy", CultureInfo.InvariantCulture);

    private static string FormatMoney(decimal amount, string currency)
    {
        var code = string.IsNullOrWhiteSpace(currency) ? "NGN" : currency.Trim().ToUpperInvariant();
        return $"{code} {amount:N2}";
    }

    private static string Encode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
