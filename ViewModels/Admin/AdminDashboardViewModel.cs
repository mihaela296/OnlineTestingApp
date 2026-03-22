using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Services;
using OnlineTestingApp.ViewModels.Auth;
using OnlineTestingApp.Views.Auth;
using OnlineTestingApp.Views.Admin;

namespace OnlineTestingApp.ViewModels.Admin
{
    public partial class AdminDashboardViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly AuthService _authService;

        [ObservableProperty]
        private int _totalUsers;

        [ObservableProperty]
        private int _pendingTeachers;

        [ObservableProperty]
        private int _totalTests;

        [ObservableProperty]
        private int _totalTeachers;

        [ObservableProperty]
        private int _totalStudents;

        public AdminDashboardViewModel(IDbContextFactory<AppDbContext> dbContextFactory, AuthService authService)
{
    _dbContextFactory = dbContextFactory;
    _authService = authService;
}

        [RelayCommand]
        public async Task LoadStatsAsync()
        {
            try
            {
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();
                
                TotalUsers = await dbContext.Users.CountAsync();
                TotalTests = await dbContext.Tests.CountAsync();
                TotalTeachers = await dbContext.Users
                    .CountAsync(u => u.Role != null && u.Role.RoleName == "Teacher" && u.IsActive);
                TotalStudents = await dbContext.Users
                    .CountAsync(u => u.Role != null && u.Role.RoleName == "Student" && u.IsActive);
                PendingTeachers = await dbContext.Users
                    .CountAsync(u => !u.IsActive && u.Role != null && u.Role.RoleName == "Teacher");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки статистики: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task GoToUserManagementAsync()
        {
            try
            {
                var userManagementVM = new UserManagementViewModel(_dbContextFactory);
                var userManagementPage = new UserManagementPage(userManagementVM);
                
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window?.Page != null)
                {
                    await window.Page.Navigation.PushAsync(userManagementPage);
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
            }
        }

        [RelayCommand]
        private async Task GoToPendingTeachersAsync()
        {
            try
            {
                var pendingVM = new PendingTeachersViewModel(_dbContextFactory);
                var pendingPage = new PendingTeachersPage(pendingVM);
                
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window?.Page != null)
                {
                    await window.Page.Navigation.PushAsync(pendingPage);
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
            }
        }

        [RelayCommand]
        private async Task GoToGroupManagementAsync()
        {
            await ShowAlertAsync("Инфо", "Управление группами в разработке");
        }

        [RelayCommand]
        private async Task GoToLogsAsync()
        {
            await ShowAlertAsync("Инфо", "Логи системы в разработке");
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                SecureStorage.RemoveAll();
                Preferences.Clear();
                
                var loginPage = new LoginPage(new LoginViewModel(_authService));
                
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window?.Page != null)
                {
                    Application.Current.MainPage = new NavigationPage(loginPage);
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
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
    }
}