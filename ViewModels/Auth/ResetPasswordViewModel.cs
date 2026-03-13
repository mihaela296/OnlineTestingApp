using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Services;

namespace OnlineTestingApp.ViewModels.Auth
{
    [QueryProperty(nameof(Email), "email")]
    [QueryProperty(nameof(Code), "code")]
    public partial class ResetPasswordViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private string _newPassword = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private bool _isSuccess;

        [ObservableProperty]
        private bool _isPasswordVisible;

        [ObservableProperty]
        private bool _isConfirmPasswordVisible;

        public ResetPasswordViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task ResetPasswordAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword.Length < 6)
            {
                Message = "Пароль должен быть не менее 6 символов";
                HasError = true;
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                Message = "Пароли не совпадают";
                HasError = true;
                return;
            }

            try
            {
                IsBusy = true;
                HasError = false;

                var result = await _authService.ResetPasswordAsync(Email, NewPassword);

                if (result.success)
                {
                    Message = result.message;
                    IsSuccess = true;
                    
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("///LoginPage");
                }
                else
                {
                    Message = result.message;
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                Message = $"Ошибка: {ex.Message}";
                HasError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void TogglePasswordVisibility() => IsPasswordVisible = !IsPasswordVisible;

        [RelayCommand]
        private void ToggleConfirmPasswordVisibility() => IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
    }
}
