using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class BlogPage : ContentPage
{
    public BlogPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BlogViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
