using PennyWise.Core;
using PennyWise.Services;

namespace PennyWise.ViewModels;

/// <summary>Screen 3: live currency rates from the Czech National Bank in a WebView.</summary>
public class WebViewModel : BaseViewModel
{
    private bool _isLoading;
    private string _address = WebViewPolicy.StartUrl;
    private string? _lastError;
    private int _reloadToken;

    public WebViewModel()
    {
        Title = "Online kurzy";
        RetryCommand = new AsyncRelayCommand(RetryAsync);
        GoCommand = new AsyncRelayCommand(GoAsync);
    }

    public AsyncRelayCommand RetryCommand { get; }

    public AsyncRelayCommand GoCommand { get; }

    public string StartUrl => WebViewPolicy.StartUrl;

    /// <summary>URL the WebView should show. Changing it reloads the page.</summary>
    public string Address
    {
        get => _address;
        private set => SetProperty(ref _address, value);
    }

    /// <summary>Increments on every retry so the page can force a reload.</summary>
    public int ReloadToken
    {
        get => _reloadToken;
        private set => SetProperty(ref _reloadToken, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }
    }

    public bool IsNotLoading => !_isLoading;

    public string? LastError
    {
        get => _lastError;
        private set
        {
            if (SetProperty(ref _lastError, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(_lastError);

    /// <summary>True when navigation may continue; otherwise sets the blocking message.</summary>
    public bool AllowNavigation(string? url)
    {
        var allowed = WebViewPolicy.IsAllowed(url);

        if (!allowed)
        {
            LastError = $"Přístup na {WebViewPolicy.DescribeHost(url ?? string.Empty)} je zablokován. " +
                        "Aplikace povoluje pouze stránky ČNB.";
        }

        return allowed;
    }

    public void ReportLoadError(string message)
    {
        IsLoading = false;
        LastError = message;
    }

    private Task RetryAsync()
    {
        LastError = null;
        Address = WebViewPolicy.StartUrl;
        ReloadToken++;
        return Task.CompletedTask;
    }

    private Task GoAsync()
    {
        if (AllowNavigation(Address))
        {
            ReloadToken++;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Demonstrates the whitelist: asks the WebView to open an external site.
    /// The Navigating handler cancels it and shows the alert.
    /// </summary>
    public string ExternalProbeUrl => "https://www.google.com/";
}
