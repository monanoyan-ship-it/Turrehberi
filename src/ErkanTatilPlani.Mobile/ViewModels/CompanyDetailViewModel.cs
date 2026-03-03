using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

[QueryProperty(nameof(CompanyId), "companyId")]
public class CompanyDetailViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private int _companyId;
    private string _companyName = string.Empty;
    private string _description = string.Empty;
    private decimal _averageRating;
    private string _phoneNumber = string.Empty;
    private string _email = string.Empty;
    private string _address = string.Empty;

    public int CompanyId
    {
        get => _companyId;
        set
        {
            SetProperty(ref _companyId, value);
            LoadCommand.Execute(null);
        }
    }

    public string CompanyName { get => _companyName; set => SetProperty(ref _companyName, value); }
    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public decimal AverageRating { get => _averageRating; set => SetProperty(ref _averageRating, value); }
    public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Address { get => _address; set => SetProperty(ref _address, value); }

    public string RatingDisplay => AverageRating > 0 ? $"{AverageRating:N1} / 5" : "-";

    public ICommand LoadCommand { get; }

    public CompanyDetailViewModel()
    {
        _apiService = new ApiService();
        LoadCommand = new Command(async () => await LoadCompanyAsync());
    }

    private async Task LoadCompanyAsync()
    {
        if (IsBusy || CompanyId == 0) return;

        try
        {
            IsBusy = true;
            var company = await _apiService.GetCompanyAsync(CompanyId);
            if (company.HasValue)
            {
                var c = company.Value;
                CompanyName = GetString(c, "name");
                Title = CompanyName;
                Description = GetString(c, "description");
                AverageRating = GetDecimal(c, "averageRating");
                OnPropertyChanged(nameof(RatingDisplay));
                PhoneNumber = GetString(c, "phoneNumber");
                Email = GetString(c, "email");
                Address = GetString(c, "address");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Firma detaylari yuklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

    private static decimal GetDecimal(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDecimal() : 0;
}
