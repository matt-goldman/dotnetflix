using DotNetFlix.Shared;
using DotNetFlix.UI.Services;
using System.Collections.ObjectModel;

namespace DotNetFlix.UI.Pages;

public partial class VideosPage : ContentPage
{
    private readonly VideosService _videosService;
    private string playlistId;

    public ObservableCollection<VideoDto> Videos { get; set; } = new();

    public VideosPage(VideosService videosService, string playlistId, string title)
	{
		InitializeComponent();
        _videosService = videosService;
        this.playlistId = playlistId;
        BindingContext = this;
        TitleLabel.Text = title;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LoadingIndicator.IsVisible = true;

        try
        {
            var videos = await _videosService.GetVideos(playlistId);

            foreach (var video in Videos)
            {
                Videos.Add(video);
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
}