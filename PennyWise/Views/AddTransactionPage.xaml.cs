using PennyWise.ViewModels;

namespace PennyWise.Views;

public partial class AddTransactionPage : ContentPage
{
    private readonly AddTransactionViewModel _viewModel;
    private bool _isSyncingSlider;
    private bool _hasUnsavedChanges;

    public AddTransactionPage(AddTransactionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AddTransactionViewModel.FormTitle)
                || args.PropertyName == nameof(AddTransactionViewModel.AmountText))
            {
                _hasUnsavedChanges = !string.IsNullOrWhiteSpace(_viewModel.FormTitle)
                                     || !string.IsNullOrWhiteSpace(_viewModel.AmountText);
            }
        };
    }

    /// <summary>
    /// The Slider only knows doubles. Setting Value from inside ValueChanged fires
    /// the handler again, so the flag stops the recursion after one rounding pass.
    /// </summary>
    private void OnPriorityValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_isSyncingSlider)
        {
            return;
        }

        var rounded = Math.Round(e.NewValue, MidpointRounding.AwayFromZero);

        if (Math.Abs(rounded - e.NewValue) > 0.0001)
        {
            _isSyncingSlider = true;
            try
            {
                PrioritySlider.Value = rounded;
            }
            finally
            {
                _isSyncingSlider = false;
            }
        }

        _viewModel.Priority = rounded;
    }

    private void OnTitleTextChanged(object? sender, TextChangedEventArgs e)
        => _hasUnsavedChanges = !string.IsNullOrWhiteSpace(e.NewTextValue);

    private void OnAmountTextChanged(object? sender, TextChangedEventArgs e)
        => _hasUnsavedChanges = !string.IsNullOrWhiteSpace(e.NewTextValue);

    private void OnOptionToggled(object? sender, EventArgs e)
    {
        // Live feedback for the toggles is shown in the status line.
        _viewModel.RefreshOptionSummary();
    }

    protected override bool OnBackButtonPressed()
    {
        if (!_hasUnsavedChanges || _viewModel.IsBusy)
        {
            return base.OnBackButtonPressed();
        }

        Dispatcher.Dispatch(async () =>
        {
            var leave = await DisplayAlert("Zahodit záznam?", "Rozepsaná data se neuloží.", "Zahodit", "Zůstat");
            if (leave)
            {
                _hasUnsavedChanges = false;
                await Shell.Current.GoToAsync("..");
            }
        });

        return true;
    }
}
