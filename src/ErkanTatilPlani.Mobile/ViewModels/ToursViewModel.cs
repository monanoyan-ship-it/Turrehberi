using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class TourListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }

    public string PriceDisplay => $"{Price:N0} TL";
    public string DurationDisplay => $"{DurationDays} Gun";
    public string RatingDisplay => AverageRating > 0 ? $"{AverageRating:N1}" : "-";
}

public class ToursViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _searchText = string.Empty;

    public ObservableCollection<TourListItem> Tours { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public ICommand LoadToursCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand TourTappedCommand { get; }

    public ToursViewModel()
    {
        Title = "Turlar";
        _apiService = new ApiService();
        LoadToursCommand = new Command(async () => await LoadToursAsync());
        RefreshCommand = new Command(async () => await LoadToursAsync());
        SearchCommand = new Command(async () => await LoadToursAsync());
        TourTappedCommand = new Command<TourListItem>(async (tour) => await GoToDetailAsync(tour));
    }

    private async Task LoadToursAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Tours.Clear();

            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
            var tours = await _apiService.GetToursAsync(search);

            // Cache for offline
            Preferences.Set("tours_cache", JsonSerializer.Serialize(tours));

            foreach (var t in tours)
            {
                Tours.Add(ParseTour(t));
            }
        }
        catch (Exception)
        {
            // Offline fallback
            var cached = Preferences.Get("tours_cache", null as string);
            if (cached != null)
            {
                var tours = JsonSerializer.Deserialize<List<JsonElement>>(cached);
                if (tours != null)
                {
                    foreach (var t in tours)
                        Tours.Add(ParseTour(t));
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Hata", "Turlar yuklenemedi. Internet baglantinizi kontrol edin.", "Tamam");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoToDetailAsync(TourListItem tour)
    {
        if (tour == null) return;
        await Shell.Current.GoToAsync($"TourDetailPage?tourId={tour.Id}");
    }

    private static TourListItem ParseTour(JsonElement t)
    {
        return new TourListItem
        {
            Id = t.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
            Name = t.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
            Destination = t.TryGetProperty("destination", out var dest) ? dest.GetString() ?? "" : "",
            Description = t.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
            DurationDays = t.TryGetProperty("durationDays", out var dur) ? dur.GetInt32() : 0,
            Price = t.TryGetProperty("price", out var price) ? price.GetDecimal() : 0,
            CompanyName = t.TryGetProperty("companyName", out var comp) ? comp.GetString() ?? "" : "",
            AverageRating = t.TryGetProperty("averageRating", out var rating) ? rating.GetDecimal() : 0
        };
    }
}
