using ErkanTatilPlani.Mobile.Services;
using ErkanTatilPlani.Mobile.Views;

namespace ErkanTatilPlani.Mobile;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var apiService = new ApiService();
        if (apiService.IsLoggedIn)
            return new Window(new AppShell());
        else
            return new Window(new NavigationPage(new LoginPage()));
    }
}
