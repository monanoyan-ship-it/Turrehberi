using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class MyCompanyPage : ContentPage
{
    public MyCompanyPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MyCompanyViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
