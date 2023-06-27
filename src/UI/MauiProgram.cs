using DotNetFlix.UI.Helpers;
using DotNetFlix.UI.Pages;
using DotNetFlix.UI.Services;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace DotNetFlix.UI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.UseBarcodeReader()
			.UsePageResolver();

		builder.Services.AddSingleton(new AuthSettings
		{
            ClientId = "dotnetflix.client",
            DeviceCodeEndpoint = "connect/deviceauthorization",
            TokenEndpoint = "connect/token",
            Scopes = "dotnetflix.api",
#if DEBUG            
            BaseUrl = "https://localhost:5001/"
#else
#endif
        });

		builder.Services.AddSingleton<AuthHandler>();

		builder.Services.AddSingleton<AuthService>();

		builder.Services.AddHttpClient();

		builder.Services.AddHttpClient(VideosService.VideosClient, client => client.BaseAddress = new Uri("https://localhost:7132/"))
            .AddHttpMessageHandler((s) => s.GetService<AuthHandler>());

		builder.Services.AddSingleton<VideosService>();


		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<PlaylistsPage>();
		builder.Services.AddSingleton<VideosPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
