using System.Net.Http.Json;

namespace DotNetFlix.UI.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    private string _deviceCode;
    private DateTime _deviceCodeExpires = DateTime.UtcNow;

    public AuthSettings _authSettings { get; }

    private TokenResponse _currentToken;
    private DateTime _tokenExpiresAt = DateTime.UtcNow;

    private readonly HttpRequestMessage _tokenRequest;

    private readonly HttpRequestMessage _codeRequest;

    public AuthService(IHttpClientFactory clientFactory, AuthSettings authSettings)
    {
        _httpClient = clientFactory.CreateClient();
        _authSettings = authSettings;

        _tokenRequest = GenerateTokenRequest();
        _codeRequest = GenerateCodeRequest();
    }

    private HttpRequestMessage GenerateTokenRequest()
    {
        // Generate a new token request
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_authSettings.BaseUrl}{_authSettings.TokenEndpoint}");
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _authSettings.ClientId),
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code"),
            new KeyValuePair<string, string>("device_code", _deviceCode)
        });

        request.Content = content;

        return request;
    }

    private HttpRequestMessage GenerateCodeRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_authSettings.BaseUrl}{_authSettings.DeviceCodeEndpoint}");
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _authSettings.ClientId),
            new KeyValuePair<string, string>("scope", _authSettings.Scopes)
        });

        request.Content = content;

        return request;
    }

    public async Task<DeviceCodeResponse> GetUserCode()
    {
        var response = await _httpClient.SendAsync(_codeRequest);

        var deviceCodeResponse = await response.Content.ReadFromJsonAsync<DeviceCodeResponse>();

        _deviceCode = deviceCodeResponse.device_code;
        _deviceCodeExpires = DateTime.UtcNow.AddSeconds(deviceCodeResponse.expires_in);

        return deviceCodeResponse;
    }

    public async Task<TokenResponse> GetToken()
    {
        // Check to see if the current token is still valid
        if (_currentToken != null || _tokenExpiresAt > DateTime.UtcNow.AddMinutes(1))
        {
            return _currentToken;
        }
        
        while (DateTime.UtcNow < _deviceCodeExpires)
        {
            var response = await _httpClient.SendAsync(_tokenRequest);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

                _currentToken = tokenResponse;
                _tokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);

                // Mark the device code as expired
                _deviceCodeExpires = DateTime.UtcNow.AddSeconds(-100);

                return tokenResponse;
            }
            else
            {
                var tokenRespnse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

                if (tokenRespnse.error == "authorization_pending")
                {
                    // The user code has not been authorised yet
                    // Requests are throttled - if you try to get the code too frequently you will be banned
                    // try again in 5 seconds
                    await Task.Delay(5000);
                    continue;
                }
                else
                {
                    throw new Exception(tokenRespnse.error_description);
                }
            }

        }

        throw new Exception("The user code has expired");
    }
}
