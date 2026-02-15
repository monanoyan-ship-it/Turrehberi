using ErkanTatilPlani.API.Factories.Auth;
using ErkanTatilPlani.API.Factories.Blogs;
using ErkanTatilPlani.API.Factories.Companies;
using ErkanTatilPlani.API.Factories.EmailAccounts;
using ErkanTatilPlani.API.Factories.EmailTemplates;
using ErkanTatilPlani.API.Factories.Favorites;
using ErkanTatilPlani.API.Factories.Languages;
using ErkanTatilPlani.API.Factories.Payments;
using ErkanTatilPlani.API.Factories.Reservations;
using ErkanTatilPlani.API.Factories.Reviews;
using ErkanTatilPlani.API.Factories.Tours;
using ErkanTatilPlani.API.Factories.Visitors;
using ErkanTatilPlani.Core.Factories.Auth;
using ErkanTatilPlani.Core.Factories.Blogs;
using ErkanTatilPlani.Core.Factories.Companies;
using ErkanTatilPlani.Core.Factories.EmailAccounts;
using ErkanTatilPlani.Core.Factories.EmailTemplates;
using ErkanTatilPlani.Core.Factories.Favorites;
using ErkanTatilPlani.Core.Factories.Languages;
using ErkanTatilPlani.Core.Factories.Payments;
using ErkanTatilPlani.Core.Factories.Reservations;
using ErkanTatilPlani.Core.Factories.Reviews;
using ErkanTatilPlani.Core.Factories.Tours;
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

        // Payment factories (1)
        services.AddScoped<IPaymentFactory, PaymentFactory>();

        // EmailAccount factories (1)
        services.AddScoped<IEmailAccountFactory, EmailAccountFactory>();

        // EmailTemplate factories (1)
        services.AddScoped<IEmailTemplateFactory, EmailTemplateFactory>();

        // Language factories (2)
        services.AddScoped<ILanguageFactory, LanguageFactory>();
        services.AddScoped<ILanguageResourceFactory, LanguageResourceFactory>();

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

        // Company factories (5)
        services.AddScoped<ICompanyCrudFactory, CompanyCrudFactory>();
        services.AddScoped<ICompanyProfileFactory, CompanyProfileFactory>();
        services.AddScoped<ICompanyApprovalFactory, CompanyApprovalFactory>();
        services.AddScoped<ICompanyDashboardFactory, CompanyDashboardFactory>();
        services.AddScoped<ICompanyGalleryFactory, CompanyGalleryFactory>();

        return services;
    }
}
