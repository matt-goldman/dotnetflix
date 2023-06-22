namespace DotNetFlix.Shared;

public class PlaylistDto
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<string> ThumbnailUrls { get; set; } = new();
}
