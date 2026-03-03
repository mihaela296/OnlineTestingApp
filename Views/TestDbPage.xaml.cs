using OnlineTestingApp.Services;

namespace OnlineTestingApp.Views;

public partial class TestDbPage : ContentPage
{
    private readonly DatabaseTestService _dbTestService;

    public TestDbPage(DatabaseTestService dbTestService)
    {
        InitializeComponent();
        _dbTestService = dbTestService;
        TestConnectionButton.Clicked += OnTestConnectionClicked;
    }

    private async void OnTestConnectionClicked(object? sender, EventArgs e)
    {
        try
        {
            ResultEditor.Text = "Подключение...";
            var result = await _dbTestService.TestConnectionAsync();
            ResultEditor.Text = result;
        }
        catch (Exception ex)
        {
            ResultEditor.Text = $"Ошибка: {ex.Message}";
        }
    }
}
