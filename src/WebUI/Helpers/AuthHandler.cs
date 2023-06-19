using System.Net.Http.Headers;

namespace WebUI.Helpers;

public class AuthHandler : DelegatingHandler
{
    public const string AuthenticatedClient = nameof(AuthenticatedClient);
    
    private static string _accessToken = string.Empty;

    public static void SetAccessToken(string token)
    {
        Console.WriteLine($"[AuthHandler] Setting access token {token}");
        _accessToken = token;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}