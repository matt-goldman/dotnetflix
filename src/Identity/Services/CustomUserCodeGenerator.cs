using Duende.IdentityServer.Services;
using Duende.IdentityServer;

namespace DotNetFlix.Identity.Services;

public sealed class CustomUserCodeGenerator : IUserCodeGenerator
{
    // This can be set to other types for testing, but the DeviceCodeOptions
    // in IdentityServer specifies this Numeric type (it's the only type
    // defined). 
    public string UserCodeType => IdentityServerConstants.UserCodeTypes.Numeric;

    public int RetryLimit => 5;

    readonly Random rnd = new Random();

    public Task<string> GenerateAsync()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string randomString = new string(Enumerable.Repeat(chars, 5)
            .Select(s => s[rnd.Next(s.Length)]).ToArray());

        return Task.FromResult(randomString);
    }
}
