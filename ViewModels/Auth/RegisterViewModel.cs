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

                // Валидация email
                if (!IsValidEmail(RegisterModel.Email))
                {
                    ErrorMessage = "Введите корректный email адрес";
                    HasError = true;
                    return;
                }

                // Валидация пароля
                if (RegisterModel.Password.Length < 6)
                {
                    ErrorMessage = "Пароль должен содержать минимум 6 символов";
                    HasError = true;
                    return;
                }

                // Валидация телефона (если указан)
                if (!string.IsNullOrWhiteSpace(RegisterModel.PhoneNumber))
                {
                    if (!IsValidRussianPhone(RegisterModel.PhoneNumber))
                    {
                        ErrorMessage = "Введите корректный российский номер телефона (11 цифр)";
                        HasError = true;
                        return;
                    }
                    
                    // Очищаем номер перед сохранением
                    RegisterModel.PhoneNumber = CleanPhoneNumber(RegisterModel.PhoneNumber);
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

        private bool IsValidRussianPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Убираем все нецифровые символы
            var digitsOnly = Regex.Replace(phone, @"[^\d]", "");
            
            // Должно быть ровно 11 цифр
            return digitsOnly.Length == 11;
        }

        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;
            
            // Убираем все нецифровые символы
            var digitsOnly = Regex.Replace(phone, @"[^\d]", "");
            
            // Если первая цифра 8, заменяем на 7
            if (digitsOnly.StartsWith("8") && digitsOnly.Length == 11)
            {
                digitsOnly = "7" + digitsOnly.Substring(1);
            }
            
            // Возвращаем в формате +7XXXXXXXXXX
            return $"+{digitsOnly}";
        }
    }
}