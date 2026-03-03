using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

[QueryProperty(nameof(TourId), "tourId")]
public class TourDetailViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private int _tourId;
    private string _tourName = string.Empty;
    private string _description = string.Empty;
    private string _destination = string.Empty;
    private string _companyName = string.Empty;
    private string _duration = string.Empty;
    private string _price = string.Empty;
    private string _rating = string.Empty;
    private string _includes = string.Empty;
    private string _excludes = string.Empty;
    private int _numberOfPeople = 1;
    private decimal _unitPrice;
    private string _estimatedPrice = string.Empty;
    private string _bookingMessage = string.Empty;
    private Color _bookingMessageColor = Colors.Green;

    public int TourId
    {
        get => _tourId;
        set
        {
            SetProperty(ref _tourId, value);
            LoadTourCommand.Execute(null);
        }
    }

    public string TourName { get => _tourName; set => SetProperty(ref _tourName, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public string Destination { get => _destination; set => SetProperty(ref _destination, value); }
    public string CompanyName { get => _companyName; set => SetProperty(ref _companyName, value); }
    public string Duration { get => _duration; set => SetProperty(ref _duration, value); }
    public string Price { get => _price; set => SetProperty(ref _price, value); }
    public string Rating { get => _rating; set => SetProperty(ref _rating, value); }
    public string Includes { get => _includes; set => SetProperty(ref _includes, value); }
    public string Excludes { get => _excludes; set => SetProperty(ref _excludes, value); }

    public int NumberOfPeople
    {
        get => _numberOfPeople;
        set
        {
            if (value < 1) value = 1;
            if (value > 20) value = 20;
            SetProperty(ref _numberOfPeople, value);
            UpdateEstimatedPrice();
        }
    }

    public string EstimatedPrice
    {
        get => _estimatedPrice;
        set => SetProperty(ref _estimatedPrice, value);
    }

    public string BookingMessage
    {
        get => _bookingMessage;
        set => SetProperty(ref _bookingMessage, value);
    }

    public Color BookingMessageColor
    {
        get => _bookingMessageColor;
        set => SetProperty(ref _bookingMessageColor, value);
    }

    public ICommand LoadTourCommand { get; }
    public ICommand IncreasePersonCommand { get; }
    public ICommand DecreasePersonCommand { get; }
    public ICommand BookCommand { get; }

    public TourDetailViewModel()
    {
        _apiService = new ApiService();
        LoadTourCommand = new Command(async () => await LoadTourAsync());
        IncreasePersonCommand = new Command(() => NumberOfPeople++);
        DecreasePersonCommand = new Command(() => NumberOfPeople--);
        BookCommand = new Command(async () => await BookAsync());
    }

    private void UpdateEstimatedPrice()
    {
        var total = _unitPrice * NumberOfPeople;
        EstimatedPrice = $"Tahmini Toplam: {total:N0} TL";
    }

    private async Task LoadTourAsync()
    {
        if (IsBusy || TourId == 0) return;

        try
        {
            IsBusy = true;
            var tour = await _apiService.GetTourAsync(TourId);
            if (tour.HasValue)
            {
                var t = tour.Value;
                TourName = GetString(t, "name");
                Title = TourName;
                Description = GetString(t, "description");
                Destination = GetString(t, "destination");
                CompanyName = GetString(t, "companyName");
                Duration = $"{GetInt(t, "durationDays")} Gun";
                _unitPrice = GetDecimal(t, "price");
                Price = $"{_unitPrice:N0} TL";
                Rating = $"{GetDecimal(t, "averageRating"):N1} / 5";
                Includes = GetString(t, "includes");
                Excludes = GetString(t, "excludes");
                UpdateEstimatedPrice();

                // Offline cache
                Preferences.Set($"tour_cache_{TourId}", t.GetRawText());
            }
        }
        catch (Exception)
        {
            // Offline fallback
            var cached = Preferences.Get($"tour_cache_{TourId}", null as string);
            if (cached != null)
            {
                var t = JsonSerializer.Deserialize<JsonElement>(cached);
                TourName = GetString(t, "name");
                Title = TourName;
                Description = GetString(t, "description");
                Destination = GetString(t, "destination");
                _unitPrice = GetDecimal(t, "price");
                Price = $"{_unitPrice:N0} TL";
                UpdateEstimatedPrice();
            }
            else
            {
                await Shell.Current.DisplayAlert("Hata", "Tur detaylari yuklenemedi", "Tamam");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BookAsync()
    {
        if (IsBusy) return;

        if (!_apiService.IsLoggedIn)
        {
            var login = await Shell.Current.DisplayAlert("Giris Gerekli",
                "Rezervasyon yapabilmek icin giris yapmaniz gerekiyor.", "Giris Yap", "Iptal");
            if (login)
            {
                _apiService.Logout();
                Application.Current!.MainPage = new NavigationPage(new Views.LoginPage());
            }
            return;
        }

        try
        {
            IsBusy = true;
            BookingMessage = string.Empty;

            var (success, error) = await _apiService.CreateReservationAsync(new
            {
                tourId = TourId,
                numberOfPeople = NumberOfPeople,
                startDate = DateTime.UtcNow.AddDays(7).ToString("o")
            });

            if (success)
            {
                BookingMessageColor = Colors.Green;
                BookingMessage = "Rezervasyonunuz basariyla olusturuldu!";
            }
            else
            {
                BookingMessageColor = Colors.Red;
                BookingMessage = error ?? "Rezervasyon olusturulamadi";
            }
        }
        catch (Exception ex)
        {
            BookingMessageColor = Colors.Red;
            BookingMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

    private static int GetInt(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

    private static decimal GetDecimal(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDecimal() : 0;
}
