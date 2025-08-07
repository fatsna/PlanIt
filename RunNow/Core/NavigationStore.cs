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

<<<<<<< HEAD
    private ObservableObject previousViewModel;
=======
    public class ShareDataServiec() {
        public string UserId { get; set; }
        public string UserName { get; set; }

    }
>>>>>>> 00e8723dd4f6cbad5e5e3b1d2d6e3ef3252b8926
}
