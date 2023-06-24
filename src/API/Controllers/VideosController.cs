using DotNetFlix.API.Services;
using DotNetFlix.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet(Name = "Videos")]
    public async Task<List<VideoDto>> Get(string playlistID)
    {
        var videos = await _videosService.GetVideos(playlistID);

        return videos;
    }
}
