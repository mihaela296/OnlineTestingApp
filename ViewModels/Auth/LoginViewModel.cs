using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Models.Auth;
using OnlineTestingApp.Services;
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

                var status = await _authService.GetUserStatusAsync(result.user!.UserId);
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
        private async Task GoToRegisterAsync()
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
                    await Shell.Current.GoToAsync("//StudentDashboardPage");
                    break;
            }
        }
    }
}
