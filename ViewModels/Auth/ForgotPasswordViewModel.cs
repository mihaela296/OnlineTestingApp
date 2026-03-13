using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Services;

namespace OnlineTestingApp.ViewModels.Auth
{
    public partial class ForgotPasswordViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private bool _hasMessage;

        [ObservableProperty]
        private bool _isSuccess;

        public ForgotPasswordViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task SendResetCodeAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(Email))
                return;

            try
            {
                IsBusy = true;
                HasMessage = false;

                var result = await _authService.RequestPasswordResetAsync(Email);

                Message = result.message;
                IsSuccess = result.success;
                HasMessage = true;

                if (result.success)
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "email", Email }
                    };
                    await Shell.Current.GoToAsync("VerifyCodePage", parameters);
                }
            }
            catch (Exception ex)
            {
                Message = $"Ошибка: {ex.Message}";
                IsSuccess = false;
                HasMessage = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
