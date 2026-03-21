using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Models;
using System.Collections.ObjectModel;
using OnlineTestingApp.Views.Admin;
using OnlineTestingApp.Services;

namespace OnlineTestingApp.ViewModels.Admin
{
    public partial class UserManagementViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<UserItem> _users = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedFilter = "all";

        public UserManagementViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [RelayCommand]
        public async Task LoadUsersAsync()
        {
            try
            {
                IsBusy = true;
                Users.Clear();

                var query = _dbContext.Users
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
                        PhoneNumber = AuthService.FormatPhoneForDisplay(user.Profile?.PhoneNumber),
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

        [RelayCommand]
        public void Filter(string filter)
        {
            if (IsBusy) return;
            
            SelectedFilter = filter;
            LoadUsersCommand.Execute(null);
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
            try
            {
                if (userId <= 0)
                {
                    await ShowAlertAsync("Ошибка", "Некорректный ID пользователя");
                    return;
                }

                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) 
                {
                    await ShowAlertAsync("Ошибка", "Пользователь не найден");
                    return;
                }

                user.IsActive = !user.IsActive;
                await _dbContext.SaveChangesAsync();
                
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
            try
            {
                if (userId <= 0)
                {
                    await ShowAlertAsync("Ошибка", "Некорректный ID пользователя");
                    return;
                }

                var user = await _dbContext.Users
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

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    if (user.Logs?.Any() == true)
                        _dbContext.Logs.RemoveRange(user.Logs);

                    if (user.Notifications?.Any() == true)
                        _dbContext.Notifications.RemoveRange(user.Notifications);

                    if (user.Devices?.Any() == true)
                        _dbContext.Devices.RemoveRange(user.Devices);

                    if (user.UserGroups?.Any() == true)
                        _dbContext.UserGroups.RemoveRange(user.UserGroups);

                    if (user.Profile != null)
                        _dbContext.Profiles.Remove(user.Profile);

                    _dbContext.Users.Remove(user);
                    
                    await _dbContext.SaveChangesAsync();
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

                var editViewModel = new EditUserViewModel(_dbContext, user);
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
            "Admin" => "#EF4444",
            "Teacher" => "#F59E0B",
            "Student" => "#10B981",
            _ => "#6B7280"
        };
        
        public string StatusIcon => IsActive ? "🔓" : "🔒";
    }
}