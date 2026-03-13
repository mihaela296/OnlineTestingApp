using System.Globalization;

namespace OnlineTestingApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colors)
            {
                var colorParts = colors.Split(':');
                if (colorParts.Length == 2)
                {
                    return boolValue ? Color.FromArgb(colorParts[0]) : Color.FromArgb(colorParts[1]);
                }
            }
            return Colors.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
