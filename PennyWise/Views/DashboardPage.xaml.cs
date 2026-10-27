using PennyWise.ViewModels;

namespace PennyWise.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    private readonly Dictionary<Button, string> _filterButtons = new();

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        BuildFilterChips();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        UpdateChipStates();
    }

    /// <summary>
    /// The assignment asks for a wrapping FlexLayout of category buttons; they are
    /// generated here because a BindableLayout gives us no handle for the
    /// selected/unselected style swap.
    /// </summary>
    private void BuildFilterChips()
    {
        foreach (var category in _viewModel.Categories)
        {
            var button = new Button
            {
                Text = category,
            };

            button.SetDynamicResource(StyleProperty, "FilterChip");
            button.Clicked += (_, _) =>
            {
                _viewModel.ToggleCategory(category);
                UpdateChipStates();
            };

            _filterButtons[button] = category;

            var margin = new Thickness(0, 0, 8, 8);
            button.Margin = margin;
            FilterFlex.Add(button);
        }
    }

    private void UpdateChipStates()
    {
        foreach (var (button, category) in _filterButtons)
        {
            var selected = _viewModel.IsCategorySelected(category);
            button.SetDynamicResource(StyleProperty, selected ? "FilterChipSelected" : "FilterChip");
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardViewModel.FilterSummary))
        {
            UpdateChipStates();
        }
    }
}
