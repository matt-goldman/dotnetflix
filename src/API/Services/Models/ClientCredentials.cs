namespace DotNetFlix.API.Services.Models;

public class ClientCredentials
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;
}
