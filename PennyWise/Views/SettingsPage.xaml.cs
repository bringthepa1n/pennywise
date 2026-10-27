using PennyWise.ViewModels;

namespace PennyWise.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    /// <summary>Live password validation - the VM recalculates the rules on every keystroke.</summary>
    private void OnPasswordTextChanged(object? sender, TextChangedEventArgs e)
        => _viewModel.Password = e.NewTextValue ?? string.Empty;

    /// <summary>The entry is never rebuilt, only its masking is toggled.</summary>
    private void OnShowPasswordToggled(object? sender, ToggledEventArgs e)
    {
        if (sender is Switch toggle)
        {
            _viewModel.ShowPassword = toggle.IsToggled;
        }
    }

    private void OnBudgetValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            // Round to whole hundreds so the label does not flicker with decimals.
            var rounded = Math.Round(e.NewValue / 100) * 100;
            if (Math.Abs(rounded - e.NewValue) > 0.5)
            {
                slider.Value = rounded;
                return;
            }

            _viewModel.BudgetValue = rounded;
        }
    }
}
