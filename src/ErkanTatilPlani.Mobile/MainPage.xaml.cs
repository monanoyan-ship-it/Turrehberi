namespace ErkanTatilPlani.Mobile;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnToursClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//ToursPage");

    private async void OnCompaniesClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("CompaniesPage");

    private async void OnDealsClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("DealsPage");

    private async void OnBlogClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("BlogPage");

    private async void OnFavoritesClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("FavoritesPage");

    private async void OnMessagesClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("MessagesPage");

    private async void OnFaqClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("FaqPage");

    private async void OnSupportClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("SupportPage");
}
