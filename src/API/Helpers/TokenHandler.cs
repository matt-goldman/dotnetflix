using DotNetFlix.API.Services.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DotNetFlix.API.Helpers;

public class TokenHandler : DelegatingHandler
{
    public const string IdentityClient = nameof(IdentityClient);
    
    private readonly HttpClient _httpClient;

    private string _videosToken = string.Empty;
    private DateTime _videosTokenExpires = DateTime.UtcNow;
    Uri _videosUri;

    private string _subscriptionsToken = string.Empty;
    private DateTime _subscriptionsTokenExpires = DateTime.UtcNow;
    Uri _subscriptionsUri;
    

    public TokenHandler(IHttpClientFactory httpClientFactory, IOptions<ServiceConfig> options)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(IdentityClient));
        _serviceConfig = options.Value;

        _videosUri = new Uri(_serviceConfig.VideosClient.BaseUrl);
        _subscriptionsUri = new Uri(_serviceConfig.SubscriptionsClient.BaseUrl);
    }

    public ServiceConfig _serviceConfig { get; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = string.Empty;
        
        if (request.RequestUri!.Host == _videosUri.Host && request.RequestUri!.Port == _videosUri.Port)
        {
            if (string.IsNullOrEmpty(_videosToken) || _videosTokenExpires < DateTime.UtcNow.AddMinutes(1))
            {
                var tokenResult = await GetApiToken(_serviceConfig.VideosClient);
                _videosToken = tokenResult.Token;
                _videosTokenExpires = tokenResult.Expires;
            }

            token = _videosToken;
        }
        else if (request.RequestUri.Host == _subscriptionsUri.Host && request.RequestUri.Port == _subscriptionsUri.Port)
        {
            if (string.IsNullOrEmpty(_subscriptionsToken) || _subscriptionsTokenExpires < DateTime.UtcNow.AddMinutes(1))
            {
                var tokenResult = await GetApiToken(_serviceConfig.SubscriptionsClient);
                _subscriptionsToken = tokenResult.Token;
                _subscriptionsTokenExpires = tokenResult.Expires;
            }

            token = _subscriptionsToken;
        }
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
               
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<(string Token, DateTime Expires)> GetApiToken(ClientCredentials credentials)
    {
        string token;
        var expires = DateTime.UtcNow;

        var paramVals = new Dictionary<string, string>();

        string grantType = "client_credentials";

        paramVals.Add("client_id", credentials.ClientId);
        paramVals.Add("scope", credentials.Scope);
        paramVals.Add("client_secret", credentials.ClientSecret);
        paramVals.Add("grant_type", grantType);


        //_httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.PostAsync("/connect/token", new FormUrlEncodedContent(paramVals));

        if (response.IsSuccessStatusCode)
        {
            var stringContent = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(stringContent);

            token = tokenResponse?.access_token ?? "";

            expires = expires.AddSeconds(tokenResponse!.expires_in);
        }
        else
        {
            throw new Exception("Error getting client credentials token");
        }

        return (token, expires);
    }
}
