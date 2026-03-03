namespace OnlineTestingApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        NavigateToTestDbButton.Clicked += OnNavigateToTestDbClicked;
    }

    private async void OnNavigateToTestDbClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("TestDbPage");
    }
}
