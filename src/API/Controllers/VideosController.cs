using DotNetFlix.API.Services;
using DotNetFlix.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DotNetFlix.API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class VideosController : ControllerBase
{
    private readonly VideosService _videosService;

    public VideosController(VideosService videosService)
    {
        _videosService = videosService;
    }

    [HttpGet("playlists/{playlistId}")]
    public async Task<List<VideoDto>> Get(string playlistID)
    {
        var username = User.FindFirstValue(ClaimTypes.Email)!;

        var videos = await _videosService.GetVideos(playlistID, username);

        return videos;
    }

    [HttpGet("playlists")]
    public async Task<List<PlaylistDto>> GetPlaylists()
    {
        var playlists = await _videosService.GetPlaylists();

        return playlists;
    }
}
