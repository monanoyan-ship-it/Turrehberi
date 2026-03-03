using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class NearbyPage : ContentPage
{
    public NearbyPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NearbyViewModel viewModel)
        {
            viewModel.LoadNearbyCommand.Execute(null);
        }
    }
}
