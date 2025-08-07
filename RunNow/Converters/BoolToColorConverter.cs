using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RunNow.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isUser)
                return isUser ? new SolidColorBrush(Color.FromRgb(240, 240, 240)) : new SolidColorBrush(Color.FromRgb(173, 216, 230));

            return new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
