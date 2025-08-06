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

    public class ShareDataServiec() {
        public string UserId { get; set; }
        public string UserName { get; set; }

    }
}
