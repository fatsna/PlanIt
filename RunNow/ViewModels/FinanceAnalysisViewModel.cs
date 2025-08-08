using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace RunNow.ViewModels
{
    public partial class FinanceAnalysisViewModel : ObservableObject
    {
        [ObservableProperty] private double totalAssets;
        [ObservableProperty] private double monthlyIncome;
        [ObservableProperty] private double fixedExpense;
        [ObservableProperty] private double variableExpense;

        [ObservableProperty] private string survivalMonth;
        [ObservableProperty] private string savingAmount;
        [ObservableProperty] private string warning;

        [RelayCommand]
        private void Analyze()
        {
            double totalExpense = FixedExpense + VariableExpense;
            double saving = MonthlyIncome - totalExpense;
            double survivalMonths = totalExpense > 0 ? TotalAssets / totalExpense : 0;

            // ✅ 결과 표시
            SurvivalMonth = $"💰 생존 가능 개월 수: {Math.Floor(survivalMonths)}개월";
            SavingAmount = $"📈 월 예상 저축액: {saving:N1}만원";

            if (saving < 0)
            {
                Warning = "⚠ 지출이 수입보다 많습니다. 지출 구조를 재검토하세요.";
            }
            else if (saving == 0)
            {
                Warning = "⚠ 수입과 지출이 동일합니다. 여유 자산이 없습니다.";
            }
            else
            {
                Warning = string.Empty;
            }
        }
    }
}
