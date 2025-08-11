using CommunityToolkit.Mvvm.ComponentModel;

public class NavigationStore
{
    private ObservableObject _currentViewModel; // 현재 뷰모델
    public ObservableObject previousViewModel;  // 이전 뷰모델

    public ObservableObject CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            // 현재 뷰모델을 이전 뷰모델로 백업
            this.previousViewModel = _currentViewModel;

            _currentViewModel = value;
            CurrentViewModelChanged?.Invoke();
        }
    }

    public event Action CurrentViewModelChanged;
}
