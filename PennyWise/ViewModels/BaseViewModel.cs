using PennyWise.Core;

namespace PennyWise.ViewModels;

public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private string _title = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        protected set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    public bool IsNotBusy => !IsBusy;

    public string Title
    {
        get => _title;
        protected set => SetProperty(ref _title, value);
    }

    /// <summary>Shell can be null on app start or during shutdown.</summary>
    protected static INavigation Navigation
        => Shell.Current?.Navigation ?? throw new InvalidOperationException("Navigace není k dispozici.");
}
