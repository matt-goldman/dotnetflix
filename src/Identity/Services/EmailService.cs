using Microsoft.AspNetCore.Identity.UI.Services;

namespace DotNetFlix.Identity.Services;

public class EmailService : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        Console.WriteLine($"Sending email to {email} with subject {subject} and message {htmlMessage}");
        return Task.CompletedTask;
    }
}
