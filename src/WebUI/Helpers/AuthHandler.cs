using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Headers;

namespace WebUI.Helpers;

public class AuthHandler : DelegatingHandler
{
    public const string AuthenticatedClient = nameof(AuthenticatedClient);
    
    public AuthHandler(IAccessTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public IAccessTokenProvider _tokenProvider { get; }

    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenResult = await _tokenProvider.RequestAccessToken();
        string token;

        if (tokenResult.TryGetToken(out var accessToken))
        {
            token = accessToken.Value;
        }
        else
        {
            throw new InvalidOperationException("Couldn't get access token");
        }
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}