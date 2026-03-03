using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class FavoriteTourItem
{
    public int Id { get; set; }
    public int TourId { get; set; }
    public string TourName { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public string PriceDisplay => $"{Price:N0} TL";
}

public class FavoritesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<FavoriteTourItem> Favorites { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand RemoveFavoriteCommand { get; }
    public ICommand TourTappedCommand { get; }

    public FavoritesViewModel()
    {
        Title = "Favorilerim";
        _apiService = new ApiService();
        LoadCommand = new Command(async () => await LoadFavoritesAsync());
        RefreshCommand = new Command(async () => await LoadFavoritesAsync());
        RemoveFavoriteCommand = new Command<FavoriteTourItem>(async (fav) => await RemoveFavoriteAsync(fav));
        TourTappedCommand = new Command<FavoriteTourItem>(async (fav) => await GoToTourAsync(fav));
    }

    private async Task LoadFavoritesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Favorites.Clear();

            if (!_apiService.IsLoggedIn)
            {
                await Shell.Current.DisplayAlert("Uyari", "Favorilerinizi gormek icin giris yapin", "Tamam");
                return;
            }

            var favorites = await _apiService.GetFavoritesAsync();

            foreach (var f in favorites)
            {
                Favorites.Add(new FavoriteTourItem
                {
                    Id = f.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    TourId = f.TryGetProperty("tourId", out var tourId) ? tourId.GetInt32() : 0,
                    TourName = f.TryGetProperty("tourName", out var name) ? name.GetString() ?? "" : "",
                    Destination = f.TryGetProperty("destination", out var dest) ? dest.GetString() ?? "" : "",
                    Price = f.TryGetProperty("price", out var price) ? price.GetDecimal() : 0
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Favoriler yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveFavoriteAsync(FavoriteTourItem fav)
    {
        if (fav == null) return;

        var confirm = await Shell.Current.DisplayAlert("Onay", $"{fav.TourName} favorilerden cikarilsin mi?", "Evet", "Hayir");
        if (!confirm) return;

        try
        {
            var (success, error) = await _apiService.RemoveFavoriteAsync(fav.Id);
            if (success)
            {
                Favorites.Remove(fav);
            }
            else
            {
                await Shell.Current.DisplayAlert("Hata", error ?? "Favori silinemedi", "Tamam");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Hata: {ex.Message}", "Tamam");
        }
    }

    private async Task GoToTourAsync(FavoriteTourItem fav)
    {
        if (fav == null) return;
        await Shell.Current.GoToAsync($"TourDetailPage?tourId={fav.TourId}");
    }
}
