using ErkanTatilPlani.Mobile.ViewModels;

namespace ErkanTatilPlani.Mobile.Views;

public partial class NotificationsPage : ContentPage
{
    public NotificationsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NotificationsViewModel viewModel)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
