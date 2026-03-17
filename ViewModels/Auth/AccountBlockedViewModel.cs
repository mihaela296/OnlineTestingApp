using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OnlineTestingApp.Services;
using OnlineTestingApp.Views.Auth;
using System;

namespace OnlineTestingApp.ViewModels.Auth
{
    public partial class AccountBlockedViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly string _userEmail;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        private const string AdminEmail = "mihaeladorogan298@mail.ru";

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
                IsBusy = true;

                // Просто показываем информацию для копирования
                await ShowAlertAsync("Связь с поддержкой", 
                    $"Для связи с администратором отправьте письмо:\n\n" +
                    $"📧 Кому: {AdminEmail}\n" +
                    $"📝 Тема: Вопрос о блокировке аккаунта {_userEmail}\n\n" +
                    $"И укажите в письме ваш email: {_userEmail}");
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Ошибка", ex.Message);
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