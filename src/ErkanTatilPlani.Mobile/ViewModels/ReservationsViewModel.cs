using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class ReservationListItem
{
    public int Id { get; set; }
    public string TourName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;

    public string PriceDisplay => $"{TotalPrice:N0} TL";
    public string PeopleDisplay => $"{NumberOfPeople} Kisi";
}

public class ReservationsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<ReservationListItem> Reservations { get; } = new();

    public ICommand LoadReservationsCommand { get; }
    public ICommand RefreshCommand { get; }

    public ReservationsViewModel()
    {
        Title = "Rezervasyonlar";
        _apiService = new ApiService();
        LoadReservationsCommand = new Command(async () => await LoadReservationsAsync());
        RefreshCommand = new Command(async () => await LoadReservationsAsync());
    }

    private async Task LoadReservationsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Reservations.Clear();

            if (!_apiService.IsLoggedIn)
            {
                await Shell.Current.DisplayAlert("Uyari", "Rezervasyonlarinizi gormek icin giris yapin", "Tamam");
                return;
            }

            var reservations = await _apiService.GetMyReservationsAsync();
            foreach (var r in reservations)
            {
                Reservations.Add(new ReservationListItem
                {
                    Id = r.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    TourName = r.TryGetProperty("tourName", out var tn) ? tn.GetString() ?? "" : "",
                    Date = r.TryGetProperty("date", out var d) ? d.GetString()?.Substring(0, 10) ?? "" : "",
                    NumberOfPeople = r.TryGetProperty("numberOfPeople", out var np) ? np.GetInt32() : 0,
                    TotalPrice = r.TryGetProperty("totalPrice", out var tp) ? tp.GetDecimal() : 0,
                    Status = r.TryGetProperty("statusName", out var s) ? s.GetString() ?? "" : ""
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Rezervasyonlar yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
