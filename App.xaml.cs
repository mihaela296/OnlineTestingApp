namespace OnlineTestingApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new AppShell());
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(MainPage);
        
        window.Title = "Online Testing Platform";
        window.MinimumWidth = 400;
        window.MinimumHeight = 700;
        
        #if WINDOWS
        window.Width = 450;
        window.Height = 800;
        #endif
        
        return window;
    }
}
