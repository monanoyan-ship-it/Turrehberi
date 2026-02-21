using ErkanTatilPlani.API.Factories.Auth;
using ErkanTatilPlani.API.Factories.Blogs;
using ErkanTatilPlani.API.Factories.Companies;
using ErkanTatilPlani.API.Factories.EmailAccounts;
using ErkanTatilPlani.API.Factories.EmailTemplates;
using ErkanTatilPlani.API.Factories.Favorites;
using ErkanTatilPlani.API.Factories.Guides;
using ErkanTatilPlani.API.Factories.Languages;
using ErkanTatilPlani.API.Factories.Notifications;
using ErkanTatilPlani.API.Factories.Payments;
using ErkanTatilPlani.API.Factories.Reservations;
using ErkanTatilPlani.API.Factories.Reviews;
using ErkanTatilPlani.API.Factories.TourDates;
using ErkanTatilPlani.API.Factories.Tours;
using ErkanTatilPlani.API.Factories.TourWatches;
using ErkanTatilPlani.API.Factories.Promotions;
using ErkanTatilPlani.API.Factories.Visitors;
using ErkanTatilPlani.Core.Factories.Auth;
using ErkanTatilPlani.Core.Factories.Blogs;
using ErkanTatilPlani.Core.Factories.Companies;
using ErkanTatilPlani.Core.Factories.EmailAccounts;
using ErkanTatilPlani.Core.Factories.EmailTemplates;
using ErkanTatilPlani.Core.Factories.Favorites;
using ErkanTatilPlani.Core.Factories.Guides;
using ErkanTatilPlani.Core.Factories.Languages;
using ErkanTatilPlani.Core.Factories.Notifications;
using ErkanTatilPlani.Core.Factories.Payments;
using ErkanTatilPlani.Core.Factories.Promotions;
using ErkanTatilPlani.Core.Factories.Reservations;
using ErkanTatilPlani.Core.Factories.Reviews;
using ErkanTatilPlani.Core.Factories.TourDates;
using ErkanTatilPlani.Core.Factories.Tours;
using ErkanTatilPlani.Core.Factories.TourWatches;
using ErkanTatilPlani.Core.Factories.Visitors;

namespace ErkanTatilPlani.API.DependencyInjection;

public static class FactoryRegistration
{
    public static IServiceCollection AddFactories(this IServiceCollection services)
    {
        // Visitor factories (1)
        services.AddScoped<IVisitorFactory, VisitorFactory>();

        // Favorite factories (1)
        services.AddScoped<IFavoriteFactory, FavoriteFactory>();

        // Tour factories (1)
        services.AddScoped<ITourFactory, TourFactory>();

        // TourDate factories (2)
        services.AddScoped<ITourDateFactory, TourDateFactory>();
        services.AddScoped<ITourCalendarFactory, TourCalendarFactory>();

        // Payment factories (1)
        services.AddScoped<IPaymentFactory, PaymentFactory>();

        // EmailAccount factories (1)
        services.AddScoped<IEmailAccountFactory, EmailAccountFactory>();

        // EmailTemplate factories (1)
        services.AddScoped<IEmailTemplateFactory, EmailTemplateFactory>();

        // Language factories (1)
        services.AddScoped<ILanguageFactory, LanguageFactory>();

        // Reservation factories (3)
        services.AddScoped<IReservationCrudFactory, ReservationCrudFactory>();
        services.AddScoped<IVisitorReservationFactory, VisitorReservationFactory>();
        services.AddScoped<IReservationPaymentFactory, ReservationPaymentFactory>();

        // Auth factories (3)
        services.AddScoped<IAuthFactory, AuthFactory>();
        services.AddScoped<IProfileFactory, ProfileFactory>();
        services.AddScoped<IAccountSecurityFactory, AccountSecurityFactory>();

        // Review factories (3)
        services.AddScoped<ITourReviewFactory, TourReviewFactory>();
        services.AddScoped<IReviewInteractionFactory, ReviewInteractionFactory>();
        services.AddScoped<IAdminReviewFactory, AdminReviewFactory>();

        // Blog factories (2)
        services.AddScoped<IBlogFactory, BlogFactory>();
        services.AddScoped<IBlogCommentFactory, BlogCommentFactory>();

        // Promotion factories (2)
        services.AddScoped<IPromotionCrudFactory, PromotionCrudFactory>();
        services.AddScoped<IPromotionCalculationFactory, PromotionCalculationFactory>();

        // TourWatch factories (1)
        services.AddScoped<ITourWatchFactory, TourWatchFactory>();

        // Notification factories (1)
        services.AddScoped<INotificationFactory, NotificationFactory>();

        // Guide factories (1)
        services.AddScoped<IGuideFactory, GuideFactory>();

        // Company factories (7)
        services.AddScoped<ICompanyCrudFactory, CompanyCrudFactory>();
        services.AddScoped<ICompanyProfileFactory, CompanyProfileFactory>();
        services.AddScoped<ICompanyApprovalFactory, CompanyApprovalFactory>();
        services.AddScoped<ICompanyDashboardFactory, CompanyDashboardFactory>();
        services.AddScoped<ICompanyGalleryFactory, CompanyGalleryFactory>();
        services.AddScoped<ICompanyPageFactory, CompanyPageFactory>();
        services.AddScoped<ICompanyAnalyticsFactory, CompanyAnalyticsFactory>();

        return services;
    }
}
