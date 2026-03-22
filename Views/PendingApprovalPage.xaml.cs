using OnlineTestingApp.Services;
using OnlineTestingApp.ViewModels.Auth;
using OnlineTestingApp.Views.Auth;

namespace OnlineTestingApp.Views;

public partial class PendingApprovalPage : ContentPage
{
    public PendingApprovalPage()
    {
        InitializeComponent();
        LogoutButton.Clicked += OnLogoutClicked;
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        SecureStorage.RemoveAll();
        Preferences.Clear();
        
        var authService = App.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
        if (authService == null) return;
        
        var loginPage = new LoginPage(new LoginViewModel(authService));
        Application.Current.MainPage = new NavigationPage(loginPage);
    }
}