using Microsoft.AspNetCore.Identity.UI.Services;

namespace DotNetFlix.Identity.Services;

public class EmailService : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        throw new NotImplementedException();
    }
}
