using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class DealsPage : ContentPage
{
    public DealsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DealsViewModel viewModel)
        {
            viewModel.LoadAllCommand.Execute(null);
        }
    }
}
