using OnlineTestingApp.ViewModels.Admin;

namespace OnlineTestingApp.Views.Admin;

public partial class PendingTeachersPage : ContentPage
{
    public PendingTeachersPage(PendingTeachersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        Appearing += async (s, e) => await viewModel.LoadPendingTeachersAsync();
    }
}