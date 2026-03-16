// ViewModels/Admin/UserManagementViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Models;
using System.Collections.ObjectModel;

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

                // Применяем фильтр
                query = ApplyFilter(query);

                // Применяем поиск
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
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        LastLoginDate = user.LastLoginDate,
                        Status = GetUserStatus(user)
                    });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить пользователей: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public void Filter(string filter)
        {
            SelectedFilter = filter;
            LoadUsersCommand.Execute(null);
        }

        [RelayCommand]
        public void Search()
        {
            LoadUsersCommand.Execute(null);
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

        private string GetUserStatus(User user)
        {
            if (!user.IsActive && user.Role?.RoleName == "Teacher")
                return "Ожидает";
            if (!user.IsActive)
                return "Заблокирован";
            return "Активен";
        }

        [RelayCommand]
        public async Task ToggleStatusAsync(int userId)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) return;

                user.IsActive = !user.IsActive;
                await _dbContext.SaveChangesAsync();
                await LoadUsersAsync();

                var message = user.IsActive ? "Пользователь активирован" : "Пользователь деактивирован";
                await Shell.Current.DisplayAlert("Успех", message, "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task DeleteAsync(int userId)
        {
            try
            {
                var accept = await Shell.Current.DisplayAlert(
                    "Подтверждение",
                    "Вы уверены, что хотите удалить пользователя? Это действие нельзя отменить.",
                    "Да", "Нет");

                if (!accept) return;

                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) return;

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                await LoadUsersAsync();

                await Shell.Current.DisplayAlert("Успех", "Пользователь удалён", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task CreateUserAsync()
        {
            await Shell.Current.DisplayAlert("Инфо", "Создание пользователя в разработке", "OK");
        }

        [RelayCommand]
        private async Task EditAsync(int userId)
        {
            await Shell.Current.DisplayAlert("Инфо", $"Редактирование пользователя {userId} в разработке", "OK");
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
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public string RoleColor => Role switch
        {
            "Admin" => "#EF4444",
            "Teacher" => "#F59E0B",
            "Student" => "#10B981",
            _ => "#6B7280"
        };
    }
}