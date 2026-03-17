using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Services;
using OnlineTestingApp.ViewModels.Auth;
using OnlineTestingApp.ViewModels.Admin;
using OnlineTestingApp.Views;
using OnlineTestingApp.Views.Auth;
using OnlineTestingApp.Views.Admin;
using CommunityToolkit.Maui; // Добавь эту строку

namespace OnlineTestingApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit() // Добавлено для поддержки toolkit
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
        builder.Services.AddSingleton<IEmailService, EmailService>();

        // Auth ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<VerifyCodeViewModel>();
        builder.Services.AddTransient<ResetPasswordViewModel>();

        // Admin ViewModels
        builder.Services.AddTransient<AdminDashboardViewModel>();
        builder.Services.AddTransient<UserManagementViewModel>();
        builder.Services.AddTransient<PendingTeachersViewModel>();
        builder.Services.AddTransient<AccountBlockedViewModel>();

        // Auth Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<VerifyCodePage>();
        builder.Services.AddTransient<ResetPasswordPage>();
        builder.Services.AddTransient<PendingGroupPage>();
        builder.Services.AddTransient<PendingApprovalPage>();
        builder.Services.AddTransient<StudentDashboardPage>();
        builder.Services.AddTransient<TeacherDashboardPage>();
        builder.Services.AddTransient<TestDbPage>();
        builder.Services.AddTransient<AccountBlockedPage>();

        // Admin Pages
        builder.Services.AddTransient<AdminDashboardPage>();
        builder.Services.AddTransient<UserManagementPage>();
        builder.Services.AddTransient<PendingTeachersPage>();

        return builder.Build();
    }
}