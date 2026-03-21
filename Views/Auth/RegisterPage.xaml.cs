using OnlineTestingApp.ViewModels.Auth;
using System.Text.RegularExpressions;

namespace OnlineTestingApp.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private bool _isPasswordVisible = false;
    private bool _isConfirmPasswordVisible = false;
    private bool _isUpdatingPhone = false;
    
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Добавляем обработчик для контроля ввода телефона
        PhoneEntry.TextChanged += OnPhoneTextChanged;
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        
        if (sender is Button button)
        {
            button.Text = _isPasswordVisible ? "👁️‍🗨️" : "👁️";
        }
    }

    private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
    {
        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
        
        if (sender is Button button)
        {
            button.Text = _isConfirmPasswordVisible ? "👁️‍🗨️" : "👁️";
        }
    }

    private void OnPhoneTextChanged(object sender, TextChangedEventArgs e)
{
    if (_isUpdatingPhone) return;
    
    var entry = sender as Entry;
    if (entry == null) return;

    _isUpdatingPhone = true;

    try
    {
        var newText = e.NewTextValue ?? string.Empty;
        
        // Убираем все нецифровые символы
        string digitsOnly = Regex.Replace(newText, @"[^\d]", "");
        
        // Ограничиваем длину до 11 цифр
        if (digitsOnly.Length > 11)
        {
            digitsOnly = digitsOnly.Substring(0, 11);
        }
        
        // Форматируем для отображения
        string formattedNumber = FormatPhoneNumber(digitsOnly);
        
        if (entry.Text != formattedNumber)
        {
            entry.Text = formattedNumber;
            
            if (BindingContext is RegisterViewModel vm)
            {
                vm.RegisterModel.PhoneNumber = formattedNumber;
            }
        }
    }
    finally
    {
        _isUpdatingPhone = false;
    }
}

    private string FormatPhoneNumber(string digits)
    {
        if (string.IsNullOrEmpty(digits))
            return string.Empty;
            
        // Формат: +X (XXX) XXX-XX-XX
        if (digits.Length == 1)
        {
            return $"+{digits[0]}";
        }
        else if (digits.Length == 2)
        {
            return $"+{digits[0]} ({digits[1]}";
        }
        else if (digits.Length == 3)
        {
            return $"+{digits[0]} ({digits.Substring(1, 2)})";
        }
        else if (digits.Length == 4)
        {
            return $"+{digits[0]} ({digits.Substring(1, 3)})";
        }
        else if (digits.Length == 5)
        {
            return $"+{digits[0]} ({digits.Substring(1, 3)}) {digits[4]}";
        }
        else if (digits.Length == 6)
        {
            return $"+{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4, 2)}";
        }
        else if (digits.Length == 7)
        {
            return $"+{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}";
        }
        else if (digits.Length == 8)
        {
            return $"+{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits[7]}";
        }
        else if (digits.Length == 9)
        {
            return $"+{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}";
        }
        else if (digits.Length == 10)
        {
            return $"+{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits[9]}";
        }
        else if (digits.Length == 11)
        {
            return $"+{digits[0]} ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits.Substring(9, 2)}";
        }
        
        return digits;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        PhoneEntry.TextChanged -= OnPhoneTextChanged;
    }
}