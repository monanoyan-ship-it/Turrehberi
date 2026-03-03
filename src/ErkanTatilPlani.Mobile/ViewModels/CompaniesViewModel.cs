using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class CompanyListItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TourCount { get; set; }

    public string RatingDisplay => AverageRating > 0 ? $"{AverageRating:N1}" : "-";
    public string TourCountDisplay => $"{TourCount} Tur";
}

public class CompaniesViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<CompanyListItem> Companies { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CompanyTappedCommand { get; }

    public CompaniesViewModel()
    {
        Title = "Firmalar";
        _apiService = new ApiService();
        LoadCommand = new Command(async () => await LoadCompaniesAsync());
        RefreshCommand = new Command(async () => await LoadCompaniesAsync());
        CompanyTappedCommand = new Command<CompanyListItem>(async (company) => await GoToDetailAsync(company));
    }

    private async Task LoadCompaniesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Companies.Clear();

            var companies = await _apiService.GetCompaniesAsync();

            foreach (var c in companies)
            {
                Companies.Add(new CompanyListItem
                {
                    Id = c.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    Name = c.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
                    Description = c.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    AverageRating = c.TryGetProperty("averageRating", out var rating) ? rating.GetDecimal() : 0,
                    TourCount = c.TryGetProperty("tourCount", out var tc) ? tc.GetInt32() : 0
                });
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Firmalar yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GoToDetailAsync(CompanyListItem company)
    {
        if (company == null) return;
        await Shell.Current.GoToAsync($"CompanyDetailPage?companyId={company.Id}");
    }
}
