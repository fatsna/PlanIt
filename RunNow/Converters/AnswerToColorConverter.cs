using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RunNow.Converters
{
    public class AnswerToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null)
                return Brushes.Transparent;

            if (values[0] == System.Windows.DependencyProperty.UnsetValue)
                return Brushes.Transparent;

            // values[0] = SelectedAnswerIndex, values[1] = answerText
            if (values[0] is int selectedIndex && values[1] is string answerText)
            {
                var allAnswers = new List<string>
                {
                    "매우그렇다", "그렇다", "보통이다", "아니다", "매우아니다"
                };

                int index = allAnswers.IndexOf(answerText);
                return index == selectedIndex ? Brushes.LightBlue : Brushes.Transparent;
            }

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QuestionAnswerParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is RunNow.Models.QuestionModel question && values[1] is string answer)
            {
                return Tuple.Create(question, answer);
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
