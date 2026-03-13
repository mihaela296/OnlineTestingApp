using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Models;
using OnlineTestingApp.Models.Auth;
using OnlineTestingApp.Services;
using System;
using System.Threading.Tasks;
using OnlineTestingApp.Views.Auth;

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
            await Shell.Current.GoToAsync("RegisterPage");
        }

        [RelayCommand]
        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private async Task NavigateBasedOnRole(User? user)
        {
            if (user == null)
            {
                await Shell.Current.GoToAsync("LoginPage");
                return;
            }

            var status = await _authService.GetUserStatusAsync(user.UserId);
            
            switch (status.status)
            {
                case "pending_group":
                    await Shell.Current.GoToAsync("PendingGroupPage");
                    break;
                case "pending_approval":
                    await Shell.Current.GoToAsync("PendingApprovalPage");
                    break;
                case "active":
                    switch (user.Role?.RoleName)
                    {
                        case "Student":
                            // Заменяем текущую страницу на дашборд студента
                            Application.Current.MainPage = new AppShell();
                            await Shell.Current.GoToAsync("StudentDashboardPage");
                            break;
                        case "Teacher":
                            Application.Current.MainPage = new AppShell();
                            await Shell.Current.GoToAsync("TeacherDashboardPage");
                            break;
                        case "Admin":
                            Application.Current.MainPage = new AppShell();
                            await Shell.Current.GoToAsync("AdminDashboardPage");
                            break;
                        default:
                            Application.Current.MainPage = new AppShell();
                            await Shell.Current.GoToAsync("StudentDashboardPage");
                            break;
                    }
                    break;
                default:
                    await Shell.Current.GoToAsync("LoginPage");
                    break;
            }
        }
                [RelayCommand]
        private async Task ForgotPasswordAsync()
        {
            var page = new ForgotPasswordPage(
                new ForgotPasswordViewModel(_authService)
            );
            await Shell.Current.Navigation.PushAsync(page);
        }

    }
}
