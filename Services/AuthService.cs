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

                if (user.PasswordHash != model.Password)
                    return (false, "Неверный пароль", null);

                if (!user.IsActive)
                    return (false, "Ваш аккаунт деактивирован. Обратитесь к администратору.", null);

                user.LastLoginDate = DateTime.UtcNow;
                
                await _deviceService.RegisterCurrentDeviceAsync(user.UserId);
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
                    PhoneNumber = model.PhoneNumber,
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
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return ("not_found", "Пользователь не найден");

            if (!user.IsActive)
                return ("inactive", "Ваш аккаунт деактивирован");

            if (user.Role?.RoleName == "Student")
            {
                var hasGroups = await _dbContext.UserGroups
                    .AnyAsync(ug => ug.UserId == userId);
                
                if (!hasGroups)
                    return ("pending_group", "Вы еще не добавлены ни в одну группу. Ожидайте, пока учитель добавит вас.");
            }

            if (user.Role?.RoleName == "Teacher" || user.Role?.RoleName == "Admin")
            {
                if (user.IsActive)
                    return ("active", "Аккаунт подтвержден");
                else
                    return ("pending_approval", "Ваша заявка на регистрацию ожидает подтверждения администратором");
            }

            return ("active", "Аккаунт активен");
        }

        public Task LogoutAsync()
        {
            Preferences.Clear();
            return Task.CompletedTask;
        }

        // ============= ВОССТАНОВЛЕНИЕ ПАРОЛЯ =============
        private static readonly Dictionary<string, (string Code, DateTime Expiry)> _resetCodes = new();

        private string GenerateResetCode() => new Random().Next(100000, 999999).ToString();

        public async Task<(bool success, string message)> RequestPasswordResetAsync(string email)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user == null)
                    return (true, "Если email зарегистрирован, код будет отправлен");

                // Проверка почтового сервиса
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

            // Хэшируем новый пароль
            user.PasswordHash = HashPassword(newPassword);
            _resetCodes.Remove(email);

            await _dbContext.SaveChangesAsync();
            return (true, "Пароль изменён");
        }
    }
}