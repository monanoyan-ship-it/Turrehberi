using System.Collections.ObjectModel;
using System.Windows.Input;
using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Mobile.Services;

namespace ErkanTatilPlani.Mobile.ViewModels;

public class ReservationsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ObservableCollection<Reservation> Reservations { get; } = new();

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

            var reservations = await _apiService.GetReservationsAsync();
            foreach (var reservation in reservations)
            {
                Reservations.Add(reservation);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Hata", $"Rezervasyonlar yüklenemedi: {ex.Message}", "Tamam");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
