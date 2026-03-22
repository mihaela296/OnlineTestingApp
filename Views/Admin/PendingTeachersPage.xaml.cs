using OnlineTestingApp.ViewModels.Admin;

namespace OnlineTestingApp.Views.Admin;

public partial class PendingTeachersPage : ContentPage
{
    public PendingTeachersPage()
    {
        InitializeComponent();
    }
    
    public PendingTeachersPage(PendingTeachersViewModel viewModel) : this()
    {
        BindingContext = viewModel;
        
        // Загружаем данные при появлении страницы
        Appearing += async (s, e) => 
        {
            if (viewModel != null)
            {
                await viewModel.LoadPendingTeachersAsync();
            }
        };
    }
}