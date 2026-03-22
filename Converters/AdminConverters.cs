using System.Globalization;
using OnlineTestingApp.ViewModels.Admin;

namespace OnlineTestingApp.Converters
{
    public class FirstLetterConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
                return str.Substring(0, 1).ToUpper();
            return "?";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FilterBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is string filter)
                {
                    var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (page?.BindingContext is UserManagementViewModel vm)
                    {
                        return vm.SelectedFilter == filter ? Color.FromArgb("#2DD4BF") : Color.FromArgb("#F3F4F6");
                    }
                }
            }
            catch { }
            
            return Color.FromArgb("#F3F4F6");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FilterTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is string filter)
                {
                    var page = Application.Current?.Windows.FirstOrDefault()?.Page;
                    if (page?.BindingContext is UserManagementViewModel vm)
                    {
                        return vm.SelectedFilter == filter ? Colors.White : Color.FromArgb("#475569");
                    }
                }
            }
            catch { }
            
            return Color.FromArgb("#475569");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StatusBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
            return isActive ? Color.FromArgb("#2DD4BF") : Color.FromArgb("#F59E0B");
        return Color.FromArgb("#F59E0B");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}

    public class ToggleStatusIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isActive)
                return isActive ? "🔒" : "🔓";
            return "🔒";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ToggleStatusColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive)
            return isActive ? Color.FromArgb("#F59E0B") : Color.FromArgb("#2DD4BF");
        return Color.FromArgb("#F59E0B");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}

    public class IsTeacherConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() == "Teacher";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}