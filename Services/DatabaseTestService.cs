using OnlineTestingApp.Data;
using Microsoft.EntityFrameworkCore;

namespace OnlineTestingApp.Services
{
    public class DatabaseTestService
    {
        private readonly AppDbContext _dbContext;

        public DatabaseTestService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> TestConnectionAsync()
{
    try
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();
        
        if (!canConnect)
            return "❌ Не удалось подключиться к базе данных";

        var userCount = await _dbContext.Users.CountAsync();
        var testCount = await _dbContext.Tests.CountAsync();

        return $"✅ Подключение успешно!\nПользователей: {userCount}\nТестов: {testCount}";
    }
    catch (Exception ex)
    {
        // Показываем ПОЛНУЮ информацию об ошибке
        return $"❌ Ошибка: {ex.Message}\n\nStack Trace: {ex.StackTrace}";
    }
}
    }
}