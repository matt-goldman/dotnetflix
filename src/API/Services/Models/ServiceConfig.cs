namespace DotNetFlix.API.Services.Models;

public class ServiceConfig
{
    public ClientCredentials VideosClient { get; set; } = new();
    public ClientCredentials SubscriptionsClient { get; set; } = new();
}
