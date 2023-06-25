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
        var subscription = await _subscriptionsClient.GetFromJsonAsync<Subscription>($"subscription?subscriberName={subscriberName}");

        return subscription.isPremium;
    }
}

internal record Subscription(string subscriberName, bool isPremium);