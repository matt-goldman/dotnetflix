using DotNetFlix.API.Services.Models;
using Microsoft.Extensions.Options;

namespace DotNetFlix.API.Services;

public class SubscriptionsService : BaseService
{
    public const string SubscriptionsClient = nameof(SubscriptionsClient);
    
    private readonly ClientCredentials _clientCredentials;

    private DateTime _tokenExpires = DateTime.UtcNow;
    
    public SubscriptionsService(IHttpClientFactory httpClientFactory, IOptions<ServiceConfig> config) : base(httpClientFactory)
    {
        _clientCredentials = config.Value.SubscriptionsClient;
    }

    public async Task<bool> SubscriberIsPremium(string subscriberName)
    {
        return await Task.FromResult(true);
    }
}
