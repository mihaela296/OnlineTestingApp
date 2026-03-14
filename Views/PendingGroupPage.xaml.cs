using OnlineTestingApp.Services;
using OnlineTestingApp.ViewModels.Auth;
using OnlineTestingApp.Views.Auth;
using Microsoft.Maui.Controls;

namespace OnlineTestingApp.Views;

public partial class PendingGroupPage : ContentPage
{
    public PendingGroupPage()
    {
        InitializeComponent();
        LogoutButton.Clicked += OnLogoutClicked;
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        SecureStorage.RemoveAll();
        Preferences.Clear();
        
        // Создаем новую страницу входа
        var authService = App.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
        if (authService == null) return;
        
        var loginPage = new LoginPage(new LoginViewModel(authService));
        
        // Заменяем текущую страницу на LoginPage
        Application.Current.MainPage = new NavigationPage(loginPage);
    }
}