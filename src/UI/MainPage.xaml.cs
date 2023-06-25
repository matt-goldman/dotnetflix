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

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        LoginIndicator.IsVisible = true;

        var response = await _authService.GetUserCode().ConfigureAwait(true);

        UserCodeLabel.Text = response.user_code;

        _authUrl = response.verification_uri_complete;

        QrView.Value = _authUrl;

        UrlLabel.Text = $"Or go to this URL and enter the code below: {response.verification_uri}";

        UserCodeMessage.IsVisible = true;

        try
        {
            var authResponse = await _authService.GetToken();

            _idToken = ParseToken(authResponse.id_token);
            UserCodeMessage.IsVisible = false;
            SessionTimerMessage.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            LoginIndicator.IsVisible = false;
        }
    }

    private void LogoutButton_Clicked(object sender, EventArgs e)
    {
        _idToken = null;
        UserCodeMessage.IsVisible = false;
        SessionTimerMessage.IsVisible = false;
        LoginButton.IsVisible = true;
        LoggedInMessage.IsVisible = false;
    }

    private async void CopyUserCode_Clicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(_authUrl);
    }

    private void SessionTimer_Dismissed(object sender, EventArgs e)
    {
        var name = _idToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

        UsernameLabel.Text = name;

        SessionTimerMessage.IsVisible = false;
        LoggedInMessage.IsVisible = true;
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

