using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Services;
using OnlineTestingApp.ViewModels.Auth;
using OnlineTestingApp.Views;
using OnlineTestingApp.Views.Auth;

namespace OnlineTestingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var connectionString = "Server=OnlineTestingPlatform.mssql.somee.com;Database=OnlineTestingPlatform;User Id=Dorogan_SQLLogin_1;Password=x4c9e1mit9;TrustServerCertificate=true;";
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Services
        builder.Services.AddSingleton<DatabaseTestService>();
        builder.Services.AddSingleton<DeviceService>();
        builder.Services.AddSingleton<AuthService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<PendingGroupPage>();
        builder.Services.AddTransient<PendingApprovalPage>();
        builder.Services.AddTransient<StudentDashboardPage>();
        builder.Services.AddTransient<TeacherDashboardPage>();
        builder.Services.AddTransient<AdminDashboardPage>();
        builder.Services.AddTransient<TestDbPage>();

        return builder.Build();
    }
}
