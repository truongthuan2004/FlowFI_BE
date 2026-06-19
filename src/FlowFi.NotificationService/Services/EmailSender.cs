using System.Net;
using System.Net.Mail;
using FlowFi.NotificationService.Interface;
using Microsoft.Extensions.Configuration;

namespace FlowFi.NotificationService.Services;

public sealed class EmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        var host = configuration["Smtp:Host"] ?? "smtp.gmail.com";
        var port = int.Parse(configuration["Smtp:Port"] ?? "587");
        var username = configuration["Smtp:Username"] ?? throw new InvalidOperationException("Smtp:Username is not configured.");
        var password = configuration["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password is not configured.");
        var fromAddress = configuration["Smtp:FromAddress"] ?? username;
        var fromName = configuration["Smtp:FromName"] ?? "FlowFi Support";

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage, cancellationToken);
    }
}
