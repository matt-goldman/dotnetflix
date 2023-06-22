using DotNetFlix.Shared;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace YouTube_Test_App;

internal class YoutubeService
{
    private YouTubeService? _youTubeService;

    private string ChannelId;
    private string ApiKey;

    public YoutubeService()
    {
        ChannelId = "UCBFgwtV9lIIhvoNh0xoQ7Pg";
        ApiKey = "<Get from secrets>";
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
}
