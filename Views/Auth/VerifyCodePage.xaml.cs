using OnlineTestingApp.ViewModels.Auth;

namespace OnlineTestingApp.Views.Auth;

public partial class VerifyCodePage : ContentPage
{
    public VerifyCodePage(VerifyCodeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
