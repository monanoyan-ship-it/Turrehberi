using ErkanTatilPlani.Mobile.Views;

namespace ErkanTatilPlani.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("TourDetailPage", typeof(TourDetailPage));
    }
}
