using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Models.Auth;
using OnlineTestingApp.Services;
using OnlineTestingApp.Views.Auth;
using System;
using System.Threading.Tasks;

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

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            
            // Для тестов можно подставить данные
            #if DEBUG
            _loginModel.Email = "ivanov@example.com";
            _loginModel.Password = "hash_12345";
            #endif
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

                // Валидация
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

                // Проверяем статус пользователя
                var status = await _authService.GetUserStatusAsync(result.user!.UserId);
                
                // Навигация в зависимости от статуса
                await NavigateBasedOnStatus(status.status, result.user.UserId);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToRegisterAsync()
        {
            await Shell.Current.GoToAsync("RegisterPage");
        }

        private async Task NavigateBasedOnStatus(string status, int userId)
        {
            switch (status)
            {
                case "pending_group":
                    await Shell.Current.GoToAsync($"PendingGroupPage?userId={userId}");
                    break;
                case "pending_approval":
                    await Shell.Current.GoToAsync($"PendingApprovalPage?userId={userId}");
                    break;
                case "active":
                default:
                    // Здесь позже добавим навигацию на дашборд в зависимости от роли
                    await Shell.Current.GoToAsync("//StudentDashboardPage");
                    break;
            }
        }
    }
}