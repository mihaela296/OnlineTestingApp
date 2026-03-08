using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Models;
using OnlineTestingApp.Models.Auth;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

namespace OnlineTestingApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly DeviceService _deviceService;

        public AuthService(AppDbContext dbContext, DeviceService deviceService)
        {
            _dbContext = dbContext;
            _deviceService = deviceService;
        }

        // Хэширование пароля
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        // Вход пользователя
        public async Task<(bool success, string message, User? user)> LoginAsync(LoginModel model)
        {
            try
            {
                // Ищем пользователя по email
                var user = await _dbContext.Users
                    .Include(u => u.Role)
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                    return (false, "Пользователь не найден", null);

                // Проверяем пароль (для тестовых данных из SQL скрипта)
                // В тестовых данных пароли уже захэшированы как "hash_12345" и т.д.
                if (user.PasswordHash != model.Password)
                    return (false, "Неверный пароль", null);

                // Проверяем активен ли пользователь
                if (!user.IsActive)
                    return (false, "Ваш аккаунт деактивирован. Обратитесь к администратору.", null);

                // Обновляем дату последнего входа
                user.LastLoginDate = DateTime.UtcNow;
                
                // Сохраняем информацию об устройстве
                await _deviceService.RegisterCurrentDeviceAsync(user.UserId);
                
                await _dbContext.SaveChangesAsync();

                // Сохраняем информацию о пользователе в SecureStorage
                await SecureStorage.SetAsync("user_id", user.UserId.ToString());
                await SecureStorage.SetAsync("user_email", user.Email);
                await SecureStorage.SetAsync("user_role", user.Role?.RoleName ?? "Student");
                
                // Сохраняем объект пользователя в Preferences для быстрого доступа
                var userJson = JsonSerializer.Serialize(new
                {
                    user.UserId,
                    user.Email,
                    user.Username,
                    Role = user.Role?.RoleName,
                    user.IsActive
                });
                Preferences.Set("current_user", userJson);

                return (true, "Успешный вход", user);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка входа: {ex.Message}", null);
            }
        }

        // Регистрация пользователя
        public async Task<(bool success, string message, User? user)> RegisterAsync(RegisterModel model)
        {
            try
            {
                // Проверяем, существует ли пользователь с таким email
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);
                
                if (existingUser != null)
                    return (false, "Пользователь с таким email уже существует", null);

                // Проверяем, существует ли пользователь с таким username
                existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username);
                
                if (existingUser != null)
                    return (false, "Пользователь с таким именем уже существует", null);

                // Получаем роль
                var role = await _dbContext.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == model.Role);
                
                if (role == null)
                    return (false, "Указанная роль не существует", null);

                // Создаем нового пользователя
                var user = new User
                {
                    Email = model.Email,
                    Username = model.Username,
                    PasswordHash = model.Password, // Для тестов сохраняем как есть
                    RoleId = role.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = model.Role == "Student" // Ученики сразу активны, учителя/админы требуют подтверждения
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                // Создаем профиль
                var profile = new Profile
                {
                    UserId = user.UserId,
                    FirstName = model.FirstName ?? model.Username,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    AvatarUrl = null
                };

                _dbContext.Profiles.Add(profile);
                await _dbContext.SaveChangesAsync();

                // Если это ученик, добавляем запись в логи
                if (model.Role == "Student")
                {
                    _dbContext.Logs.Add(new Log
                    {
                        UserId = user.UserId,
                        Action = "StudentRegistered",
                        Timestamp = DateTime.UtcNow,
                        Details = "Ученик зарегистрировался автоматически"
                    });
                }
                else
                {
                    _dbContext.Logs.Add(new Log
                    {
                        UserId = user.UserId,
                        Action = "TeacherRegistration",
                        Timestamp = DateTime.UtcNow,
                        Details = "Учитель зарегистрировался, ожидает подтверждения"
                    });
                    
                    // Создаем уведомление для админов
                    var admins = await _dbContext.Users
                        .Where(u => u.RoleId == 3) // Admin role
                        .ToListAsync();
                    
                    foreach (var admin in admins)
                    {
                        _dbContext.Notifications.Add(new Notification
                        {
                            UserId = admin.UserId,
                            Title = "Новая заявка на регистрацию",
                            NotificationText = $"Пользователь {user.Username} ({user.Email}) зарегистрировался как {model.Role}",
                            NotificationType = "RegistrationRequest",
                            Data = $"{{\"UserId\":{user.UserId}}}",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _dbContext.SaveChangesAsync();

                return (true, "Регистрация успешна", user);
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка регистрации: {ex.Message}", null);
            }
        }

        // Проверка статуса пользователя
        public async Task<(string status, string message)> GetUserStatusAsync(int userId)
        {
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return ("not_found", "Пользователь не найден");

            if (!user.IsActive)
                return ("inactive", "Ваш аккаунт деактивирован");

            // Для учеников проверяем, добавлены ли они в группу
            if (user.Role?.RoleName == "Student")
            {
                var hasGroups = await _dbContext.UserGroups
                    .AnyAsync(ug => ug.UserId == userId);
                
                if (!hasGroups)
                    return ("pending_group", "Вы еще не добавлены ни в одну группу. Ожидайте, пока учитель добавит вас.");
            }

            // Для учителей проверяем, подтвержден ли аккаунт
            if (user.Role?.RoleName == "Teacher" || user.Role?.RoleName == "Admin")
            {
                if (user.IsActive)
                    return ("active", "Аккаунт подтвержден");
                else
                    return ("pending_approval", "Ваша заявка на регистрацию ожидает подтверждения администратором");
            }

            return ("active", "Аккаунт активен");
        }

        // Выход из системы
        public Task LogoutAsync()
        {
            SecureStorage.RemoveAll();
            Preferences.Clear();
            return Task.CompletedTask;
        }
    }
}
