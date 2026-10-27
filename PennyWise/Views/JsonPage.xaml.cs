using PennyWise.ViewModels;

namespace PennyWise.Views;

public partial class JsonPage : ContentPage
{
    private readonly JsonViewModel _viewModel;

    public JsonPage(JsonViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.ReloadAsync();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await _viewModel.ReloadAsync();

        if (sender is RefreshView refreshView)
        {
            refreshView.IsRefreshing = false;
        }
    }
}
