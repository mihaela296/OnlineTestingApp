using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using OnlineTestingApp.Data;
using OnlineTestingApp.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace OnlineTestingApp.ViewModels.Admin
{
    public partial class EditUserViewModel : ObservableObject
    {
        private readonly AppDbContext _dbContext;
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

        public EditUserViewModel(AppDbContext dbContext, UserItem user)
        {
            _dbContext = dbContext;
            _originalUser = user;
            User = user;

            EditUsername = user.Username;
            EditFirstName = user.FirstName ?? string.Empty;
            EditLastName = user.LastName ?? string.Empty;
            EditEmail = user.Email;
            EditPhoneNumber = user.PhoneNumber ?? string.Empty;
            EditRole = user.Role;
            EditIsActive = user.IsActive;
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
            ValidateUsername();
        }

        partial void OnEditEmailChanged(string value)
        {
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
        }

        private void UpdateIsValid()
        {
            IsValid = IsUsernameValid && IsEmailValid;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
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
                        return;
                    }
                }

                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == EditEmail && u.UserId != User.UserId);
                
                if (existingUser != null)
                {
                    ErrorMessage = "Пользователь с таким email уже существует";
                    HasError = true;
                    return;
                }

                existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == EditUsername && u.UserId != User.UserId);
                
                if (existingUser != null)
                {
                    ErrorMessage = "Пользователь с таким именем уже существует";
                    HasError = true;
                    return;
                }

                var dbUser = await _dbContext.Users
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.UserId == User.UserId);

                if (dbUser == null)
                {
                    await ShowAlertAsync("Ошибка", "Пользователь не найден в базе данных");
                    return;
                }

                var role = await _dbContext.Roles
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
                    _dbContext.Profiles.Add(dbUser.Profile);
                }
                else
                {
                    dbUser.Profile.FirstName = EditFirstName;
                    dbUser.Profile.LastName = EditLastName;
                    dbUser.Profile.PhoneNumber = cleanPhone;
                }

                await _dbContext.SaveChangesAsync();

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