namespace OnlineTestingApp.Views;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage()
    {
        InitializeComponent();
        LogoutButton.Clicked += OnLogoutClicked;
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        SecureStorage.RemoveAll();
        Preferences.Clear();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}