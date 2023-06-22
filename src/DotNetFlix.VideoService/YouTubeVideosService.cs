using DotNetFlix.Shared;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace DotNetFlix.VideoService;

public class YouTubeVideosService
{
    private YouTubeService? _youTubeService;
    
    private string ChannelId;
    private string ApiKey;

    public YouTubeVideosService(IConfiguration configuration)
    {
        ChannelId = configuration["YouTube:ChannelId"]!;
        ApiKey = configuration["YouTube:ApiKey"]!;
    }

    private void Initialise()
    {
        _youTubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = ApiKey,
            ApplicationName = this.GetType().ToString()
        });
    }

    public async Task<List<PlaylistDto>> GetPlayLists()
    {
        if (_youTubeService is null)
        {
            Initialise();
        }

        var playlistRequest = _youTubeService!.Playlists.List("snippet");

        playlistRequest.ChannelId = ChannelId;
        playlistRequest.MaxResults = 50;


        var playlists = await playlistRequest.ExecuteAsync();

        var result = new List<PlaylistDto>();

        foreach (var item in playlists.Items)
        {
            var urls = new[]
            {
                item.Snippet.Thumbnails.Medium?.Url,
                item.Snippet.Thumbnails.High?.Url,
                item.Snippet.Thumbnails.Standard?.Url,
                item.Snippet.Thumbnails.Maxres?.Url
            }.Where(url => url != null).ToList();

            result.Add(new PlaylistDto
            {
                Id = item.Id,
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrls = urls!
            });
        }

        return result;
    }

    public async Task<List<VideoDto>> GetPlaylistVideos(string listId)
    {
        if (_youTubeService is null)
        {
            Initialise();
        }

        var playlistItemsRequest = _youTubeService!.PlaylistItems.List("snippet");

        playlistItemsRequest.PlaylistId = listId;
        playlistItemsRequest.MaxResults = 50;

        var playlistItems = await playlistItemsRequest.ExecuteAsync();

        var result = new List<VideoDto>();

        foreach (var item in playlistItems.Items)
        {
            result.Add(new VideoDto
            {
                Id = item.Snippet.ResourceId?.VideoId!,
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrl = item.Snippet.Thumbnails.Medium?.Url ?? item.Snippet.Thumbnails.High?.Url ?? item.Snippet.Thumbnails.Standard?.Url ?? item.Snippet.Thumbnails.Maxres?.Url ?? string.Empty,
                IsPremium = item.Snippet.IsPremium()
            });
        }

        return result;
    }
}

public static class SnipperHelpers
{
    public static bool IsPremium(this PlaylistItemSnippet snippet)
    {
        return snippet.Title.ToLower().Contains("goldman") || snippet.Description.ToLower().Contains("goldman");
    }
}
