using DotNetFlix.Shared;
using System.Net.Http.Json;

namespace DotNetFlix.UI.Services;

public class VideosService
{
    public const string VideosClient = nameof(VideosClient);
    private readonly IHttpClientFactory _httpClientFactory;

    public VideosService(IHttpClientFactory httpClientFactory)
	{
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<PlaylistDto>> GetPlaylists()
    {
        var client = _httpClientFactory.CreateClient(VideosClient);

        var playlists = await client.GetFromJsonAsync<List<PlaylistDto>>("Videos/playlists");

        return playlists;
    }

    public async Task<IEnumerable<VideoDto>> GetVideos(string playlistId)
    {
        var client = _httpClientFactory.CreateClient(VideosClient);

        var videos = await client.GetFromJsonAsync<List<VideoDto>>($"Videos/playlists/{PlaylistId}");

        return videos;
    }
}
