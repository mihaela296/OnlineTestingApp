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
        
        if (sender is Button button)
        {
            button.Text = _isPasswordVisible ? "👁️‍🗨️" : "👁️";
        }
    }
}