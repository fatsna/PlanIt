using CommunityToolkit.Mvvm.ComponentModel;

public class NavigationStore
{
    private ObservableObject _currentViewModel;
    public ObservableObject CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel = value;
            CurrentViewModelChanged?.Invoke();
        }
    }

    public event Action CurrentViewModelChanged;

    private ObservableObject previousViewModel;
}
