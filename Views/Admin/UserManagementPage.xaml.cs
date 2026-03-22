using OnlineTestingApp.ViewModels.Admin;

namespace OnlineTestingApp.Views.Admin;

public partial class UserManagementPage : ContentPage
{
    private bool _isLoading = false;
    
    public UserManagementPage()
    {
        InitializeComponent();
    }
    
    public UserManagementPage(UserManagementViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (!_isLoading && BindingContext is UserManagementViewModel vm)
        {
            _isLoading = true;
            
            Task.Run(async () =>
            {
                await Task.Delay(100);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    vm.LoadUsersCommand.Execute(null);
                    _isLoading = false;
                });
            });
        }
        
        if (SearchEntry != null)
        {
            SearchEntry.TextChanged += OnSearchTextChanged;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (SearchEntry != null)
        {
            SearchEntry.TextChanged -= OnSearchTextChanged;
        }
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (BindingContext is UserManagementViewModel vm && !_isLoading)
        {
            _isLoading = true;
            await vm.SearchCommand.ExecuteAsync(null);
            _isLoading = false;
        }
    }
}