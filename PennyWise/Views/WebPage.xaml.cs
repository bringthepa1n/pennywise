using PennyWise.ViewModels;

namespace PennyWise.Views;

public partial class WebPage : ContentPage
{
    private readonly WebViewModel _viewModel;

    public WebPage(WebViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(WebViewModel.ReloadToken))
            {
                CnBWebView.Source = _viewModel.Address;
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.IsLoading = true;
    }

    private async void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        _viewModel.IsLoading = true;

        if (_viewModel.AllowNavigation(e.Url))
        {
            return;
        }

        e.Cancel = true;
        _viewModel.IsLoading = false;

        await DisplayAlert(
            "Stránka blokována",
            "Aplikace povoluje pouze stránky České národní banky (www.cnb.cz). "
            + $"Přenos na {Services.WebViewPolicy.DescribeHost(e.Url ?? string.Empty)} nebyl proveden.",
            "OK");
    }

    /// <summary>Loads an external URL on purpose so the whitelist can be demonstrated.</summary>
    private void OnTestBlockingClicked(object? sender, EventArgs e)
        => CnBWebView.Source = _viewModel.ExternalProbeUrl;

    private void OnNavigated(object? sender, WebNavigatedEventArgs e)
    {
        _viewModel.IsLoading = false;

        if (e.Result != WebNavigationResult.Success)
        {
            _viewModel.ReportLoadError(
                "Kurzovní lístek se nepodařilo načíst. Zkontroluj připojení k internetu a zkus to znovu.");
        }
    }
}
