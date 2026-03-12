namespace OnlineTestingApp.Views;

public partial class TeacherDashboardPage : ContentPage
{
    public TeacherDashboardPage()
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