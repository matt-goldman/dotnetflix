using System.Diagnostics;
using System.Net.Http.Json;

namespace DotNetFlix.UI.Services;

public class AuthService
{
    const string ClientId = "device";/*maui-b2c "daea0fc2-c44f-422e-af7e-07b51237668d";*/ // goldienote - "5b3ae15d-0f8f-4f2e-85bd-72baee779aa3";//will - "68f5b5ac-7d8a-4537-9145-cf599e2f74b7";

    private readonly HttpClient _httpClient;

    private string _deviceCode;

    public AuthService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient();
    }

    public async Task<DeviceCodeResponse> GetUserCode()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://dc02-159-196-124-207.ngrok.io/connect/deviceauthorization");//"https://login.microsoftonline.com/common/oauth2/v2.0/devicecode");
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", ClientId),
            new KeyValuePair<string, string>("scope", "scope2 profile openid")
        });

        request.Content = content;

        var response = await _httpClient.SendAsync(request);

        var deviceCodeResponse = await response.Content.ReadFromJsonAsync<DeviceCodeResponse>();

        _deviceCode = deviceCodeResponse.device_code;

        return deviceCodeResponse;
    }

    public async Task<TokenResponse> GetToken()
    {


        var stopwatch = new Stopwatch();

        stopwatch.Start();

        var request = new HttpRequestMessage(HttpMethod.Post, "https://dc02-159-196-124-207.ngrok.io/connect/token"); //"https://login.microsoftonline.com/common/oauth2/v2.0/token");
        var content = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code"),
                new KeyValuePair<string, string>("device_code", _deviceCode)
            });

        request.Content = content;

        while (stopwatch.ElapsedMilliseconds < 900000) // 900000ms is 15 minutes which is the max validity for the user code
        {
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                    stopwatch.Stop();

                    return tokenResponse;
                }
                catch (Exception ex)
                {
                    // If the response is not a successful response, it will throw a JsonException
                    // We can ignore this exception and continue polling for the token
                    throw;
                }
            }
            else
            {
                try
                {
                    var tokenRespnse = await response.Content.ReadFromJsonAsync<ErrorResponse>();

                    if (tokenRespnse.error == "authorization_pending")
                    {
                        await Task.Delay(5000);
                        continue;
                    }
                    else
                    {
                        throw new Exception(tokenRespnse.error_description);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        }

        stopwatch.Stop();

        throw new Exception("The user code has expired");
    }
}
