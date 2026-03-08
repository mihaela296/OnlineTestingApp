using OnlineTestingApp.ViewModels.Auth;
using Microsoft.Maui.Controls;

namespace OnlineTestingApp.Views.Auth;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
