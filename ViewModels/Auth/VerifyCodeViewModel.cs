using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Services;

namespace OnlineTestingApp.ViewModels.Auth
{
    [QueryProperty(nameof(Email), "email")]
    public partial class VerifyCodeViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _message = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        public VerifyCodeViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private async Task VerifyCodeAsync()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(Code) || Code.Length != 6)
            {
                Message = "Введите 6-значный код";
                HasError = true;
                return;
            }

            try
            {
                IsBusy = true;
                HasError = false;

                var result = await _authService.VerifyResetCodeAsync(Email, Code);

                if (result.success)
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "email", Email },
                        { "code", Code }
                    };
                    await Shell.Current.GoToAsync("ResetPasswordPage", parameters);
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
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
