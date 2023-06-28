using DotNetFlix.UI.Models;
using DotNetFlix.UI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DotNetFlix.UI.Pages;

public partial class PlaylistsPage : ContentPage
{
    private readonly VideosService _videosService;

    public ICommand ViewPlaylistCommand => new Command<string>(async (id) => await ViewPlaylist(id));

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

        LoadingIndicator.IsVisible = true;
        
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
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private async Task ViewPlaylist(string playlistId)
    {
        await App.Current.MainPage.DisplayAlert(playlistId,"Showing playlist", "OK");
        var title = Playlists.FirstOrDefault(p => p.Id == playlistId)?.Title;
        await Navigation.PushAsync<VideosPage>(playlistId, title);
    }
}