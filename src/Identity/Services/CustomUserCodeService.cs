using Duende.IdentityServer.Services;

namespace DotNetFlix.Identity.Services;

public sealed class CustomUserCodeService : IUserCodeService
{
    private IUserCodeGenerator? _generator;

    public Task<IUserCodeGenerator> GetGenerator(string userCodeType)
    {
        if (_generator is null)
        {
            _generator = new CustomUserCodeGenerator();
        }

        return Task.FromResult(_generator);
    }
}