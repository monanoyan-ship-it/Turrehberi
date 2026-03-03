using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class NearbyTourItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceKm { get; set; }

    public string PriceDisplay => $"{Price:N0} TL";
    public string DistanceDisplay => DistanceKm < 1 ? $"{DistanceKm * 1000:N0} m" : $"{DistanceKm:N1} km";
}

public class NearbyViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _locationStatus = "Konum alinamadi";

    public ObservableCollection<NearbyTourItem> NearbyTours { get; } = new();

    public string LocationStatus
    {
        get => _locationStatus;
        set => SetProperty(ref _locationStatus, value);
    }

    public ICommand LoadNearbyCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand TourTappedCommand { get; }

    public NearbyViewModel()
    {
        Title = "Yakinimdaki Turlar";
        _apiService = new ApiService();
        LoadNearbyCommand = new Command(async () => await LoadNearbyToursAsync());
        RefreshCommand = new Command(async () => await LoadNearbyToursAsync());
        TourTappedCommand = new Command<NearbyTourItem>(async (tour) =>
        {
            if (tour != null)
                await Shell.Current.GoToAsync($"TourDetailPage?tourId={tour.Id}");
        });
    }

    private async Task LoadNearbyToursAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            NearbyTours.Clear();
            LocationStatus = "Konum aliniyor...";

            // Get current location
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location == null)
            {
                // Try cached location
                location = await Geolocation.GetLastKnownLocationAsync();
            }

            if (location == null)
            {
                LocationStatus = "Konum alinamadi. Konum izni verin.";
                return;
            }

            LocationStatus = $"Konumunuz: {location.Latitude:N4}, {location.Longitude:N4}";

            // Get all tours
            var tours = await _apiService.GetToursAsync();

            var nearbyTours = new List<NearbyTourItem>();
            foreach (var t in tours)
            {
                var lat = t.TryGetProperty("latitude", out var latEl) && latEl.ValueKind == JsonValueKind.Number
                    ? latEl.GetDouble() : 0;
                var lng = t.TryGetProperty("longitude", out var lngEl) && lngEl.ValueKind == JsonValueKind.Number
                    ? lngEl.GetDouble() : 0;

                double distance = 999;
                if (lat != 0 && lng != 0)
                {
                    distance = CalculateDistanceKm(location.Latitude, location.Longitude, lat, lng);
                }

                nearbyTours.Add(new NearbyTourItem
                {
                    Id = t.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    Name = t.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                    Destination = t.TryGetProperty("destination", out var dest) ? dest.GetString() ?? "" : "",
                    Price = t.TryGetProperty("price", out var price) ? price.GetDecimal() : 0,
                    Latitude = lat,
                    Longitude = lng,
                    DistanceKm = distance
                });
            }

            // Sort by distance
            foreach (var tour in nearbyTours.OrderBy(t => t.DistanceKm))
            {
                NearbyTours.Add(tour);
            }
        }
        catch (FeatureNotSupportedException)
        {
            LocationStatus = "Bu cihazda konum desteklenmiyor";
        }
        catch (FeatureNotEnabledException)
        {
            LocationStatus = "Konum servisleri kapali. Lutfen acin.";
        }
        catch (PermissionException)
        {
            LocationStatus = "Konum izni verilmedi";
        }
        catch (Exception ex)
        {
            LocationStatus = $"Hata: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
