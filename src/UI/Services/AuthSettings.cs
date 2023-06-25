namespace DotNetFlix.UI.Services;

public class AuthSettings
{
    public string DeviceCodeEndpoint { get; set; }

    public string TokenEndpoint { get; set; }

    public string ClientId { get; set; }

    public string BaseUrl { get; set; }

    public string Scopes { get; set; }
}
