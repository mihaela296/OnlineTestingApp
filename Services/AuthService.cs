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
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext dbContext, DeviceService deviceService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _deviceService = deviceService;
            _emailService = emailService;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task<(bool success, string message, User? user)> LoginAsync(LoginModel model)
{
    try
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
            return (false, "Пользователь не найден", null);

        // Проверяем, активен ли аккаунт
        if (!user.IsActive)
        {
            return (false, "account_deactivated", user); // Специальный статус для деактивированных
        }

        // ВРЕМЕННОЕ РЕШЕНИЕ: для админа используем простую проверку
        if (model.Email == "admin@example.com")
        {
            if (model.Password != "admin123")
                return (false, "Неверный пароль", null);
            
            // Устанавливаем правильный хэш для будущих входов
            user.PasswordHash = HashPassword("admin123");
        }
        else
        {
            // Для остальных пользователей - нормальная проверка хэша
            var inputHash = HashPassword(model.Password);
            if (user.PasswordHash != inputHash)
                return (false, "Неверный пароль", null);
        }

        user.LastLoginDate = DateTime.UtcNow;
        
        if (_deviceService != null)
        {
            await _deviceService.RegisterCurrentDeviceAsync(user.UserId);
        }
        await _dbContext.SaveChangesAsync();

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

        // Остальные методы без изменений...
        public async Task<(bool success, string message, User? user)> RegisterAsync(RegisterModel model)
        {
            try
            {
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);
                
                if (existingUser != null)
                    return (false, "Пользователь с таким email уже существует", null);

                existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username);
                
                if (existingUser != null)
                    return (false, "Пользователь с таким именем уже существует", null);

                var role = await _dbContext.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == model.Role);
                
                if (role == null)
                    return (false, "Указанная роль не существует", null);

                var user = new User
                {
                    Email = model.Email,
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password),
                    RoleId = role.RoleId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = model.Role == "Student"
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                var profile = new Profile
                {
                    UserId = user.UserId,
                    FirstName = model.FirstName ?? model.Username,
                    LastName = model.LastName,
                    PhoneNumber = NormalizePhoneNumber(model.PhoneNumber),
                    AvatarUrl = null
                };

                _dbContext.Profiles.Add(profile);
                await _dbContext.SaveChangesAsync();

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
                    
                    var admins = await _dbContext.Users
                        .Where(u => u.RoleId == 3)
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

        public async Task<(string status, string message)> GetUserStatusAsync(int userId)
{
    try
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return ("not_found", "Пользователь не найден");

        // Добавляем отладку
        System.Diagnostics.Debug.WriteLine($"GetUserStatusAsync: UserId={userId}, IsActive={user.IsActive}, Role={user.Role?.RoleName}");

        if (!user.IsActive)
        {
            if (user.Role?.RoleName == "Teacher" || user.Role?.RoleName == "Admin")
            {
                return ("pending_approval", "Ваша заявка на регистрацию ожидает подтверждения администратором");
            }
            return ("inactive", "Ваш аккаунт деактивирован");
        }

        if (user.Role?.RoleName == "Student")
        {
            var hasGroups = await _dbContext.UserGroups
                .AnyAsync(ug => ug.UserId == userId);
            
            if (!hasGroups)
                return ("pending_group", "Вы еще не добавлены ни в одну группу. Ожидайте, пока учитель добавит вас.");
        }

        return ("active", "Аккаунт активен");
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"GetUserStatusAsync error: {ex.Message}");
        return ("error", $"Ошибка: {ex.Message}");
    }
}

        public Task LogoutAsync()
        {
            Preferences.Clear();
            return Task.CompletedTask;
        }

        private static readonly Dictionary<string, (string Code, DateTime Expiry)> _resetCodes = new();
        private string GenerateResetCode() => new Random().Next(100000, 999999).ToString();

        public async Task<(bool success, string message)> RequestPasswordResetAsync(string email)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user == null)
                    return (true, "Если email зарегистрирован, код будет отправлен");

                if (_emailService == null)
                    return (false, "Ошибка конфигурации почтового сервиса");

                var code = GenerateResetCode();
                _resetCodes[email] = (code, DateTime.UtcNow.AddMinutes(15));

                await _emailService.SendResetCodeAsync(email, code);

                return (true, "Код отправлен на email");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> VerifyResetCodeAsync(string email, string code)
        {
            if (_resetCodes.TryGetValue(email, out var data))
            {
                if (data.Expiry < DateTime.UtcNow)
                {
                    _resetCodes.Remove(email);
                    return (false, "Код истёк");
                }

                if (data.Code == code)
                    return (true, "Код подтверждён");
            }
            return (false, "Неверный код");
        }

        public async Task<(bool success, string message)> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !_resetCodes.ContainsKey(email))
                return (false, "Ошибка");

            user.PasswordHash = HashPassword(newPassword);
            _resetCodes.Remove(email);

            await _dbContext.SaveChangesAsync();
            return (true, "Пароль изменён");
        }

        public async Task<User?> GetUserWithRoleAsync(int userId)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
                // Нормализация номера телефона: всегда начинается с +7
        public static string NormalizePhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Убираем все нецифровые символы
            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(digitsOnly))
                return string.Empty;

            // Если первая цифра 8, заменяем на 7
            if (digitsOnly.Length == 11 && digitsOnly[0] == '8')
            {
                digitsOnly = "7" + digitsOnly.Substring(1);
            }
            // Если номер из 11 цифр и начинается не с 7, меняем первую цифру на 7
            else if (digitsOnly.Length == 11 && digitsOnly[0] != '7')
            {
                digitsOnly = "7" + digitsOnly.Substring(1);
            }
            // Если номер из 10 цифр, добавляем 7 в начало
            else if (digitsOnly.Length == 10)
            {
                digitsOnly = "7" + digitsOnly;
            }

            // Возвращаем в формате +7XXXXXXXXXX
            return $"+{digitsOnly}";
        }

        // Форматирование номера для отображения: +X (XXX) XXX-XX-XX
        public static string FormatPhoneForDisplay(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length == 11)
            {
                return $"+{digitsOnly[0]} ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, 2)}";
            }

            return phone;
        }
    }
}