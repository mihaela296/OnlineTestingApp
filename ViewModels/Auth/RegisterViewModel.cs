using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Models.Auth;
using OnlineTestingApp.Services;
using OnlineTestingApp.Views.Auth;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

                // Валидация имени пользователя - минимум 2 символа
                if (RegisterModel.Username.Length < 2)
                {
                    ErrorMessage = "Имя пользователя должно содержать минимум 2 символа";
                    HasError = true;
                    return;
                }

                if (!IsValidEmail(RegisterModel.Email))
                {
                    ErrorMessage = "Введите корректный email адрес";
                    HasError = true;
                    return;
                }

                if (RegisterModel.Password.Length < 6)
                {
                    ErrorMessage = "Пароль должен содержать минимум 6 символов";
                    HasError = true;
                    return;
                }

                // Валидация телефона (если указан)
                if (!string.IsNullOrWhiteSpace(RegisterModel.PhoneNumber))
                {
                    string normalized = AuthService.NormalizePhoneNumber(RegisterModel.PhoneNumber);
                    if (string.IsNullOrEmpty(normalized))
                    {
                        ErrorMessage = "Введите корректный номер телефона";
                        HasError = true;
                        return;
                    }
                    
                    var digitsOnly = new string(normalized.Where(char.IsDigit).ToArray());
                    if (digitsOnly.Length != 11)
                    {
                        ErrorMessage = "Номер телефона должен содержать 11 цифр";
                        HasError = true;
                        return;
                    }
                    
                    RegisterModel.PhoneNumber = normalized;
                }

                var result = await _authService.RegisterAsync(RegisterModel);
                
                if (!result.success)
                {
                    ErrorMessage = result.message;
                    HasError = true;
                    return;
                }

                var window = Application.Current?.Windows.FirstOrDefault();
                if (window?.Page != null)
                {
                    await window.Page.DisplayAlert("Успешно!", result.message, "OK");
                    
                    var loginPage = new LoginPage(new LoginViewModel(_authService));
                    await window.Page.Navigation.PushAsync(loginPage);
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
                
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window?.Page != null)
                {
                    await window.Page.Navigation.PushAsync(loginPage);
                }
            }
            catch (Exception ex)
            {
                var window = Application.Current?.Windows.FirstOrDefault();
                if (window?.Page != null)
                {
                    await window.Page.DisplayAlert("Ошибка", ex.Message, "OK");
                }
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}