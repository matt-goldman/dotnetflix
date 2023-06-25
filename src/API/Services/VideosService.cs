using DotNetFlix.Shared;
using System.Security.Claims;

namespace DotNetFlix.API.Services;

public class VideosService
{
    public const string VideosClient = nameof(VideosClient);

    private readonly HttpClient _videosClient;
    
    private readonly SubscriptionsService _subscriptionsService;

    public VideosService(IHttpClientFactory httpClientFactory, SubscriptionsService subscriptionsService)
    {
        _videosClient = httpClientFactory.CreateClient(VideosClient);
        _subscriptionsService = subscriptionsService;
    }

    public async Task<List<VideoDto>> GetVideos(string playlistID, string usernmae)
    {
        var videoList = await _videosClient.GetFromJsonAsync<List<VideoDto>>($"playlists/{playlistID}/videos");

        if (videoList is null)
        {
            return new List<VideoDto>();
        }

        var isPremium = await _subscriptionsService.SubscriberIsPremium(usernmae);

        foreach (var video in videoList)
        {
            video.IsRestricted = video.IsPremium && !isPremium;
        }

        return videoList;
    }

    public async Task<List<PlaylistDto>> GetPlaylists()
    {
        var playlists = await _videosClient.GetFromJsonAsync<List<PlaylistDto>>($"playlists/");
        return playlists ?? new List<PlaylistDto>();
    }
}
