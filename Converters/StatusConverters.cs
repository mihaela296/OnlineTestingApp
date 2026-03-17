using System.Globalization;

namespace OnlineTestingApp.Converters
{
    public class StatusTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isActive)
                return isActive ? "Активен" : "Заблокирован";
            return "Неизвестно";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isActive)
                return isActive ? Color.FromArgb("#10B981") : Color.FromArgb("#EF4444");
            return Color.FromArgb("#6B7280");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}