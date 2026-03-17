using OnlineTestingApp.ViewModels.Auth;

namespace OnlineTestingApp.Views.Auth;

public partial class AccountBlockedPage : ContentPage
{
    public AccountBlockedPage(AccountBlockedViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}