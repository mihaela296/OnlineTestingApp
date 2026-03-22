using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static OnlineTestingApp.ViewModels.Admin.UserManagementViewModel;

namespace OnlineTestingApp.ViewModels.Admin
{
    public partial class EditUserViewModel : ObservableObject
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly UserItem _originalUser;

        [ObservableProperty]
        private UserItem _user;

        [ObservableProperty]
        private string _editUsername;

        [ObservableProperty]
        private string _editFirstName;

        [ObservableProperty]
        private string _editLastName;

        [ObservableProperty]
        private string _editEmail;

        [ObservableProperty]
        private string _editPhoneNumber;

        [ObservableProperty]
        private string _editRole;

        [ObservableProperty]
        private bool _editIsActive;

        [ObservableProperty]
        private ObservableCollection<string> _roles = new() { "Student", "Teacher", "Admin" };

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isValid = true;

        [ObservableProperty]
        private bool _isUsernameValid = true;

        [ObservableProperty]
        private bool _isEmailValid = true;

        public string StatusText => EditIsActive ? "Активен" : "Заблокирован";

        public EditUserViewModel(IDbContextFactory<AppDbContext> dbContextFactory, UserItem user)
        {
            _dbContextFactory = dbContextFactory;
            _originalUser = user;
            User = user;

            EditUsername = user.Username;
            EditFirstName = user.FirstName ?? string.Empty;
            EditLastName = user.LastName ?? string.Empty;
            EditEmail = user.Email;
            EditPhoneNumber = user.PhoneNumber ?? string.Empty;
            EditRole = user.Role;
            EditIsActive = user.IsActive;
            
            // Проверяем валидность при загрузке
            ValidateUsername();
            ValidateEmail();
            
            System.Diagnostics.Debug.WriteLine($"EditUserViewModel создан: IsValid={IsValid}, Username={EditUsername}, Email={EditEmail}");
        }

        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;
            
            var digitsOnly = Regex.Replace(phone, @"[^\d]", "");
            
            if (digitsOnly.StartsWith("8") && digitsOnly.Length == 11)
            {
                digitsOnly = "7" + digitsOnly.Substring(1);
            }
            
            return digitsOnly.Length == 11 ? $"+{digitsOnly}" : phone;
        }

        partial void OnEditUsernameChanged(string value)
        {
            System.Diagnostics.Debug.WriteLine($"Username изменен: {value}");
            ValidateUsername();
        }

        partial void OnEditEmailChanged(string value)
        {
            System.Diagnostics.Debug.WriteLine($"Email изменен: {value}");
            ValidateEmail();
        }

        partial void OnEditIsActiveChanged(bool value)
        {
            OnPropertyChanged(nameof(StatusText));
        }

        private void ValidateUsername()
        {
            IsUsernameValid = !string.IsNullOrWhiteSpace(EditUsername) && EditUsername.Length >= 3;
            UpdateIsValid();
            System.Diagnostics.Debug.WriteLine($"ValidateUsername: IsUsernameValid={IsUsernameValid}");
        }

        private void ValidateEmail()
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(EditEmail);
                IsEmailValid = addr.Address == EditEmail;
            }
            catch
            {
                IsEmailValid = false;
            }
            UpdateIsValid();
            System.Diagnostics.Debug.WriteLine($"ValidateEmail: IsEmailValid={IsEmailValid}");
        }

        private void UpdateIsValid()
        {
            IsValid = IsUsernameValid && IsEmailValid;
            System.Diagnostics.Debug.WriteLine($"UpdateIsValid: IsValid={IsValid}");
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            System.Diagnostics.Debug.WriteLine("SaveAsync ВЫЗВАН!");
            
            try
            {
                IsSaving = true;
                HasError = false;

                string cleanPhone = CleanPhoneNumber(EditPhoneNumber);

                if (!string.IsNullOrWhiteSpace(cleanPhone))
                {
                    var digitsOnly = Regex.Replace(cleanPhone, @"[^\d]", "");
                    if (digitsOnly.Length != 11)
                    {
                        ErrorMessage = "Номер телефона должен содержать 11 цифр";
                        HasError = true;
                        System.Diagnostics.Debug.WriteLine($"Ошибка: {ErrorMessage}");
                        return;
                    }
                }

                // Создаем новый контекст для операции
                using var dbContext = await _dbContextFactory.CreateDbContextAsync();

                var existingUser = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == EditEmail && u.UserId != User.UserId);
                
                if (existingUser != null)
                {
                    ErrorMessage = "Пользователь с таким email уже существует";
                    HasError = true;
                    System.Diagnostics.Debug.WriteLine($"Ошибка: {ErrorMessage}");
                    return;
                }

                existingUser = await dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == EditUsername && u.UserId != User.UserId);
                
                if (existingUser != null)
                {
                    ErrorMessage = "Пользователь с таким именем уже существует";
                    HasError = true;
                    System.Diagnostics.Debug.WriteLine($"Ошибка: {ErrorMessage}");
                    return;
                }

                var dbUser = await dbContext.Users
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.UserId == User.UserId);

                if (dbUser == null)
                {
                    await ShowAlertAsync("Ошибка", "Пользователь не найден в базе данных");
                    System.Diagnostics.Debug.WriteLine("Ошибка: пользователь не найден");
                    return;
                }

                var role = await dbContext.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == EditRole);

                dbUser.Username = EditUsername;
                dbUser.Email = EditEmail;
                dbUser.RoleId = role?.RoleId ?? dbUser.RoleId;
                dbUser.IsActive = EditIsActive;

                if (dbUser.Profile == null)
                {
                    dbUser.Profile = new Profile
                    {
                        UserId = dbUser.UserId,
                        FirstName = EditFirstName,
                        LastName = EditLastName,
                        PhoneNumber = cleanPhone
                    };
                    dbContext.Profiles.Add(dbUser.Profile);
                }
                else
                {
                    dbUser.Profile.FirstName = EditFirstName;
                    dbUser.Profile.LastName = EditLastName;
                    dbUser.Profile.PhoneNumber = cleanPhone;
                }

                await dbContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Сохранение успешно!");

                User.Username = EditUsername;
                User.Email = EditEmail;
                User.FirstName = EditFirstName;
                User.LastName = EditLastName;
                User.PhoneNumber = cleanPhone;
                User.Role = EditRole;
                User.IsActive = EditIsActive;

                await ClosePageAsync(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
                await ShowAlertAsync("Ошибка", $"Не удалось сохранить изменения: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            System.Diagnostics.Debug.WriteLine("CancelAsync вызван!");
            await ClosePageAsync(false);
        }

        [RelayCommand]
        private void ValidateUsernameCommand()
        {
            ValidateUsername();
        }

        [RelayCommand]
        private void ValidateEmailCommand()
        {
            ValidateEmail();
        }

        private async Task ClosePageAsync(bool saved)
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page != null)
            {
                await window.Page.Navigation.PopAsync();
                
                if (saved && window.Page.BindingContext is UserManagementViewModel vm)
                {
                    await vm.LoadUsersAsync();
                }
            }
        }

        private async Task ShowAlertAsync(string title, string message)
        {
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window?.Page != null)
            {
                await window.Page.DisplayAlert(title, message, "OK");
            }
        }
    }
}