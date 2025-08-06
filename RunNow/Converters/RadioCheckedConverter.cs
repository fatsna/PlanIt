using System;
using System.Globalization;
using System.Windows.Data;

namespace RunNow.Converters
{
    public class RadioCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return int.Parse(parameter.ToString());
            return Binding.DoNothing;
        }
    }
}
