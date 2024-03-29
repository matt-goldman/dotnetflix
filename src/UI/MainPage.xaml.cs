﻿using DotNetFlix.UI.Pages;
using DotNetFlix.UI.Services;
using System.IdentityModel.Tokens.Jwt;

namespace DotNetFlix.UI;

public partial class MainPage : ContentPage
{
    private readonly AuthService _authService;

    private JwtSecurityToken _idToken;

    string _authUrl;

    public MainPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected async override void OnAppearing()
    {
        LoginIndicator.IsVisible = true;

        var response = await _authService.GetUserCode().ConfigureAwait(true);

        UrlLabel.Text = response.verification_uri;

        CodeLabel.Text = response.user_code;

        _authUrl = response.verification_uri_complete;

        QrView.Value = _authUrl;

        LoginIndicator.IsVisible = false;
        
        try
        {
            var authResponse = await _authService.GetToken();

            _idToken = ParseToken(authResponse.id_token);

            await Navigation.PushAsync<PlaylistsPage>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private JwtSecurityToken ParseToken(string inTtoken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(inTtoken);

            return token;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}

