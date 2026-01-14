using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class ReservationsPage : ContentPage
{
    public ReservationsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ReservationsViewModel viewModel)
        {
            viewModel.LoadReservationsCommand.Execute(null);
        }
    }
}
