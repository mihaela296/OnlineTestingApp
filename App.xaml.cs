using OnlineTestingApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace OnlineTestingApp;

public partial class App : Application
{
    public App()
    {
        try
        {
            InitializeComponent();
            
            // Добавляем обработчик глобальных исключений
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedException;
            
            MainPage = new NavigationPage(new AppShell());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка в конструкторе App: {ex}");
            System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            
            // Создаем страницу с ошибкой
            MainPage = new ContentPage
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 20,
                    Padding = 30,
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                        new Label 
                        { 
                            Text = "❌ Ошибка запуска приложения",
                            FontSize = 24,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        new Label 
                        { 
                            Text = ex.Message,
                            FontSize = 14,
                            TextColor = Colors.Red,
                            HorizontalOptions = LayoutOptions.Center
                        },
                        new Label 
                        { 
                            Text = ex.StackTrace ?? "Нет стека вызовов",
                            FontSize = 10,
                            LineBreakMode = LineBreakMode.WordWrap,
                            MaxLines = 20
                        }
                    }
                }
            };
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        System.Diagnostics.Debug.WriteLine($"UnhandledException: {ex?.Message}");
        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex?.StackTrace}");
    }

    private void OnTaskSchedulerUnobservedException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"UnobservedTaskException: {e.Exception.Message}");
        System.Diagnostics.Debug.WriteLine($"StackTrace: {e.Exception.StackTrace}");
        e.SetObserved();
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