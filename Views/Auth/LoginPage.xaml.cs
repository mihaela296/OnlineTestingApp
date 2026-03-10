using OnlineTestingApp.ViewModels.Auth;

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
        
        var button = sender as Button;
        if (button != null)
        {
            button.Text = _isPasswordVisible ? "👁️‍🗨️" : "👁️";
        }
    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Восстановление пароля", 
            "Функция восстановления пароля будет доступна в следующей версии", 
            "OK");
    }
}
