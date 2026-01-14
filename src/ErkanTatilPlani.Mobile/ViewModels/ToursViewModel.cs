using System.Collections.ObjectModel;
using System.Windows.Input;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class ToursViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<Tour> Tours { get; } = new();

    public ICommand LoadToursCommand { get; }
    public ICommand RefreshCommand { get; }

    public ToursViewModel()
    {
        Title = "Turlar";
        _apiService = new ApiService();
        LoadToursCommand = new Command(async () => await LoadToursAsync());
        RefreshCommand = new Command(async () => await LoadToursAsync());
    }

    private async Task LoadToursAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Tours.Clear();

            var tours = await _apiService.GetToursAsync();
            foreach (var tour in tours)
            {
                Tours.Add(tour);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Turlar yüklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
