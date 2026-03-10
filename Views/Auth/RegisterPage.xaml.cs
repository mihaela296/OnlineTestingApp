using OnlineTestingApp.ViewModels.Auth;

namespace OnlineTestingApp.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private bool _isPasswordVisible = false;
    private bool _isConfirmPasswordVisible = false;
    
    public RegisterPage(RegisterViewModel viewModel)
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

    private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
    {
        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
        
        var button = sender as Button;
        if (button != null)
        {
            button.Text = _isConfirmPasswordVisible ? "👁️‍🗨️" : "👁️";
        }
    }
}
