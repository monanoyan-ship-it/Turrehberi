using ErkanTatilPlani.Mobile.Views;

namespace ErkanTatilPlani.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes
        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
        Routing.RegisterRoute("TourDetailPage", typeof(TourDetailPage));
        Routing.RegisterRoute("CompanyDetailPage", typeof(CompanyDetailPage));
        Routing.RegisterRoute("BlogDetailPage", typeof(BlogDetailPage));
        Routing.RegisterRoute("CompaniesPage", typeof(CompaniesPage));
        Routing.RegisterRoute("BlogPage", typeof(BlogPage));
        Routing.RegisterRoute("FaqPage", typeof(FaqPage));
        Routing.RegisterRoute("FavoritesPage", typeof(FavoritesPage));
        Routing.RegisterRoute("NotificationsPage", typeof(NotificationsPage));
        Routing.RegisterRoute("DealsPage", typeof(DealsPage));
        Routing.RegisterRoute("MessagesPage", typeof(MessagesPage));
        Routing.RegisterRoute("SupportPage", typeof(SupportPage));
        Routing.RegisterRoute("MyCompanyPage", typeof(MyCompanyPage));
    }
}
