namespace ErkanTatilPlani.Mobile;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnToursClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ToursPage");
    }

    private async void OnReservationsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ReservationsPage");
    }
}
