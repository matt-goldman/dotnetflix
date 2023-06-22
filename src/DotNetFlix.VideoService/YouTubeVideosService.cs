using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace DotNetFlix.VideoService;

public class YouTubeVideosService
{
    private UserCredential? _userCredential;
    private YouTubeService? _youTubeService;

    private string ClientId;
    private string ClientSecret;
    private string ChannelId;

    public YouTubeVideosService(IConfiguration configuration)
    {
        ClientId = configuration["YouTube:ClientId"]!;
        ClientSecret = configuration["YouTube:ClientSecret"]!;
        ChannelId = configuration["YouTube:ChannelId"]!;
    }

    private async Task Initialise()
    {
        _userCredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret
            },
            new[] { YouTubeService.Scope.YoutubeReadonly },
            "user",
            CancellationToken.None);

        _youTubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _userCredential,
            ApplicationName = "DotNetFlix"
        });
    }

    public async Task GetPlayLists()
    {
        if (_youTubeService == null)
        {
            await Initialise();
        }

        var playLists = _youTubeService!.Playlists.List("snippet");

        playLists.ChannelId = ChannelId;
        playLists.MaxResults = 50;
        var theList = await playLists.ExecuteAsync();
    }

    public async Task ListPlaylists(string channelId)
    {
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = "YOUR_API_KEY",
            ApplicationName = this.GetType().ToString()
        });

        var playlistsRequest = youtubeService.Playlists.List("snippet");
        playlistsRequest.ChannelId = channelId;

        var playlistsResponse = await playlistsRequest.ExecuteAsync();

        foreach (var playlist in playlistsResponse.Items)
        {
            Console.WriteLine($"Playlist ID: {playlist.Id} Title: {playlist.Snippet.Title}");
        }
    }
}
