using DotNetFlix.UI.Services;
using System.Net.Http.Headers;

namespace DotNetFlix.UI.Helpers;

public class AuthHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public AuthHandler(AuthService authService)
    {
        _authService = authService;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authService.GetToken();

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

        return await base.SendAsync(request, cancellationToken);
    }
}
