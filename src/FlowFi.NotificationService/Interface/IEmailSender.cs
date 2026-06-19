namespace FlowFi.NotificationService.Interface;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
}
