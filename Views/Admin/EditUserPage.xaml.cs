using OnlineTestingApp.ViewModels.Admin;

namespace OnlineTestingApp.Views.Admin;

public partial class EditUserPage : ContentPage
{
    public EditUserPage(EditUserViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}