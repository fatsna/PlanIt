using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RunNow.Behaviors
{
    public static class NumericInputBehavior
    {
        private static readonly Regex _onlyDigitsRegex = new Regex("[^0-9]+");

        public static bool GetIsNumeric(DependencyObject obj) =>
            (bool)obj.GetValue(IsNumericProperty);

        public static void SetIsNumeric(DependencyObject obj, bool value) =>
            obj.SetValue(IsNumericProperty, value);

        public static readonly DependencyProperty IsNumericProperty =
            DependencyProperty.RegisterAttached(
                "IsNumeric",
                typeof(bool),
                typeof(NumericInputBehavior),
                new UIPropertyMetadata(false, OnIsNumericChanged));

        private static void OnIsNumericChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                    textBox.PreviewTextInput += TextBox_PreviewTextInput;
                else
                    textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            }
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _onlyDigitsRegex.IsMatch(e.Text);
        }
    }
}
