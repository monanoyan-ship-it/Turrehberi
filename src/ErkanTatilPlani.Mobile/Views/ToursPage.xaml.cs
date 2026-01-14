using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class ToursPage : ContentPage
{
    public ToursPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ToursViewModel viewModel)
        {
            viewModel.LoadToursCommand.Execute(null);
        }
    }
}
