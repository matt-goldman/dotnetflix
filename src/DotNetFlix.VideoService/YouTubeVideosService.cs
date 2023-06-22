using DotNetFlix.Shared;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

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

        var playLists = _youTubeService!.Playlists.List("snippet");

        playLists.ChannelId = ChannelId;
        var theList = await playLists.ExecuteAsync();

        var result = new List<PlaylistDto>();

        foreach (var item in theList.Items)
        {
            result.Add(new PlaylistDto
            {
                Id = item.Id,
                Title = item.Snippet.Title,
                Description = item.Snippet.Description,
                ThumbnailUrls = new List<string>
                {
                    item.Snippet.Thumbnails.Medium.Url,
                    item.Snippet.Thumbnails.High.Url,
                    item.Snippet.Thumbnails.Standard.Url,
                    item.Snippet.Thumbnails.Maxres.Url
                }
            });
        }

        return result;
    }
}
