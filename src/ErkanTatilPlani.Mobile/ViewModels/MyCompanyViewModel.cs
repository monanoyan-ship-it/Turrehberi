using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class MyTourItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int ReservationCount { get; set; }

    public string PriceDisplay => $"{Price:N0} TL";
}

public class MyReservationItem
{
    public int Id { get; set; }
    public string TourName { get; set; } = string.Empty;
    public string VisitorName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public string PriceDisplay => $"{TotalPrice:N0} TL";
    public string PeopleDisplay => $"{NumberOfPeople} Kisi";
}

public class MyCompanyViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private string _companyName = string.Empty;
    private string _totalRevenue = string.Empty;
    private string _totalReservations = string.Empty;
    private string _averageRating = string.Empty;

    public string CompanyName { get => _companyName; set => SetProperty(ref _companyName, value); }
    public string TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }
    public string TotalReservations { get => _totalReservations; set => SetProperty(ref _totalReservations, value); }
    public string AverageRating { get => _averageRating; set => SetProperty(ref _averageRating, value); }

    public ObservableCollection<MyTourItem> MyTours { get; } = new();
    public ObservableCollection<MyReservationItem> RecentReservations { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }

    public MyCompanyViewModel()
    {
        Title = "Firmam";
        _apiService = new ApiService();
        LoadCommand = new Command(async () => await LoadDashboardAsync());
        RefreshCommand = new Command(async () => await LoadDashboardAsync());
    }

    private async Task LoadDashboardAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            MyTours.Clear();
            RecentReservations.Clear();

            if (!_apiService.IsLoggedIn)
            {
                await Shell.Current.DisplayAlert("Uyari", "Giris yapin", "Tamam");
                return;
            }

            var dashboard = await _apiService.GetMyCompanyDashboardAsync();
            if (dashboard.HasValue)
            {
                var d = dashboard.Value;
                CompanyName = d.TryGetProperty("companyName", out var cn) ? cn.GetString() ?? "" : "";
                TotalRevenue = d.TryGetProperty("totalRevenue", out var tr) ? $"{tr.GetDecimal():N0} TL" : "0 TL";
                TotalReservations = d.TryGetProperty("totalReservations", out var trs) ? trs.GetInt32().ToString() : "0";
                AverageRating = d.TryGetProperty("averageRating", out var ar) ? $"{ar.GetDecimal():N1}" : "0";

                if (d.TryGetProperty("recentTours", out var tours) && tours.ValueKind == JsonValueKind.Array)
                {
                    foreach (var t in tours.EnumerateArray())
                    {
                        MyTours.Add(new MyTourItem
                        {
                            Id = t.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                            Name = t.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                            Price = t.TryGetProperty("price", out var p) ? p.GetDecimal() : 0,
                            StatusName = t.TryGetProperty("statusName", out var s) ? s.GetString() ?? "" : "",
                        });
                    }
                }

                if (d.TryGetProperty("recentReservations", out var reservations) && reservations.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in reservations.EnumerateArray())
                    {
                        RecentReservations.Add(new MyReservationItem
                        {
                            Id = r.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                            TourName = r.TryGetProperty("tourName", out var tn) ? tn.GetString() ?? "" : "",
                            VisitorName = r.TryGetProperty("visitorName", out var vn) ? vn.GetString() ?? "" : "",
                            Date = r.TryGetProperty("date", out var dt) ? dt.GetString()?.Substring(0, 10) ?? "" : "",
                            NumberOfPeople = r.TryGetProperty("numberOfPeople", out var np) ? np.GetInt32() : 0,
                            TotalPrice = r.TryGetProperty("totalPrice", out var tp) ? tp.GetDecimal() : 0,
                            StatusName = r.TryGetProperty("statusName", out var s) ? s.GetString() ?? "" : ""
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Dashboard yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
