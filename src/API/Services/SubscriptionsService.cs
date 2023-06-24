namespace DotNetFlix.API.Services;

public class SubscriptionsService
{
    public const string SubscriptionsClient = nameof(SubscriptionsClient);

    private readonly HttpClient _subscriptionsClient;

    public SubscriptionsService(IHttpClientFactory httpClientFactory)
    {
        _subscriptionsClient = httpClientFactory.CreateClient(SubscriptionsClient);
    }

    public async Task<bool> SubscriberIsPremium(string subscriberName)
    {
        var isPremium = await _subscriptionsClient.GetFromJsonAsync<bool>($"subscriptions/{subscriberName}");

        return isPremium;
    }
}
