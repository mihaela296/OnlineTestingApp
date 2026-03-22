using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using OnlineTestingApp.Views.Admin;

namespace OnlineTestingApp.ViewModels.Admin
{
    public partial class UserManagementViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        [ObservableProperty]
        private ObservableCollection<UserItem> _users = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilter = "all";

        public UserManagementViewModel(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        [RelayCommand]
        public async Task LoadUsersAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                Users.Clear();

                // Создаем новый контекст для каждой операции
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();

                var query = dbContext.Users
                    .Include(u => u.Role)
                    .Include(u => u.Profile)
                    .AsQueryable();

                query = ApplyFilter(query);

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var search = SearchText.ToLower();
                    query = query.Where(u => 
                        u.Username.ToLower().Contains(search) ||
                        u.Email.ToLower().Contains(search) ||
                        (u.Profile != null && (
                            u.Profile.FirstName.ToLower().Contains(search) ||
                            u.Profile.LastName.ToLower().Contains(search)
                        ))
                    );
                }

                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .ToListAsync();

                foreach (var user in users)
                {
                    Users.Add(new UserItem
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role?.RoleName ?? "Unknown",
                        FirstName = user.Profile?.FirstName,
                        LastName = user.Profile?.LastName,
                        PhoneNumber = FormatStoredPhoneNumber(user.Profile?.PhoneNumber),
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        Status = user.IsActive ? "Активен" : "Заблокирован"
                    });
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", $"Не удалось загрузить пользователей: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string FormatStoredPhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;
            
            var digitsOnly = Regex.Replace(phone, @"[^\d]", "");
            
            if (digitsOnly.StartsWith("8") && digitsOnly.Length == 11)
            {
                digitsOnly = "7" + digitsOnly.Substring(1);
            }
            
            if (digitsOnly.Length == 11 && digitsOnly.StartsWith("7"))
            {
                return $"+{digitsOnly[0]} ({digitsOnly.Substring(1, 3)}) {digitsOnly.Substring(4, 3)}-{digitsOnly.Substring(7, 2)}-{digitsOnly.Substring(9, 2)}";
            }
            
            return phone;
        }

        [RelayCommand]
        public async Task Filter(string filter)
        {
            if (IsBusy) return;
            
            SelectedFilter = filter;
            await LoadUsersAsync();
        }

        [RelayCommand]
        public async Task Search()
        {
            if (IsBusy) return;
            
            await LoadUsersAsync();
        }

        private IQueryable<User> ApplyFilter(IQueryable<User> query)
        {
            return SelectedFilter switch
            {
                "students" => query.Where(u => u.Role != null && u.Role.RoleName == "Student"),
                "teachers" => query.Where(u => u.Role != null && u.Role.RoleName == "Teacher"),
                "admins" => query.Where(u => u.Role != null && u.Role.RoleName == "Admin"),
                "active" => query.Where(u => u.IsActive),
                "inactive" => query.Where(u => !u.IsActive),
                _ => query
            };
        }

        [RelayCommand]
        public async Task ToggleStatusAsync(int userId)
        {
            if (IsBusy) return;
            
            try
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                
                if (userId <= 0)
                {
                    await ShowAlertAsync("Ошибка", "Некорректный ID пользователя");
                    return;
                }

                var user = await dbContext.Users.FindAsync(userId);
                if (user == null) 
                {
                    await ShowAlertAsync("Ошибка", "Пользователь не найден");
                    return;
                }

                user.IsActive = !user.IsActive;
                await dbContext.SaveChangesAsync();
                
                await LoadUsersAsync();
                
                string status = user.IsActive ? "активирован" : "деактивирован";
                await ShowAlertAsync("Успех", $"✅ Пользователь {user.Username ?? "Пользователь"} {status}");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
            }
        }

        [RelayCommand]
        public async Task DeleteAsync(int userId)
        {
            if (IsBusy) return;
            
            try
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                
                if (userId <= 0)
                {
                    await ShowAlertAsync("Ошибка", "Некорректный ID пользователя");
                    return;
                }

                var user = await dbContext.Users
                    .Include(u => u.Profile)
                    .Include(u => u.Logs)
                    .Include(u => u.Notifications)
                    .Include(u => u.Devices)
                    .Include(u => u.UserGroups)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null) 
                {
                    await ShowAlertAsync("Ошибка", "Пользователь не найден");
                    return;
                }

                var window = Application.Current?.Windows.FirstOrDefault();
                if (window?.Page == null) return;

                var accept = await window.Page.DisplayAlert(
                    "Подтверждение удаления",
                    $"Вы уверены, что хотите удалить пользователя {user.Username ?? "Пользователь"}?",
                    "Да",
                    "Нет");

                if (!accept) return;

                IsBusy = true;

                using var transaction = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    if (user.Logs?.Any() == true)
                        dbContext.Logs.RemoveRange(user.Logs);

                    if (user.Notifications?.Any() == true)
                        dbContext.Notifications.RemoveRange(user.Notifications);

                    if (user.Devices?.Any() == true)
                        dbContext.Devices.RemoveRange(user.Devices);

                    if (user.UserGroups?.Any() == true)
                        dbContext.UserGroups.RemoveRange(user.UserGroups);

                    if (user.Profile != null)
                        dbContext.Profiles.Remove(user.Profile);

                    dbContext.Users.Remove(user);
                    
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    await LoadUsersAsync();
                    await ShowAlertAsync("Успех", $"✅ Пользователь {user.Username} успешно удалён");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", $"Не удалось удалить пользователя: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ShowAlertAsync(string title, string message)
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page != null)
            {
                await window.Page.DisplayAlert(title, message, "OK");
            }
        }

        [RelayCommand]
        private async Task CreateUserAsync()
        {
            await ShowAlertAsync("Инфо", "Создание пользователя в разработке");
        }

        [RelayCommand]
        private async Task EditAsync(int userId)
        {
            if (IsBusy) return;
            
            try
            {
                if (userId <= 0)
                {
                    await ShowAlertAsync("Ошибка", "Некорректный ID пользователя");
                    return;
                }

                var user = Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    await ShowAlertAsync("Ошибка", "Пользователь не найден");
                    return;
                }

                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                var editViewModel = new EditUserViewModel(dbContext, user);
                var editPage = new EditUserPage(editViewModel);
                
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window?.Page != null)
                {
                    await window.Page.Navigation.PushAsync(editPage);
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
            }
        }
    }

    public class UserItem
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public string RoleColor => Role switch
{
    "Admin" => "#F59E0B",    // Оранжевый для админа
    "Teacher" => "#2DD4BF",  // Бирюзовый для учителя
    "Student" => "#10B981",  // Зеленый для ученика
    _ => "#6B7280"
};
        
        public string StatusIcon => IsActive ? "🔓" : "🔒";
    }
}