using PennyWise.Views;

namespace PennyWise;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Screen 2 is deliberately not part of the flyout: it is only reachable
        // through Shell.Current.GoToAsync(nameof(AddTransactionPage)).
        Routing.RegisterRoute(nameof(AddTransactionPage), typeof(AddTransactionPage));
    }
}
