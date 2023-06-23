using System.Text.Json;
using DotNetFlix.API.Services.Models;

namespace DotNetFlix.API.Services;

public class BaseService
{
    protected readonly HttpClient _httpClient;

    public const string IdentityClient = nameof(IdentityClient);

    public BaseService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(IdentityClient));
    }
    
    protected async Task<string> GetApiToken(ClientCredentials credentials)
    {
        string token;

        var paramVals = new Dictionary<string, string>();
        
        string grantType = "client_credentials";

        paramVals.Add("client_id", credentials.ClientId);
        paramVals.Add("scope", credentials.Scope);
        paramVals.Add("client_secret", credentials.ClientSecret);
        paramVals.Add("grant_type", grantType);


        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.PostAsync("/oauth2/v2.0/token", new FormUrlEncodedContent(paramVals));

        if (response.IsSuccessStatusCode)
        {
            var stringContent = await response.Content.ReadAsStringAsync();

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(stringContent);

            token = tokenResponse?.access_token??"";
        }
        else
        {
            throw new Exception("Error getting client credentials token");
        }

        return token;
    }
}
