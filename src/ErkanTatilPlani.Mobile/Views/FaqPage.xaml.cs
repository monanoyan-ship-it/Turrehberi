using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class FaqPage : ContentPage
{
    public FaqPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is FaqViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
