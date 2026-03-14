using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Models.Auth;
using OnlineTestingApp.Services;
using OnlineTestingApp.Views.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineTestingApp.ViewModels.Auth
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private RegisterModel _registerModel = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private List<string> _roles = new() { "Student", "Teacher" };

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
            _registerModel.Role = "Student";
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                HasError = false;
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(RegisterModel.Username) ||
                    string.IsNullOrWhiteSpace(RegisterModel.Email) ||
                    string.IsNullOrWhiteSpace(RegisterModel.Password))
                {
                    ErrorMessage = "Заполните все обязательные поля";
                    HasError = true;
                    return;
                }

                var result = await _authService.RegisterAsync(RegisterModel);
                
                if (!result.success)
                {
                    ErrorMessage = result.message;
                    HasError = true;
                    return;
                }

                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Успешно!", result.message, "OK");
                    
                    var loginPage = new LoginPage(new LoginViewModel(_authService));
                    await Application.Current.MainPage.Navigation.PushAsync(loginPage);
                }
            }
            catch (System.Exception ex)
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
        private async Task GoToLoginAsync()
        {
            try
            {
                var loginPage = new LoginPage(new LoginViewModel(_authService));
                
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(loginPage);
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", ex.Message, "OK");
                }
            }
        }
    }
}