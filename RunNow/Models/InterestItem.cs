using CommunityToolkit.Mvvm.ComponentModel;

namespace RunNow.Models
{
    // ✅ MVVM Toolkit을 쓰기 위해 ObservableObject 상속
    public partial class InterestItem : ObservableObject
    {
        // 화면에 표시할 이름
        public string Name { get; set; } = string.Empty;

        // 그룹(섹션 헤더)
        public string Group { get; set; } = string.Empty;

        // 체크 상태 (양방향 바인딩용)
        [ObservableProperty]
        private bool isSelected;
    }
}
