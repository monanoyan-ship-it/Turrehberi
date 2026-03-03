using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class DealItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public string EndDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public string DiscountDisplay => $"%{DiscountValue:N0} Indirim";
}

public class DealsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<DealItem> FlashSales { get; } = new();
    public ObservableCollection<DealItem> LastMinuteDeals { get; } = new();

    public ICommand LoadFlashSalesCommand { get; }
    public ICommand LoadLastMinuteCommand { get; }
    public ICommand LoadAllCommand { get; }
    public ICommand RefreshCommand { get; }

    public DealsViewModel()
    {
        Title = "Firsatlar";
        _apiService = new ApiService();
        LoadFlashSalesCommand = new Command(async () => await LoadFlashSalesAsync());
        LoadLastMinuteCommand = new Command(async () => await LoadLastMinuteAsync());
        LoadAllCommand = new Command(async () => await LoadAllAsync());
        RefreshCommand = new Command(async () => await LoadAllAsync());
    }

    private async Task LoadAllAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await LoadFlashSalesAsync();
            await LoadLastMinuteAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadFlashSalesAsync()
    {
        try
        {
            FlashSales.Clear();

            var result = await _apiService.GetFlashSalesAsync();
            if (result.HasValue)
            {
                var items = result.Value;
                if (items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        FlashSales.Add(ParseDeal(item, "Flash Sale"));
                    }
                }
            }
        }
        catch (Exception)
        {
            // Silently handle - main LoadAll will handle IsBusy
        }
    }

    private async Task LoadLastMinuteAsync()
    {
        try
        {
            LastMinuteDeals.Clear();

            var result = await _apiService.GetLastMinuteDealsAsync();
            if (result.HasValue)
            {
                var items = result.Value;
                if (items.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        LastMinuteDeals.Add(ParseDeal(item, "Son Dakika"));
                    }
                }
            }
        }
        catch (Exception)
        {
            // Silently handle - main LoadAll will handle IsBusy
        }
    }

    private static DealItem ParseDeal(JsonElement el, string type)
    {
        return new DealItem
        {
            Id = el.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
            Name = el.TryGetProperty("name", out var name) ? name.GetString() ?? "" : "",
            Description = el.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
            DiscountValue = el.TryGetProperty("discountValue", out var dv) ? dv.GetDecimal() : 0,
            EndDate = el.TryGetProperty("endDate", out var ed) ? ed.GetString() ?? "" : "",
            Type = type
        };
    }
}
