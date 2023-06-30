using DotNetFlix.UI.Models;
using DotNetFlix.UI.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DotNetFlix.UI.Pages;

public partial class PlaylistsPage : ContentPage
{
    private readonly VideosService _videosService;
    
    public ObservableCollection<Playlist> Playlists { get; set; } = new();

    public Playlist SelectedPlaylist { get; set; }

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

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var playlist = e.CurrentSelection.FirstOrDefault() as Playlist;

        if (playlist != null)
        {
            try
            {
                await Navigation.PushAsync<VideosPage>(playlist.Id, playlist.Title);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}