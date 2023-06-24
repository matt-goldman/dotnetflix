using DotNetFlix.Shared;
using System.Security.Claims;

namespace DotNetFlix.API.Services;

public class VideosService
{
    public const string VideosClient = nameof(VideosClient);

    private readonly HttpClient _videosClient;
    
    private readonly SubscriptionsService _subscriptionsService;

    public VideosService(IHttpClientFactory httpClientFactory, SubscriptionsService subscriptionsService, IHttpContextAccessor httpContext)
    {
        _videosClient = httpClientFactory.CreateClient(VideosClient);
        _subscriptionsService = subscriptionsService;
        _httpContext = httpContext;
    }

    public IHttpContextAccessor _httpContext { get; }

    public async Task<List<VideoDto>> GetVideos(string playlistID)
    {
        var videoList = await _videosClient.GetFromJsonAsync<List<VideoDto>>($"videos/{playlistID}");

        if (videoList is null)
        {
            return new List<VideoDto>();
        }

        var un = _httpContext!.HttpContext!.User.FindFirst(ClaimTypes.Email)!.Value;

        var isPremium = await _subscriptionsService.SubscriberIsPremium(un);

        foreach (var video in videoList)
        {
            video.IsRestricted = video.IsPremium && !isPremium;
        }

        return videoList;
    }
}
