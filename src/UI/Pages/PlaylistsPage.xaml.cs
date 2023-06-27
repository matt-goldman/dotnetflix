using DotNetFlix.Shared;
using DotNetFlix.UI.Models;
using DotNetFlix.UI.Services;
using System.Collections.ObjectModel;

namespace DotNetFlix.UI.Pages;

public partial class PlaylistsPage : ContentPage
{
    private readonly VideosService _videosService;

    public ObservableCollection<Playlist> Playlists { get; set; } = new();

    public PlaylistsPage(VideosService videosService)
	{
		InitializeComponent();
        _videosService = videosService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var playlists = await _videosService.GetPlaylists();

            foreach (var playlist in playlists)
            {
                Playlists.Add(new Playlist
                {
                    Description = playlist.Description,
                    Id = playlist.Id,
                    Thumbnail = playlist.ThumbnailUrls.FirstOrDefault(),
                    Title = playlist.Title
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}