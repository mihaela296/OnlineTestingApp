// ViewModels/Admin/PendingTeachersViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Models;
using System.Collections.ObjectModel;

namespace OnlineTestingApp.ViewModels.Admin
{
    public partial class PendingTeachersViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<PendingTeacher> _pendingTeachers = new();

        [ObservableProperty]
        private int _pendingCount;

        [ObservableProperty]
        private bool _isBusy;

        public PendingTeachersViewModel(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [RelayCommand]
        public async Task LoadPendingTeachersAsync()
        {
            try
            {
                IsBusy = true;
                PendingTeachers.Clear();

                var teachers = await _dbContext.Users
                    .Include(u => u.Role)
                    .Include(u => u.Profile)
                    .Where(u => !u.IsActive && u.Role != null && u.Role.RoleName == "Teacher")
                    .OrderBy(u => u.CreatedAt)
                    .ToListAsync();

                foreach (var teacher in teachers)
                {
                    PendingTeachers.Add(new PendingTeacher
                    {
                        UserId = teacher.UserId,
                        Username = teacher.Username,
                        Email = teacher.Email,
                        FirstName = teacher.Profile?.FirstName,
                        LastName = teacher.Profile?.LastName,
                        CreatedAt = teacher.CreatedAt
                    });
                }

                PendingCount = PendingTeachers.Count;
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task ApproveAsync(int userId)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) return;

                user.IsActive = true;
                
                _dbContext.Logs.Add(new Log
                {
                    UserId = userId,
                    Action = "TeacherApproved",
                    Timestamp = DateTime.UtcNow,
                    Details = "Учитель подтверждён администратором"
                });

                await _dbContext.SaveChangesAsync();
                await LoadPendingTeachersAsync();

                await ShowAlertAsync("Успех", "✅ Учитель подтверждён");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
            }
        }

        [RelayCommand]
public async Task RejectAsync(int userId)
{
    try
    {
        var accept = await ShowConfirmAsync(
            "Подтверждение",
            "Вы уверены, что хотите отклонить заявку учителя?");

        if (!accept) return;

        IsBusy = true;

        // Загружаем учителя со всеми связанными данными
        var user = await _dbContext.Users
            .Include(u => u.Profile)
            .Include(u => u.Logs)
            .Include(u => u.Notifications)
            .Include(u => u.Devices)
            .FirstOrDefaultAsync(u => u.UserId == userId);
            
        if (user == null) 
        {
            await ShowAlertAsync("Ошибка", "Пользователь не найден");
            return;
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            // Удаляем все связанные данные
            if (user.Logs?.Any() == true)
                _dbContext.Logs.RemoveRange(user.Logs);

            if (user.Notifications?.Any() == true)
                _dbContext.Notifications.RemoveRange(user.Notifications);

            if (user.Devices?.Any() == true)
                _dbContext.Devices.RemoveRange(user.Devices);

            if (user.Profile != null)
                _dbContext.Profiles.Remove(user.Profile);

            // Удаляем пользователя
            _dbContext.Users.Remove(user);
            
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            await LoadPendingTeachersAsync();
            await ShowAlertAsync("Успех", "✗ Заявка отклонена");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            
            // Показываем подробную ошибку
            var errorMessage = $"Ошибка при удалении: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
            }
            
            await ShowAlertAsync("Ошибка", errorMessage);
        }
    }
    catch (Exception ex)
    {
        await ShowAlertAsync("Ошибка", ex.Message);
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

        private async Task<bool> ShowConfirmAsync(string title, string message)
{
    try
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.Page != null)
        {
            return await window.Page.DisplayAlert(title, message, "Да", "Нет");
        }
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Ошибка показа диалога: {ex.Message}");
    }
    return false;
}
    }

    public class PendingTeacher
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public DateTime CreatedAt { get; set; }
    }
}