using OnlineTestingApp.ViewModels.Auth;
using OnlineTestingApp.Services;
using OnlineTestingApp.ViewModels.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineTestingApp.Views.Auth;

public partial class LoginPage : ContentPage
{
    private bool _isPasswordVisible = false;
    
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        
        if (sender is Button button)
        {
            button.Text = _isPasswordVisible ? "👁️‍🗨️" : "👁️";
        }
    }

            private async void OnForgotPasswordButtonClicked(object sender, EventArgs e)
    {
        var authService = App.Current?.Handler?.MauiContext?.Services.GetService<AuthService>();
        if (authService == null) return;

        var viewModel = new ForgotPasswordViewModel(authService);
        var page = new ForgotPasswordPage(viewModel);
        
        await Navigation.PushAsync(page);
    }
}