using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Services;
using OnlineTestingApp.Views.Auth;
using System.Collections.Generic;
using System;

namespace OnlineTestingApp.ViewModels.Auth
{
    public partial class AccountBlockedViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly string _userEmail;

        [ObservableProperty]
        private string _email = string.Empty;

        public AccountBlockedViewModel(AuthService authService, string email)
        {
            _authService = authService;
            _userEmail = email;
            Email = email;
        }

        [RelayCommand]
        private async Task ContactSupportAsync()
        {
            try
            {
                // Упрощенный вариант - просто показываем информацию
                await ShowAlertAsync("Связь с поддержкой", 
                    $"Для связи с поддержкой отправьте email на адрес:\n\nsupport@onlinetesting.com\n\n" +
                    $"Укажите в письме ваш email: {_userEmail}");
            }
            catch (Exception)
            {
                await ShowAlertAsync("Ошибка", "Не удалось открыть почтовый клиент");
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
                await ShowAlertAsync("Ошибка", ex.Message);
            }
        }

        private async Task ShowAlertAsync(string title, string message)
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page != null)
            {
                await window.Page.DisplayAlertAsync(title, message, "OK");
            }
        }
    }
}