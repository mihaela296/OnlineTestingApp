namespace OnlineTestingApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        
        // Настройка окна для десктопа
        window.Title = "Online Testing Platform";
        window.MinimumWidth = 400;
        window.MinimumHeight = 700;
        
        // Для Windows устанавливаем размер
        #if WINDOWS
        window.Width = 450;
        window.Height = 800;
        #endif
        
        return window;
    }
}