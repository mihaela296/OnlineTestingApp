using System.Globalization;

namespace OnlineTestingApp.Converters
{
    public class BoolToEyeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isVisible)
            {
                return isVisible ? "👁️‍🗨️" : "👁️";
            }
            return "👁️";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
