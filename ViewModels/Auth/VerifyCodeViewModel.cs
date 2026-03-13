using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Services;
using OnlineTestingApp.Views.Auth;

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

                System.Diagnostics.Debug.WriteLine($"🔵 Проверка кода: Email={Email}, Code={Code}");
                
                var result = await _authService.VerifyResetCodeAsync(Email, Code);

                System.Diagnostics.Debug.WriteLine($"✅ Результат: success={result.success}, message={result.message}");

                if (result.success)
                {
                    var resetPasswordVM = new ResetPasswordViewModel(_authService);
                    resetPasswordVM.Email = Email;
                    resetPasswordVM.Code = Code;
                    
                    var resetPasswordPage = new ResetPasswordPage(resetPasswordVM);
                    
                    await Application.Current.MainPage.Navigation.PushAsync(resetPasswordPage);
                }
                else
                {
                    Message = result.message;
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка: {ex.Message}");
                Message = $"Ошибка: {ex.Message}";
                HasError = true;
                
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}