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
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        [ObservableProperty]
        private ObservableCollection<PendingTeacher> _pendingTeachers = new();

        [ObservableProperty]
        private int _pendingCount;

        [ObservableProperty]
        private bool _isBusy;

        public PendingTeachersViewModel(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        [RelayCommand]
        public async Task LoadPendingTeachersAsync()
        {
            if (IsBusy) return;
            
            try
            {
                IsBusy = true;
                PendingTeachers.Clear();

                using var dbContext = await _dbContextFactory.CreateDbContextAsync();

                var teachers = await dbContext.Users
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
            if (IsBusy) return;
            
            try
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                
                var user = await dbContext.Users.FindAsync(userId);
                if (user == null) return;

                user.IsActive = true;
                
                dbContext.Logs.Add(new Log
                {
                    UserId = userId,
                    Action = "TeacherApproved",
                    Timestamp = DateTime.UtcNow,
                    Details = "Учитель подтверждён администратором"
                });

                await dbContext.SaveChangesAsync();
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
            if (IsBusy) return;
            
            try
            {
                var accept = await ShowConfirmAsync(
                    "Подтверждение",
                    "Вы уверены, что хотите отклонить заявку учителя?");

                if (!accept) return;

                IsBusy = true;

                using var dbContext = await _dbContextFactory.CreateDbContextAsync();

                var user = await dbContext.Users
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

                using var transaction = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    if (user.Logs?.Any() == true)
                        dbContext.Logs.RemoveRange(user.Logs);

                    if (user.Notifications?.Any() == true)
                        dbContext.Notifications.RemoveRange(user.Notifications);

                    if (user.Devices?.Any() == true)
                        dbContext.Devices.RemoveRange(user.Devices);

                    if (user.Profile != null)
                        dbContext.Profiles.Remove(user.Profile);

                    dbContext.Users.Remove(user);
                    
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    
                    await LoadPendingTeachersAsync();
                    await ShowAlertAsync("Успех", "✗ Заявка отклонена");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;  // Исправлено: throw вместо throw ex
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