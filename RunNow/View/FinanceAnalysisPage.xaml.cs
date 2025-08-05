using System;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace ByeCompany.Pages
{
    public partial class FinanceAnalysisPage : Page
    {
        public FinanceAnalysisPage()
        {
            InitializeComponent();
        }

        private void Calculate_Click(object sender, RoutedEventArgs e)
        {
            // 모든 항목 입력 여부 확인
            if (!double.TryParse(AssetsBox.Text, out double assets) ||
                !double.TryParse(IncomeBox.Text, out double income) ||
                !double.TryParse(FixedExpenseBox.Text, out double fixedExpense) ||
                !double.TryParse(VariableExpenseBox.Text, out double variableExpense))
            {
                WarningText.Text = "⚠ 모든 항목을 올바르게 입력해주세요.";
                SurvivalMonthText.Text = "";
                SavingAmountText.Text = "";
                return;
            }

            double totalExpense = fixedExpense + variableExpense;
            double saving = income - totalExpense;
            double survivalMonths = totalExpense > 0 ? assets / totalExpense : 0;

            // 결과 출력
            SurvivalMonthText.Text = $"💰 생존 가능 개월 수: {Math.Floor(survivalMonths)}개월";
            SavingAmountText.Text = $"📈 월 예상 저축액: {saving:N1}만원";

            // 경고 문구 표시 조건
            if (saving < 0)
            {
                WarningText.Text = "⚠ 지출이 수입보다 많습니다. 지출 구조를 재검토하세요.";
            }
            else if (saving == 0)
            {
                WarningText.Text = "⚠ 수입과 지출이 동일합니다. 여유 자산이 없습니다.";
            }
            else
            {
                WarningText.Text = ""; // 정상 상태일 경우 경고 제거
            }
        }

        private void NumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
            {
                // 숫자만 허용
                e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
            }

        private void NumberBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(text, "^[0-9,]+$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void NumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string text = textBox.Text.Replace(",", "");

            if (double.TryParse(text, out double number))
            {
                textBox.TextChanged -= NumberBox_TextChanged; // 이벤트 중첩 방지
                textBox.Text = string.Format(CultureInfo.InvariantCulture, "{0:N0}", number);
                textBox.CaretIndex = textBox.Text.Length;
                textBox.TextChanged += NumberBox_TextChanged;
            }
        }

    }
}
