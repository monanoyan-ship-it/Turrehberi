using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class CompaniesPage : ContentPage
{
    public CompaniesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CompaniesViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
