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
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
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
                await _dbContext.SaveChangesAsync();
                await LoadPendingTeachersAsync();

                await Shell.Current.DisplayAlert("Успех", "Учитель подтверждён", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task RejectAsync(int userId)
        {
            try
            {
                var accept = await Shell.Current.DisplayAlert(
                    "Подтверждение",
                    "Вы уверены, что хотите отклонить заявку учителя?",
                    "Да", "Нет");

                if (!accept) return;

                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null) return;

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                await LoadPendingTeachersAsync();

                await Shell.Current.DisplayAlert("Успех", "Заявка отклонена", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
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