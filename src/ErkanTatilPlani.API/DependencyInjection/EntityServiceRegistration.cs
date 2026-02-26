using ErkanTatilPlani.API.EntityServices;
using ErkanTatilPlani.Core.EntityServices;

namespace ErkanTatilPlani.API.DependencyInjection;

public static class EntityServiceRegistration
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services.AddScoped<IVisitorEntityService, VisitorEntityService>();
        services.AddScoped<ITourEntityService, TourEntityService>();
        services.AddScoped<ICompanyEntityService, CompanyEntityService>();
        services.AddScoped<IReservationEntityService, ReservationEntityService>();
        services.AddScoped<IReviewEntityService, ReviewEntityService>();
        services.AddScoped<IFavoriteEntityService, FavoriteEntityService>();
        services.AddScoped<IEmailAccountEntityService, EmailAccountEntityService>();
        services.AddScoped<IEmailTemplateEntityService, EmailTemplateEntityService>();
        services.AddScoped<ILanguageEntityService, LanguageEntityService>();
        services.AddScoped<ICompanyGalleryEntityService, CompanyGalleryEntityService>();
        services.AddScoped<IBlogEntityService, BlogEntityService>();
        services.AddScoped<ICompanyPageEntityService, CompanyPageEntityService>();
        services.AddScoped<ITourScheduleEntityService, TourScheduleEntityService>();
        services.AddScoped<IPromotionEntityService, PromotionEntityService>();
        services.AddScoped<ITourWatchEntityService, TourWatchEntityService>();
        services.AddScoped<INotificationEntityService, NotificationEntityService>();
        services.AddScoped<IGuideEntityService, GuideEntityService>();
        services.AddScoped<ITourPhotoEntityService, TourPhotoEntityService>();
        services.AddScoped<IMessageEntityService, MessageEntityService>();
        services.AddScoped<IMessageTemplateEntityService, MessageTemplateEntityService>();
        services.AddScoped<IPushSubscriptionEntityService, PushSubscriptionEntityService>();
        services.AddScoped<ILoyaltyEntityService, LoyaltyEntityService>();
        services.AddScoped<IReferralEntityService, ReferralEntityService>();
        services.AddScoped<IScheduledEmailEntityService, ScheduledEmailEntityService>();
        services.AddScoped<IAbandonedCartEntityService, AbandonedCartEntityService>();
        return services;
    }
}
