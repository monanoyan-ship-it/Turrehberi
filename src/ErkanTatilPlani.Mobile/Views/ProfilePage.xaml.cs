using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ApiService _apiService;

    public ProfilePage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UserNameLabel.Text = _apiService.UserName ?? "Kullanici";
        UserEmailLabel.Text = $"ID: {_apiService.UserId}";
    }

    private async void OnReservationsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ReservationsPage");
    }

    private void OnLogoutClicked(object? sender, EventArgs e)
    {
        _apiService.Logout();
        Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}
