using System;
using System.Globalization;
using System.Windows.Data;

namespace RunNow.Converters
{
    public class NumberFormatConverter : IValueConverter
    {
        // View → ViewModel (TextBox 입력 → double)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && double.TryParse(str.Replace(",", ""), out double result))
                return result;

            return 0d; // 잘못된 입력일 경우 0으로 처리
        }

        // ViewModel → View (double → TextBox 표시)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double num)
                return num.ToString("N0", culture); // 3자리 콤마 포맷

            return "0";
        }
    }
}
