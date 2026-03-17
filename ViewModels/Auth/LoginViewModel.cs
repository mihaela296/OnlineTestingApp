using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Models;
using OnlineTestingApp.Models.Auth;
using OnlineTestingApp.Services;
using System;
using System.Threading.Tasks;
using OnlineTestingApp.Views;
using OnlineTestingApp.Views.Auth;
using OnlineTestingApp.Views.Admin;
using OnlineTestingApp.ViewModels.Admin; // Добавлено для AdminDashboardViewModel
using OnlineTestingApp.Data; // Добавлено для AppDbContext

namespace OnlineTestingApp.ViewModels.Auth
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private LoginModel _loginModel = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private bool _isPasswordVisible = false;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
private async Task LoginAsync()
{
    if (IsBusy)
        return;

    try
    {
        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(LoginModel.Email) || 
            string.IsNullOrWhiteSpace(LoginModel.Password))
        {
            ErrorMessage = "Заполните все поля";
            HasError = true;
            return;
        }

        var result = await _authService.LoginAsync(LoginModel);
        
        if (!result.success)
        {
            // Проверяем специальный статус для заблокированных
            if (result.message == "account_deactivated" && result.user != null)
            {
                var blockedPage = new AccountBlockedPage(
                    new AccountBlockedViewModel(_authService, result.user.Email)
                );
                
                var window = GetCurrentWindow();
                if (window?.Page != null)
                {
                    await window.Page.Navigation.PushAsync(blockedPage);
                    return;
                }
            }
            
            ErrorMessage = result.message;
            HasError = true;
            return;
        }

        var user = result.user;
        await NavigateBasedOnRole(user);
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Ошибка входа: {ex.Message}";
        HasError = true;
    }
    finally
    {
        IsBusy = false;
    }
}

        [RelayCommand]
        private async Task GoToRegisterAsync()
        {
            try
            {
                var registerPage = new RegisterPage(
                    new RegisterViewModel(_authService)
                );
                
                var window = GetCurrentWindow();
                if (window?.Page != null)
                {
                    await window.Page.Navigation.PushAsync(registerPage);
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
            }
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            var page = new ForgotPasswordPage(
                new ForgotPasswordViewModel(_authService)
            );
            
            var window = GetCurrentWindow();
            if (window?.Page != null)
            {
                await window.Page.Navigation.PushAsync(page);
            }
        }

        private async Task NavigateBasedOnRole(User? user)
        {
            if (user == null)
            {
                await Shell.Current.GoToAsync("LoginPage");
                return;
            }

            var status = await _authService.GetUserStatusAsync(user.UserId);
            
            var window = GetCurrentWindow();
            if (window?.Page == null) return;
            
            switch (status.status)
            {
                case "pending_group":
                    var pendingGroupPage = new PendingGroupPage();
                    await window.Page.Navigation.PushAsync(pendingGroupPage);
                    break;
                    
                case "pending_approval":
                    var pendingApprovalPage = new PendingApprovalPage();
                    await window.Page.Navigation.PushAsync(pendingApprovalPage);
                    break;
                    
                case "active":
                    switch (user.Role?.RoleName)
                    {
                        case "Student":
                            var studentPage = new StudentDashboardPage();
                            await window.Page.Navigation.PushAsync(studentPage);
                            break;
                        case "Teacher":
                            var teacherPage = new TeacherDashboardPage();
                            await window.Page.Navigation.PushAsync(teacherPage);
                            break;
                        case "Admin":
                            // Используем полное имя с пространством имён
                            var dbContext = App.Current?.Handler?.MauiContext?.Services.GetService<AppDbContext>();
                            if (dbContext != null)
                            {
                                var adminVM = new AdminDashboardViewModel(dbContext, _authService);
                                var adminPage = new OnlineTestingApp.Views.Admin.AdminDashboardPage(adminVM);
                                await window.Page.Navigation.PushAsync(adminPage);
                            }
                            break;
                        default:
                            var defaultPage = new StudentDashboardPage();
                            await window.Page.Navigation.PushAsync(defaultPage);
                            break;
                    }
                    break;
                    
                default:
                    await Shell.Current.GoToAsync("LoginPage");
                    break;
            }
        }

        // Вспомогательные методы для работы с устаревшим API
        private Window? GetCurrentWindow()
        {
            return Application.Current?.Windows.FirstOrDefault();
        }

        private async Task ShowAlertAsync(string title, string message)
        {
            var window = GetCurrentWindow();
            if (window?.Page != null)
            {
                await window.Page.DisplayAlert(title, message, "OK");
            }
        }
    }
}