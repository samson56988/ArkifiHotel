using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Shared.Data.Emails;
using Shared.Services.Abstractions;
using Shared.Services.Options;

namespace Shared.Services.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _smtp;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpSettings> smtpOptions, ILogger<SmtpEmailSender> logger)
    {
        _smtp = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_smtp.Server) ||
            string.IsNullOrWhiteSpace(_smtp.SenderEmail))
        {
            throw new InvalidOperationException("SMTP settings are incomplete. Check SmtpSettings in configuration.");
        }

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_smtp.SenderName, _smtp.SenderEmail));
        mime.To.Add(new MailboxAddress(message.ToName ?? message.ToEmail, message.ToEmail));
        mime.Subject = message.Subject;
        mime.Body = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody ?? "Please view this email in an HTML-compatible client.",
        }.ToMessageBody();

        using var client = new SmtpClient();
        var secureSocket = _smtp.Ssl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

        await client.ConnectAsync(_smtp.Server, _smtp.Port, secureSocket, cancellationToken).ConfigureAwait(false);
        if (!_smtp.DefaultCredentials)
        {
            await client.AuthenticateAsync(_smtp.SenderEmail, _smtp.Password, cancellationToken).ConfigureAwait(false);
        }

        await client.SendAsync(mime, cancellationToken).ConfigureAwait(false);
        await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Onboarding email sent to {Email}", message.ToEmail);
    }
}
