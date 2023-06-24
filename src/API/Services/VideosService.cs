using DotNetFlix.API.Services.Models;
using DotNetFlix.Shared;
using Microsoft.Extensions.Options;

namespace DotNetFlix.API.Services;

public class VideosService : BaseService
{
    public const string VideosClient = nameof(VideosClient);

    private readonly HttpClient _videosClient;

    private readonly ClientCredentials _clientCredentials;
    private readonly SubscriptionsService _subscriptionsService;
    private DateTime _tokenExpires = DateTime.UtcNow;

    public VideosService(IHttpClientFactory httpClientFactory, IOptions<ServiceConfig> config, SubscriptionsService subscriptionsService) : base(httpClientFactory)
    {
        _clientCredentials = config.Value.VideosClient;
        _videosClient = httpClientFactory.CreateClient(VideosClient);
        _subscriptionsService = subscriptionsService;
    }

    public async Task<List<VideoDto>> GetVideos(string playlistID)
    {
        var videoList = await _videosClient.GetFromJsonAsync<List<VideoDto>>($"videos/{playlistID}");

        if (videoList is null)
        {
            return new List<VideoDto>();
        }

        var isPremium = await _subscriptionsService.SubscriberIsPremium("user's name");

        foreach (var video in videoList)
        {
            video.IsRestricted = video.IsPremium && !isPremium;
        }

        return videoList;
    }
}
