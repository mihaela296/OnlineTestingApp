using OnlineTestingApp.ViewModels.Admin;

namespace OnlineTestingApp.Views.Admin;

public partial class UserManagementPage : ContentPage
{
    public UserManagementPage(UserManagementViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Подписываемся на событие изменения текста
        SearchEntry.TextChanged += OnSearchTextChanged;
        
        // Загружаем пользователей при появлении страницы
        if (BindingContext is UserManagementViewModel vm)
        {
            vm.LoadUsersCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Отписываемся от события
        SearchEntry.TextChanged -= OnSearchTextChanged;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        // Вызываем поиск при изменении текста
        if (BindingContext is UserManagementViewModel vm)
        {
            vm.SearchCommand.Execute(null);
        }
    }
}