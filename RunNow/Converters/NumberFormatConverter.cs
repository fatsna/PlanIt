using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RunNow.Converters
{
    public class NumberFormatConverter : IValueConverter
    {
        // View → ViewModel (TextBox 입력 → double)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                var raw = str.Replace(",", "");

                // 숫자 형태일 때만 변환
                if (double.TryParse(raw, out double result))
                    return result;

                // ❗ 숫자가 아닌 경우에는 DependencyProperty.UnsetValue 반환
                return DependencyProperty.UnsetValue;
            }

            return DependencyProperty.UnsetValue;
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
