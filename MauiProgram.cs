using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Services;

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

        // ==== Подключение к базе данных ====
        var connectionString = "Server=OnlineTestingPlatform.mssql.somee.com;Database=OnlineTestingPlatform;User Id=Dorogan_SQLLogin_1;Password=x4c9e1mit9;TrustServerCertificate=true;";
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddSingleton<DatabaseTestService>();

        return builder.Build();
    }
}
