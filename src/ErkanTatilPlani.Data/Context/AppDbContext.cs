using ErkanTatilPlani.Core.Entities;
using ErkanTatilPlani.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ErkanTatilPlani.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<Visitor> Visitors => Set<Visitor>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<LocaleStringResource> LocaleStringResources => Set<LocaleStringResource>();

    // Review System
    public DbSet<TourReview> TourReviews => Set<TourReview>();
    public DbSet<ReviewImage> ReviewImages => Set<ReviewImage>();
    public DbSet<ReviewHelpful> ReviewHelpfuls => Set<ReviewHelpful>();
    public DbSet<ReviewReply> ReviewReplies => Set<ReviewReply>();
    public DbSet<ReviewReport> ReviewReports => Set<ReviewReport>();

    // Favorites
    public DbSet<FavoriteTour> FavoriteTours => Set<FavoriteTour>();

    // Logging
    public DbSet<AppLog> AppLogs => Set<AppLog>();

    // Gallery
    public DbSet<CompanyGalleryImage> CompanyGalleryImages => Set<CompanyGalleryImage>();

    // Blog
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<BlogComment> BlogComments => Set<BlogComment>();

    // Company Pages
    public DbSet<CompanyPage> CompanyPages => Set<CompanyPage>();

    // Tour Dates (Musaitlik Takvimi)
    public DbSet<TourDate> TourDates => Set<TourDate>();

    // Promotions
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionUsage> PromotionUsages => Set<PromotionUsage>();

    // Email Management
    public DbSet<EmailAccount> EmailAccounts => Set<EmailAccount>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailTemplateTranslation> EmailTemplateTranslations => Set<EmailTemplateTranslation>();

    // Tour Watch & Notifications
    public DbSet<TourWatch> TourWatches => Set<TourWatch>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.MetaTitle).HasMaxLength(100);
            entity.Property(e => e.MetaDescription).HasMaxLength(300);
            entity.Property(e => e.Tagline).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TaxNumber).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();

            // Onay yapan yetkili iliskisi
            entity.HasOne(e => e.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedById)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.GuideLanguages).HasMaxLength(200);
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Tours)
                  .HasForeignKey(e => e.CompanyId);
        });

        modelBuilder.Entity<TourDate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(18, 2);

            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.TourDates)
                  .HasForeignKey(e => e.TourId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .IsRequired(false);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.Reservations)
                  .HasForeignKey(e => e.TourId);
            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.Reservations)
                  .HasForeignKey(e => e.VisitorId);
            entity.HasOne(e => e.Promotion)
                  .WithMany()
                  .HasForeignKey(e => e.PromotionId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LanguageCulture).IsRequired().HasMaxLength(20);
            entity.Property(e => e.UniqueSeoCode).IsRequired().HasMaxLength(5);
            entity.HasIndex(e => e.LanguageCulture).IsUnique();
            entity.HasIndex(e => e.UniqueSeoCode).IsUnique();
        });

        modelBuilder.Entity<LocaleStringResource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ResourceName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ResourceValue).IsRequired();
            entity.HasIndex(e => new { e.LanguageId, e.ResourceName }).IsUnique();
            entity.HasOne(e => e.Language)
                  .WithMany(l => l.LocaleStringResources)
                  .HasForeignKey(e => e.LanguageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================================
        // YORUM SISTEMI
        // ===============================================

        modelBuilder.Entity<TourReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Pros).HasMaxLength(2000);
            entity.Property(e => e.Cons).HasMaxLength(2000);
            entity.Property(e => e.Comment).HasMaxLength(5000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            // Bir kullanici bir tura bir kez yorum yapabilir
            entity.HasIndex(e => new { e.TourId, e.VisitorId }).IsUnique();

            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.Reviews)
                  .HasForeignKey(e => e.TourId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.Reviews)
                  .HasForeignKey(e => e.VisitorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Reservation)
                  .WithMany()
                  .HasForeignKey(e => e.ReservationId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ModeratedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ModeratedById)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReviewImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.MimeType).HasMaxLength(50);
            entity.Property(e => e.AltText).HasMaxLength(200);

            entity.HasOne(e => e.Review)
                  .WithMany(r => r.Images)
                  .HasForeignKey(e => e.ReviewId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewHelpful>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IpAddress).HasMaxLength(45);

            // Bir kullanici bir yoruma bir kez oy verebilir
            entity.HasIndex(e => new { e.ReviewId, e.VisitorId }).IsUnique();

            entity.HasOne(e => e.Review)
                  .WithMany(r => r.HelpfulVotes)
                  .HasForeignKey(e => e.ReviewId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.HelpfulVotes)
                  .HasForeignKey(e => e.VisitorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewReply>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);

            entity.HasOne(e => e.Review)
                  .WithMany(r => r.Replies)
                  .HasForeignKey(e => e.ReviewId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.ReviewReplies)
                  .HasForeignKey(e => e.VisitorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentReply)
                  .WithMany(r => r.ChildReplies)
                  .HasForeignKey(e => e.ParentReplyId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ModeratedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ModeratedById)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReviewReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ReviewNote).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(45);

            entity.HasOne(e => e.Review)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Reply)
                  .WithMany()
                  .HasForeignKey(e => e.ReplyId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Visitor)
                  .WithMany()
                  .HasForeignKey(e => e.VisitorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedById)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ===============================================
        // FIRMA GALERI
        // ===============================================

        modelBuilder.Entity<CompanyGalleryImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.MimeType).HasMaxLength(50);
            entity.Property(e => e.AltText).HasMaxLength(200);

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.GalleryImages)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================================
        // FAVORI TURLAR
        // ===============================================

        modelBuilder.Entity<FavoriteTour>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Bir kullanici bir turu bir kez favorilere ekleyebilir
            entity.HasIndex(e => new { e.VisitorId, e.TourId }).IsUnique();

            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.FavoriteTours)
                  .HasForeignKey(e => e.VisitorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.FavoritedBy)
                  .HasForeignKey(e => e.TourId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================================
        // UYGULAMA LOGLARI
        // ===============================================

        modelBuilder.Entity<AppLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Level).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(200);
            entity.Property(e => e.Action).HasMaxLength(200);
            entity.Property(e => e.TraceId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RequestPath).HasMaxLength(500);
            entity.Property(e => e.RequestMethod).HasMaxLength(10);

            // Indexler - sorgulama performansi icin
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.Source);
            entity.HasIndex(e => new { e.Level, e.Timestamp });
        });

        // ===============================================
        // EMAIL YONETIMI
        // ===============================================

        modelBuilder.Entity<EmailAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SmtpHost).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SmtpUsername).HasMaxLength(200);
            entity.Property(e => e.SmtpPassword).HasMaxLength(500);
            entity.Property(e => e.FromEmail).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FromName).HasMaxLength(200);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.IsDefault);
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Placeholders).HasMaxLength(2000);

            entity.HasIndex(e => e.Key).IsUnique();

            entity.HasOne(e => e.EmailAccount)
                  .WithMany(a => a.EmailTemplates)
                  .HasForeignKey(e => e.EmailAccountId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EmailTemplateTranslation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Body).IsRequired();

            // Her sablon icin her dil sadece bir kez tanimlanabilir
            entity.HasIndex(e => new { e.EmailTemplateId, e.LanguageCode }).IsUnique();

            entity.HasOne(e => e.EmailTemplate)
                  .WithMany(t => t.Translations)
                  .HasForeignKey(e => e.EmailTemplateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================================
        // BLOG SISTEMI
        // ===============================================

        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Summary).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.MetaTitle).HasMaxLength(100);
            entity.Property(e => e.MetaDescription).HasMaxLength(300);
            entity.Property(e => e.Tags).HasMaxLength(500);

            entity.HasIndex(e => e.Slug).IsUnique();

            entity.HasOne(e => e.Author)
                  .WithMany(v => v.BlogPosts)
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);

            entity.HasOne(e => e.BlogPost)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(e => e.BlogPostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.BlogComments)
                  .HasForeignKey(e => e.VisitorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentComment)
                  .WithMany(c => c.Replies)
                  .HasForeignKey(e => e.ParentCommentId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===============================================
        // FIRMA SAYFALARI
        // ===============================================

        modelBuilder.Entity<CompanyPage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.MetaTitle).HasMaxLength(100);
            entity.Property(e => e.MetaDescription).HasMaxLength(300);

            entity.HasIndex(e => new { e.CompanyId, e.Slug }).IsUnique();

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Pages)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================================
        // PROMOSYON SISTEMI
        // ===============================================

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.WeekendMultiplier).HasPrecision(18, 4);
            entity.Property(e => e.HighDemandMultiplier).HasPrecision(18, 4);

            entity.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique()
                  .HasFilter("\"Code\" IS NOT NULL");

            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByVisitor)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedByVisitorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PromotionUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.AppliedRule).HasMaxLength(500);

            entity.HasOne(e => e.Promotion)
                  .WithMany(p => p.Usages)
                  .HasForeignKey(e => e.PromotionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Reservation)
                  .WithMany()
                  .HasForeignKey(e => e.ReservationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Visitor)
                  .WithMany()
                  .HasForeignKey(e => e.VisitorId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ===============================================
        // TUR TAKIP (TOUR WATCH)
        // ===============================================

        modelBuilder.Entity<TourWatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.VisitorId, e.TourId }).IsUnique();

            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.TourWatches)
                  .HasForeignKey(e => e.VisitorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.TourWatches)
                  .HasForeignKey(e => e.TourId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===============================================
        // BILDIRIMLER (NOTIFICATIONS)
        // ===============================================

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TitleKey).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MessageKey).IsRequired().HasMaxLength(200);
            entity.Property(e => e.MessageParams).HasMaxLength(1000);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);

            entity.HasIndex(e => new { e.VisitorId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.Notifications)
                  .HasForeignKey(e => e.VisitorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Reservation - yeni alanlar
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(e => e.QrCode).HasMaxLength(500);
            entity.Property(e => e.QrToken).HasMaxLength(100);
            entity.Property(e => e.PhotoLink).HasMaxLength(500);
        });

        // Tour - bulusma noktasi
        modelBuilder.Entity<Tour>(entity =>
        {
            entity.Property(e => e.MeetingPointAddress).HasMaxLength(500);
        });

        // Seed Data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Firmalar (StatusId = Approved, mevcut firmalar zaten onaylanmis)
        var companies = new[]
        {
            new Company { Id = 1, Name = "Ege Tur", Description = "Ege bolgesi turlari konusunda uzman seyahat acentasi. 15 yillik deneyimimizle Efes, Pamukkale, Cesme ve daha fazlasini kesfetmenizi sagliyoruz.", Email = "info@egetur.com", Phone = "0232 555 1234", Address = "Izmir, Alsancak Mah. Kordon Cad. No:123", Website = "www.egetur.com", TaxNumber = "1234567890", LogoUrl = "https://picsum.photos/seed/egetur/200", Slug = "ege-tur", City = "Izmir", Tagline = "Ege'nin Guzelliklerini Kesfedin", FoundedYear = 2010, MetaTitle = "Ege Tur - Izmir Cikisli Ege Turlari", MetaDescription = "Efes, Pamukkale, Cesme ve tum Ege turlarinda uzman acenta. Gunubirlik ve konaklamali turlar.", CoverImageUrl = "https://picsum.photos/seed/egetur-cover/1200/400", StatusId = CompanyStatuses.Ids.Approved, ApplicationDate = now, ReviewedAt = now, ReviewedById = 1, CreatedAt = now, IsActive = true },
            new Company { Id = 2, Name = "Karadeniz Gezileri", Description = "Karadeniz'in essiz dogasini ve yaylalarini profesyonel rehberler esliginde kesfetmenizi saglayan tur firmasi. Uzungol, Ayder ve daha fazlasi.", Email = "info@karadenizgezileri.com", Phone = "0462 555 5678", Address = "Trabzon, Meydan Mah. Ataturk Cad. No:45", Website = "www.karadenizgezileri.com", TaxNumber = "2345678901", LogoUrl = "https://picsum.photos/seed/karadeniz/200", Slug = "karadeniz-gezileri", City = "Trabzon", Tagline = "Yesil Cennetin Kapisi", FoundedYear = 2015, MetaTitle = "Karadeniz Gezileri - Yayla ve Doga Turlari", MetaDescription = "Uzungol, Ayder, Sumela ve tum Karadeniz yaylalarini kesfedin. Dogayla ic ice tatil firsatlari.", CoverImageUrl = "https://picsum.photos/seed/karadeniz-cover/1200/400", StatusId = CompanyStatuses.Ids.Approved, ApplicationDate = now, ReviewedAt = now, ReviewedById = 1, CreatedAt = now, IsActive = true },
            new Company { Id = 3, Name = "Akdeniz Turizm", Description = "Turkiye'nin en guzel sahillerinde unutulmaz deneyimler. Tekne turlari, dalıs aktiviteleri ve kultur gezileri duzenliyoruz.", Email = "info@akdenizturizm.com", Phone = "0242 555 9012", Address = "Antalya, Konyaalti Mah. Liman Cad. No:78", Website = "www.akdenizturizm.com", TaxNumber = "3456789012", LogoUrl = "https://picsum.photos/seed/akdeniz/200", Slug = "akdeniz-turizm", City = "Antalya", Tagline = "Akdeniz'in Mavisiyle Tanisin", FoundedYear = 2008, MetaTitle = "Akdeniz Turizm - Antalya Tekne ve Kultur Turlari", MetaDescription = "Kemer, Kas, Kekova tekne turlari. Akdeniz'in en guzel koylarini ve antik kentleri kesfedin.", CoverImageUrl = "https://picsum.photos/seed/akdeniz-cover/1200/400", StatusId = CompanyStatuses.Ids.Approved, ApplicationDate = now, ReviewedAt = now, ReviewedById = 1, CreatedAt = now, IsActive = true },
            new Company { Id = 4, Name = "Kapadokya Balonlari", Description = "Kapadokya'nin buleyuci peri bacalari uzerinde balon deneyimi ve kultur turlari. Yeralti sehirleri, vadiler ve daha fazlasi.", Email = "info@kapadokyabalonlari.com", Phone = "0384 555 3456", Address = "Nevsehir, Goreme Kasabasi Merkez No:12", Website = "www.kapadokyabalonlari.com", TaxNumber = "4567890123", LogoUrl = "https://picsum.photos/seed/kapadokya/200", Slug = "kapadokya-balonlari", City = "Nevsehir", Tagline = "Gokyuzunden Kapadokya", FoundedYear = 2012, MetaTitle = "Kapadokya Balonlari - Sicak Hava Balonu ve Tur", MetaDescription = "Kapadokya balon turu, yeralti sehirleri, ATV safari. Peri bacalari uzerinde unutulmaz deneyim.", CoverImageUrl = "https://picsum.photos/seed/kapadokya-cover/1200/400", StatusId = CompanyStatuses.Ids.Approved, ApplicationDate = now, ReviewedAt = now, ReviewedById = 1, CreatedAt = now, IsActive = true },
            new Company { Id = 5, Name = "Istanbul Turlari", Description = "Dunyanin en etkileyici sehrinde tarihi ve kulturel turlarin adresi. Sultanahmet, Bogaz turlari ve ozel organizasyonlar.", Email = "info@istanbulturlari.com", Phone = "0212 555 7890", Address = "Istanbul, Sultanahmet Mah. Divanyolu Cad. No:56", Website = "www.istanbulturlari.com", TaxNumber = "5678901234", LogoUrl = "https://picsum.photos/seed/istanbul/200", Slug = "istanbul-turlari", City = "Istanbul", Tagline = "Iki Kitanin Kesisim Noktasi", FoundedYear = 2005, MetaTitle = "Istanbul Turlari - Tarihi Yarimada ve Bogaz Turu", MetaDescription = "Sultanahmet, Ayasofya, Topkapi Sarayi, Bogaz turu. Istanbul'un tum guzelliklerini kesfedin.", CoverImageUrl = "https://picsum.photos/seed/istanbul-cover/1200/400", StatusId = CompanyStatuses.Ids.Approved, ApplicationDate = now, ReviewedAt = now, ReviewedById = 1, CreatedAt = now, IsActive = true }
        };
        modelBuilder.Entity<Company>().HasData(companies);

        // Turlar
        var tours = new[]
        {
            // Ege Tur
            new Tour { Id = 1, Name = "Efes Antik Kent Turu", Description = "Efes antik kenti ve Meryem Ana evi gezisi", Destination = "Selcuk, Izmir", Price = 750, DurationDays = 1, MaxCapacity = 40, ImageUrl = "https://picsum.photos/seed/efes/800/600", IsFeatured = true, CompanyId = 1, CreatedAt = now, IsActive = true },
            new Tour { Id = 2, Name = "Cesme-Alacati Turu", Description = "Cesme ve Alacati sokaklarinda keyifli bir gun", Destination = "Cesme, Izmir", Price = 500, DurationDays = 1, MaxCapacity = 30, ImageUrl = "https://picsum.photos/seed/cesme/800/600", IsFeatured = false, CompanyId = 1, CreatedAt = now, IsActive = true },
            new Tour { Id = 3, Name = "Pamukkale Turu", Description = "Beyaz travertenler ve Hierapolis antik kenti", Destination = "Pamukkale, Denizli", Price = 900, DurationDays = 2, MaxCapacity = 35, ImageUrl = "https://picsum.photos/seed/pamukkale/800/600", IsFeatured = true, CompanyId = 1, CreatedAt = now, IsActive = true },

            // Karadeniz Gezileri
            new Tour { Id = 4, Name = "Uzungol Turu", Description = "Uzungol ve cevresindeki yaylalar", Destination = "Uzungol, Trabzon", Price = 1200, DurationDays = 3, MaxCapacity = 25, ImageUrl = "https://picsum.photos/seed/uzungol/800/600", IsFeatured = true, CompanyId = 2, CreatedAt = now, IsActive = true },
            new Tour { Id = 5, Name = "Ayder Yaylasi Turu", Description = "Ayder yaylasi ve sicak su kaynaklari", Destination = "Ayder, Rize", Price = 1400, DurationDays = 4, MaxCapacity = 20, ImageUrl = "https://picsum.photos/seed/ayder/800/600", IsFeatured = false, CompanyId = 2, CreatedAt = now, IsActive = true },
            new Tour { Id = 6, Name = "Sumela Manastiri", Description = "Sumela Manastiri ve Trabzon gezisi", Destination = "Macka, Trabzon", Price = 800, DurationDays = 2, MaxCapacity = 30, ImageUrl = "https://picsum.photos/seed/sumela/800/600", IsFeatured = true, CompanyId = 2, CreatedAt = now, IsActive = true },

            // Akdeniz Turizm
            new Tour { Id = 7, Name = "Kemer Tekne Turu", Description = "Kemer koylarinda tekne turu", Destination = "Kemer, Antalya", Price = 600, DurationDays = 1, MaxCapacity = 50, ImageUrl = "https://picsum.photos/seed/kemer/800/600", IsFeatured = false, CompanyId = 3, CreatedAt = now, IsActive = true },
            new Tour { Id = 8, Name = "Kas-Kekova Turu", Description = "Batan sehir Kekova ve Kas", Destination = "Kas, Antalya", Price = 1100, DurationDays = 2, MaxCapacity = 25, ImageUrl = "https://picsum.photos/seed/kas/800/600", IsFeatured = true, CompanyId = 3, CreatedAt = now, IsActive = true },
            new Tour { Id = 9, Name = "Olimpos-Yanaras Turu", Description = "Olimpos antik kenti ve Yanaras alevi", Destination = "Olimpos, Antalya", Price = 850, DurationDays = 2, MaxCapacity = 30, ImageUrl = "https://picsum.photos/seed/olimpos/800/600", IsFeatured = false, CompanyId = 3, CreatedAt = now, IsActive = true },

            // Kapadokya Balonlari
            new Tour { Id = 10, Name = "Kapadokya Balon Turu", Description = "Peri bacalari uzerinde balon deneyimi", Destination = "Goreme, Nevsehir", Price = 3500, DurationDays = 1, MaxCapacity = 16, ImageUrl = "https://picsum.photos/seed/balon/800/600", IsFeatured = true, CompanyId = 4, CreatedAt = now, IsActive = true },
            new Tour { Id = 11, Name = "Kapadokya Kultur Turu", Description = "Yeralti sehirleri ve vadiler", Destination = "Kapadokya, Nevsehir", Price = 1800, DurationDays = 3, MaxCapacity = 30, ImageUrl = "https://picsum.photos/seed/vadiler/800/600", IsFeatured = true, CompanyId = 4, CreatedAt = now, IsActive = true },
            new Tour { Id = 12, Name = "ATV Safari", Description = "Kapadokya vadilerinde ATV macerasi", Destination = "Goreme, Nevsehir", Price = 700, DurationDays = 1, MaxCapacity = 20, ImageUrl = "https://picsum.photos/seed/atv/800/600", IsFeatured = false, CompanyId = 4, CreatedAt = now, IsActive = true },

            // Istanbul Turlari
            new Tour { Id = 13, Name = "Tarihi Yarimada Turu", Description = "Sultanahmet, Ayasofya, Topkapi", Destination = "Sultanahmet, Istanbul", Price = 450, DurationDays = 1, MaxCapacity = 40, ImageUrl = "https://picsum.photos/seed/sultanahmet/800/600", IsFeatured = true, CompanyId = 5, CreatedAt = now, IsActive = true },
            new Tour { Id = 14, Name = "Bogaz Turu", Description = "Istanbul Bogazinda tekne gezisi", Destination = "Bogaz, Istanbul", Price = 350, DurationDays = 1, MaxCapacity = 60, ImageUrl = "https://picsum.photos/seed/bogaz/800/600", IsFeatured = true, CompanyId = 5, CreatedAt = now, IsActive = true },
            new Tour { Id = 15, Name = "Prensesler Adalari", Description = "Buyukada ve Heybeliada gezisi", Destination = "Adalar, Istanbul", Price = 400, DurationDays = 1, MaxCapacity = 45, ImageUrl = "https://picsum.photos/seed/adalar/800/600", IsFeatured = false, CompanyId = 5, CreatedAt = now, IsActive = true }
        };
        modelBuilder.Entity<Tour>().HasData(tours);

        // Ziyaretciler (Kullanicilar)
        // PasswordHash = SHA256("123456") in Base64
        var passwordHash = "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=";

        var visitors = new[]
        {
            // Sistem Admin
            new Visitor { Id = 1, FirstName = "Sistem", LastName = "Admin", Email = "admin@erkantatilplani.com", Phone = "0532 111 1111", IdentityNumber = "11111111111", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.Admin, CompanyId = null, CreatedAt = now, IsActive = true },
            // Personel
            new Visitor { Id = 2, FirstName = "Personel", LastName = "Kullanici", Email = "staff@erkantatilplani.com", Phone = "0533 222 2222", IdentityNumber = "22222222222", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.Staff, CompanyId = null, CreatedAt = now, IsActive = true },
            // Firma sahipleri
            new Visitor { Id = 3, FirstName = "Ahmet", LastName = "Yilmaz", Email = "ahmet@egetur.com", Phone = "0534 333 3333", IdentityNumber = "33333333333", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.CompanyOwner, CompanyId = 1, CreatedAt = now, IsActive = true },
            new Visitor { Id = 4, FirstName = "Mehmet", LastName = "Kaya", Email = "mehmet@karadenizgezileri.com", Phone = "0535 444 4444", IdentityNumber = "44444444444", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.CompanyOwner, CompanyId = 2, CreatedAt = now, IsActive = true },
            new Visitor { Id = 5, FirstName = "Fatma", LastName = "Demir", Email = "fatma@akdenizturizm.com", Phone = "0536 555 5555", IdentityNumber = "55555555555", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.CompanyOwner, CompanyId = 3, CreatedAt = now, IsActive = true },
            new Visitor { Id = 6, FirstName = "Ali", LastName = "Celik", Email = "ali@kapadokyabalonlari.com", Phone = "0537 666 6666", IdentityNumber = "66666666666", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.CompanyOwner, CompanyId = 4, CreatedAt = now, IsActive = true },
            new Visitor { Id = 7, FirstName = "Ayse", LastName = "Ozturk", Email = "ayse@istanbulturlari.com", Phone = "0538 777 7777", IdentityNumber = "77777777777", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.CompanyOwner, CompanyId = 5, CreatedAt = now, IsActive = true },
            // Normal ziyaretciler
            new Visitor { Id = 8, FirstName = "Zeynep", LastName = "Arslan", Email = "zeynep@gmail.com", Phone = "0539 888 8888", IdentityNumber = "88888888888", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.Visitor, CompanyId = null, CreatedAt = now, IsActive = true },
            new Visitor { Id = 9, FirstName = "Mustafa", LastName = "Sahin", Email = "mustafa@gmail.com", Phone = "0530 999 9999", IdentityNumber = "99999999999", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.Visitor, CompanyId = null, CreatedAt = now, IsActive = true },
            new Visitor { Id = 10, FirstName = "Elif", LastName = "Yildiz", Email = "elif@gmail.com", Phone = "0531 000 0000", IdentityNumber = "10101010101", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.Visitor, CompanyId = null, CreatedAt = now, IsActive = true },
            new Visitor { Id = 11, FirstName = "Emre", LastName = "Koc", Email = "emre@gmail.com", Phone = "0541 111 1111", IdentityNumber = "12121212121", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.Visitor, CompanyId = null, CreatedAt = now, IsActive = true },
            new Visitor { Id = 12, FirstName = "Selin", LastName = "Aydin", Email = "selin@gmail.com", Phone = "0542 222 2222", IdentityNumber = "13131313131", PasswordHash = passwordHash, UserTypeId = UserTypes.Ids.Visitor, CompanyId = null, CreatedAt = now, IsActive = true }
        };
        modelBuilder.Entity<Visitor>().HasData(visitors);

        // Rezervasyonlar (Visitor ID'leri guncellendi: 8-12 arasi normal ziyaretciler)
        var reservations = new[]
        {
            new Reservation { Id = 1, TourId = 1, VisitorId = 8, StartDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 2, TotalPrice = 1500, Status = ReservationStatuses.Ids.Confirmed, Notes = "Ogle yemegi dahil", CreatedAt = now, IsActive = true },
            new Reservation { Id = 2, TourId = 4, VisitorId = 9, StartDate = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 4, TotalPrice = 4800, Status = ReservationStatuses.Ids.Pending, Notes = "Aile gezisi", CreatedAt = now, IsActive = true },
            new Reservation { Id = 3, TourId = 10, VisitorId = 10, StartDate = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 2, TotalPrice = 7000, Status = ReservationStatuses.Ids.Confirmed, Notes = "Balon ucusu sabah erken", CreatedAt = now, IsActive = true },
            new Reservation { Id = 4, TourId = 13, VisitorId = 11, StartDate = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 3, TotalPrice = 1350, Status = ReservationStatuses.Ids.Completed, Notes = "", CreatedAt = now, IsActive = true },
            new Reservation { Id = 5, TourId = 8, VisitorId = 12, StartDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 2, TotalPrice = 2200, Status = ReservationStatuses.Ids.Confirmed, Notes = "Balikadamla dalis isteniyor", CreatedAt = now, IsActive = true },
            new Reservation { Id = 6, TourId = 14, VisitorId = 8, StartDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 5, TotalPrice = 1750, Status = ReservationStatuses.Ids.Pending, Notes = "Ozel kutlama", CreatedAt = now, IsActive = true }
        };
        modelBuilder.Entity<Reservation>().HasData(reservations);

        // Diller - Sadece varsayilan diller, ceviriler XML'den yuklenecek
        var languages = new[]
        {
            new Language { Id = 1, Name = "Turkce", LanguageCulture = "tr-TR", UniqueSeoCode = "tr", FlagIcon = "fi fi-tr", IsDefault = true, DisplayOrder = 1, CreatedAt = now, IsActive = true },
            new Language { Id = 2, Name = "English", LanguageCulture = "en-US", UniqueSeoCode = "en", FlagIcon = "fi fi-us", IsDefault = false, DisplayOrder = 2, CreatedAt = now, IsActive = true },
            new Language { Id = 3, Name = "Deutsch", LanguageCulture = "de-DE", UniqueSeoCode = "de", FlagIcon = "fi fi-de", IsDefault = false, DisplayOrder = 3, CreatedAt = now, IsActive = true },
            new Language { Id = 4, Name = "Русский", LanguageCulture = "ru-RU", UniqueSeoCode = "ru", FlagIcon = "fi fi-ru", IsDefault = false, DisplayOrder = 4, CreatedAt = now, IsActive = true },
            new Language { Id = 5, Name = "Espanol", LanguageCulture = "es-ES", UniqueSeoCode = "es", FlagIcon = "fi fi-es", IsDefault = false, DisplayOrder = 5, CreatedAt = now, IsActive = true }
        };
        modelBuilder.Entity<Language>().HasData(languages);

        // Email Hesaplari
        var emailAccounts = new[]
        {
            new EmailAccount
            {
                Id = 1,
                Name = "default",
                Description = "Varsayilan email hesabi",
                SmtpHost = "smtp.gmail.com",
                SmtpPort = 587,
                SmtpUsername = "noreply@erkantatilplani.com",
                SmtpPassword = "",
                FromEmail = "noreply@erkantatilplani.com",
                FromName = "Erkan Tatil Plani",
                EnableSsl = true,
                IsDefault = true,
                DisplayOrder = 1,
                CreatedAt = now,
                IsActive = true
            },
            new EmailAccount
            {
                Id = 2,
                Name = "reservations",
                Description = "Rezervasyon emailleri icin",
                SmtpHost = "smtp.gmail.com",
                SmtpPort = 587,
                SmtpUsername = "reservations@erkantatilplani.com",
                SmtpPassword = "",
                FromEmail = "reservations@erkantatilplani.com",
                FromName = "Erkan Tatil Plani - Rezervasyonlar",
                EnableSsl = true,
                IsDefault = false,
                DisplayOrder = 2,
                CreatedAt = now,
                IsActive = true
            }
        };
        modelBuilder.Entity<EmailAccount>().HasData(emailAccounts);

        // Email Sablonlari (5 sistem sablonu)
        var emailTemplates = new[]
        {
            new EmailTemplate
            {
                Id = 1,
                Key = "password_reset",
                Name = "Sifre Sifirlama",
                Description = "Kullanici sifre sifirlama istegi yaptiginda gonderilir",
                EmailAccountId = 1,
                Placeholders = "[\"{customerName}\", \"{resetUrl}\"]",
                IsSystemTemplate = true,
                CreatedAt = now,
                IsActive = true
            },
            new EmailTemplate
            {
                Id = 2,
                Key = "email_verification",
                Name = "Email Dogrulama",
                Description = "Kullanici kayit oldugunda veya email dogrulamasi istediginde gonderilir",
                EmailAccountId = 1,
                Placeholders = "[\"{customerName}\", \"{verifyUrl}\"]",
                IsSystemTemplate = true,
                CreatedAt = now,
                IsActive = true
            },
            new EmailTemplate
            {
                Id = 3,
                Key = "reservation_confirmed",
                Name = "Rezervasyon Onaylandi",
                Description = "Rezervasyon onaylandiginda musteriye gonderilir",
                EmailAccountId = 2,
                Placeholders = "[\"{customerName}\", \"{tourName}\", \"{companyName}\", \"{destination}\", \"{startDate}\", \"{endDate}\", \"{numberOfPeople}\", \"{totalPrice}\"]",
                IsSystemTemplate = true,
                CreatedAt = now,
                IsActive = true
            },
            new EmailTemplate
            {
                Id = 4,
                Key = "reservation_cancelled",
                Name = "Rezervasyon Iptal Edildi",
                Description = "Rezervasyon iptal edildiginde musteriye gonderilir",
                EmailAccountId = 2,
                Placeholders = "[\"{customerName}\", \"{tourName}\", \"{companyName}\", \"{destination}\", \"{startDate}\", \"{endDate}\", \"{numberOfPeople}\", \"{totalPrice}\"]",
                IsSystemTemplate = true,
                CreatedAt = now,
                IsActive = true
            },
            new EmailTemplate
            {
                Id = 5,
                Key = "reservation_rejected",
                Name = "Rezervasyon Reddedildi",
                Description = "Rezervasyon reddedildiginde musteriye gonderilir",
                EmailAccountId = 2,
                Placeholders = "[\"{customerName}\", \"{tourName}\", \"{companyName}\", \"{destination}\", \"{startDate}\", \"{endDate}\", \"{numberOfPeople}\", \"{totalPrice}\", \"{rejectionReason}\"]",
                IsSystemTemplate = true,
                CreatedAt = now,
                IsActive = true
            }
        };
        modelBuilder.Entity<EmailTemplate>().HasData(emailTemplates);

        // Email Sablon Cevirileri (Her sablon icin tr ve en)
        SeedEmailTemplateTranslations(modelBuilder, now);

        // Blog Yazilari
        var publishedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var blogPosts = new[]
        {
            new BlogPost
            {
                Id = 1,
                Title = "Kapadokya'da Balon Turu: Bilmeniz Gereken Her Sey",
                Slug = "kapadokyada-balon-turu-bilmeniz-gereken-her-sey",
                Summary = "Kapadokya balon turuna katilmadan once bilmeniz gereken ipuclari, en iyi sezon ve hazirlik onerileri.",
                Content = "<h2>Kapadokya Balon Turu Rehberi</h2><p>Kapadokya'nin essiz peri bacalari uzerinde balon turu, dunyanin en etkileyici deneyimlerinden biridir. Bu yazida, balon turuna katilmadan once bilmeniz gereken her seyi paylasiyoruz.</p><h3>En Iyi Sezon</h3><p>Nisan-Kasim ayları arasinda hava kosullari balon ucuslari icin en uygun donemdir. Ozellikle Mayis-Haziran ve Eylul-Ekim aylarinda hava en stabil halindedir.</p><h3>Fiyatlar</h3><p>Standart ucuslar 150-250 Euro arasinda degismektedir. Ozel ucuslar ise 300 Euro'dan baslamaktadir.</p><h3>Hazirlik</h3><p>Sabah erken kalkmaniz gerekecektir. Rahat kiyafetler ve spor ayakkabi giyin. Kameranizi unutmayin!</p>",
                ImageUrl = "https://picsum.photos/seed/blog-kapadokya/800/400",
                CategoryId = BlogCategories.Ids.TravelTips,
                StatusId = BlogStatuses.Ids.Published,
                AuthorId = 1,
                CompanyId = null,
                ViewCount = 342,
                PublishedAt = publishedAt,
                Tags = "kapadokya,balon,seyahat ipucu",
                CreatedAt = now,
                IsActive = true
            },
            new BlogPost
            {
                Id = 2,
                Title = "Ege'nin Saklı Koyları: Keşfedilmemiş Cennetler",
                Slug = "egenin-sakli-koylari-kesfedilmemis-cennetler",
                Summary = "Ege sahillerinde turistik kalabaliktan uzak, huzurlu ve dogal koylari kesfedin.",
                Content = "<h2>Ege'nin Gizli Cennetleri</h2><p>Turkiye'nin Ege sahilleri, dunya uzerindeki en guzel kumsallara ve koylara ev sahipligi yapmaktadir. Bu yazida, henuz kesfedilmemis sakli koylari tanitiyoruz.</p><h3>1. Sazak Koyu - Mugla</h3><p>Berrak sulari ve bakir dogasiyla Sazak Koyu, huzur arayanlar icin ideal bir destinasyondur.</p><h3>2. Kabak Koyu - Fethiye</h3><p>Oludeniz'in guneybatisinda yer alan Kabak Koyu, dogal guzelligi ve sakli plajlariyla unlüdur.</p><h3>3. Hayıtbuku - Mugla</h3><p>Bodrum yakinlarindaki Hayitbuku, sakin atmosferi ve lezzetli deniz urunleriyle bilinir.</p>",
                ImageUrl = "https://picsum.photos/seed/blog-ege/800/400",
                CategoryId = BlogCategories.Ids.Destinations,
                StatusId = BlogStatuses.Ids.Published,
                AuthorId = 3,
                CompanyId = 1,
                ViewCount = 215,
                PublishedAt = new DateTime(2026, 1, 20, 14, 0, 0, DateTimeKind.Utc),
                Tags = "ege,koy,deniz,doga",
                CreatedAt = now,
                IsActive = true
            },
            new BlogPost
            {
                Id = 3,
                Title = "2026 Yilinda Turkiye Turizm Trendleri",
                Slug = "2026-yilinda-turkiye-turizm-trendleri",
                Summary = "2026 yilinda Turkiye turizmini sekillendiren trendler ve yenilikler hakkinda detayli analiz.",
                Content = "<h2>2026 Turizm Trendleri</h2><p>Turkiye turizm sektoru hizla gelismektedir. Iste 2026 yilinin one cikan trendleri:</p><h3>Eko-Turizm Yukseliste</h3><p>Surdurulebilir turizm anlayisi giderek yayginlasmaktadir. Dogayla uyumlu konaklama tesisleri ve karbon ayak izini azaltan tur programlari populerlesmektedir.</p><h3>Dijital Deneyimler</h3><p>Sanal gerceklik turlari ve arttirilmis gerceklik uygulamalari turizm sektorunde yeni firsatlar sunmaktadir.</p><h3>Gastronomi Turizmi</h3><p>Yerel lezzetleri kesfetmek amaciyla yapilan seyahatler her gecen gun artmaktadir.</p>",
                ImageUrl = "https://picsum.photos/seed/blog-trends/800/400",
                CategoryId = BlogCategories.Ids.News,
                StatusId = BlogStatuses.Ids.Published,
                AuthorId = 1,
                CompanyId = null,
                ViewCount = 178,
                PublishedAt = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc),
                Tags = "turizm,trend,2026",
                CreatedAt = now,
                IsActive = true
            },
            new BlogPost
            {
                Id = 4,
                Title = "Karadeniz Yaylalarinda Lezzet Duragi",
                Slug = "karadeniz-yaylalarinda-lezzet-duragi",
                Summary = "Karadeniz mutfagininin en ozel lezzetlerini yaylalarda tatma rehberi.",
                Content = "<h2>Karadeniz Lezzetleri</h2><p>Karadeniz mutfagi, Turkiye'nin en zengin ve ozgun mutfaklarindan biridir.</p><h3>Muhlama</h3><p>Karadeniz'in en meshur lezzeti muhlama, tereyagi ve peynirle yapilan essiz bir yemektir.</p><h3>Kuymak</h3><p>Misir unu ve peynirle hazirlanan kuymak, ozellikle kis aylarinda tercih edilir.</p><h3>Hamsi</h3><p>Karadeniz denince akla ilk gelen balik olan hamsi, tava, pilav ve hatta tatli olarak bile hazirlanir.</p>",
                ImageUrl = "https://picsum.photos/seed/blog-karadeniz/800/400",
                CategoryId = BlogCategories.Ids.FoodAndDrink,
                StatusId = BlogStatuses.Ids.Published,
                AuthorId = 4,
                CompanyId = 2,
                ViewCount = 124,
                PublishedAt = new DateTime(2026, 2, 5, 11, 0, 0, DateTimeKind.Utc),
                Tags = "karadeniz,yemek,lezzet,yayla",
                CreatedAt = now,
                IsActive = true
            }
        };
        modelBuilder.Entity<BlogPost>().HasData(blogPosts);

        // Blog Yorumlari
        var blogComments = new[]
        {
            new BlogComment { Id = 1, BlogPostId = 1, VisitorId = 8, Content = "Cok faydali bir yazi olmus, tesekkurler! Kapadokya'ya gitmeden once mutlaka okunsun.", CreatedAt = now, IsActive = true },
            new BlogComment { Id = 2, BlogPostId = 1, VisitorId = 9, Content = "Gecen yil gittik, gercekten harikaydı. Eylul ayini tavsiye ederim.", CreatedAt = now, IsActive = true },
            new BlogComment { Id = 3, BlogPostId = 2, VisitorId = 10, Content = "Kabak Koyu gercekten muhtesem! Herkesin gitmesi lazim.", CreatedAt = now, IsActive = true },
            new BlogComment { Id = 4, BlogPostId = 3, VisitorId = 11, Content = "Eko-turizm trendi cok guzel, doga korundukca turizm de gelisir.", CreatedAt = now, IsActive = true }
        };
        modelBuilder.Entity<BlogComment>().HasData(blogComments);
    }

    private void SeedEmailTemplateTranslations(ModelBuilder modelBuilder, DateTime now)
    {
        var translations = new List<EmailTemplateTranslation>();
        var id = 1;

        // Password Reset - TR
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 1,
            LanguageCode = "tr",
            Subject = "Sifre Sifirlama - Erkan Tatil Plani",
            Body = GetPasswordResetTemplateBody("tr"),
            CreatedAt = now,
            IsActive = true
        });

        // Password Reset - EN
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 1,
            LanguageCode = "en",
            Subject = "Password Reset - Erkan Tatil Plani",
            Body = GetPasswordResetTemplateBody("en"),
            CreatedAt = now,
            IsActive = true
        });

        // Email Verification - TR
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 2,
            LanguageCode = "tr",
            Subject = "Email Dogrulama - Erkan Tatil Plani",
            Body = GetEmailVerificationTemplateBody("tr"),
            CreatedAt = now,
            IsActive = true
        });

        // Email Verification - EN
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 2,
            LanguageCode = "en",
            Subject = "Email Verification - Erkan Tatil Plani",
            Body = GetEmailVerificationTemplateBody("en"),
            CreatedAt = now,
            IsActive = true
        });

        // Reservation Confirmed - TR
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 3,
            LanguageCode = "tr",
            Subject = "Rezervasyonunuz Onaylandi - Erkan Tatil Plani",
            Body = GetReservationConfirmedTemplateBody("tr"),
            CreatedAt = now,
            IsActive = true
        });

        // Reservation Confirmed - EN
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 3,
            LanguageCode = "en",
            Subject = "Your Reservation is Confirmed - Erkan Tatil Plani",
            Body = GetReservationConfirmedTemplateBody("en"),
            CreatedAt = now,
            IsActive = true
        });

        // Reservation Cancelled - TR
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 4,
            LanguageCode = "tr",
            Subject = "Rezervasyonunuz Iptal Edildi - Erkan Tatil Plani",
            Body = GetReservationCancelledTemplateBody("tr"),
            CreatedAt = now,
            IsActive = true
        });

        // Reservation Cancelled - EN
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 4,
            LanguageCode = "en",
            Subject = "Your Reservation has been Cancelled - Erkan Tatil Plani",
            Body = GetReservationCancelledTemplateBody("en"),
            CreatedAt = now,
            IsActive = true
        });

        // Reservation Rejected - TR
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 5,
            LanguageCode = "tr",
            Subject = "Rezervasyonunuz Reddedildi - Erkan Tatil Plani",
            Body = GetReservationRejectedTemplateBody("tr"),
            CreatedAt = now,
            IsActive = true
        });

        // Reservation Rejected - EN
        translations.Add(new EmailTemplateTranslation
        {
            Id = id++,
            EmailTemplateId = 5,
            LanguageCode = "en",
            Subject = "Your Reservation has been Rejected - Erkan Tatil Plani",
            Body = GetReservationRejectedTemplateBody("en"),
            CreatedAt = now,
            IsActive = true
        });

        modelBuilder.Entity<EmailTemplateTranslation>().HasData(translations);
    }

    private static string GetEmailBaseTemplate(string content) => $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; }}
        .header h1 {{ color: #ffffff; margin: 0; font-size: 24px; }}
        .content {{ padding: 40px 30px; }}
        .btn {{ display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; padding: 14px 30px; text-decoration: none; border-radius: 5px; font-weight: 600; margin: 20px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6c757d; }}
        .details {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .details-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e9ecef; }}
        .details-row:last-child {{ border-bottom: none; }}
    </style>
</head>
<body>
    <div class='container'>
        {content}
        <div class='footer'>
            <p>Bu email Erkan Tatil Plani tarafindan otomatik olarak gonderilmistir.</p>
            <p>&copy; 2026 Erkan Tatil Plani. Tum haklari saklidir.</p>
        </div>
    </div>
</body>
</html>";

    private static string GetPasswordResetTemplateBody(string lang) => lang == "tr"
        ? GetEmailBaseTemplate(@"
            <div class='header'><h1>Sifre Sifirlama</h1></div>
            <div class='content'>
                <p>Merhaba {customerName},</p>
                <p>Sifrenizi sifirlamak icin asagidaki butona tiklayin. Eger bu istegi siz yapmadiysiniz, bu emaili gormezden gelebilirsiniz.</p>
                <p style='text-align: center;'><a href='{resetUrl}' class='btn'>Sifremi Sifirla</a></p>
                <p style='color: #6c757d; font-size: 14px;'>Bu link 1 saat icinde gecerliligini yitirecektir.</p>
            </div>")
        : GetEmailBaseTemplate(@"
            <div class='header'><h1>Password Reset</h1></div>
            <div class='content'>
                <p>Hello {customerName},</p>
                <p>Click the button below to reset your password. If you did not make this request, you can ignore this email.</p>
                <p style='text-align: center;'><a href='{resetUrl}' class='btn'>Reset My Password</a></p>
                <p style='color: #6c757d; font-size: 14px;'>This link will expire in 1 hour.</p>
            </div>");

    private static string GetEmailVerificationTemplateBody(string lang) => lang == "tr"
        ? GetEmailBaseTemplate(@"
            <div class='header'><h1>Email Dogrulama</h1></div>
            <div class='content'>
                <p>Merhaba {customerName},</p>
                <p>Hesabinizi aktif etmek ve tum ozelliklere erisebilmek icin email adresinizi dogrulayin.</p>
                <p style='text-align: center;'><a href='{verifyUrl}' class='btn'>Email Adresimi Dogrula</a></p>
            </div>")
        : GetEmailBaseTemplate(@"
            <div class='header'><h1>Email Verification</h1></div>
            <div class='content'>
                <p>Hello {customerName},</p>
                <p>Verify your email address to activate your account and access all features.</p>
                <p style='text-align: center;'><a href='{verifyUrl}' class='btn'>Verify My Email</a></p>
            </div>");

    private static string GetReservationConfirmedTemplateBody(string lang) => lang == "tr"
        ? GetEmailBaseTemplate(@"
            <div class='header'><h1>Rezervasyonunuz Onaylandi!</h1></div>
            <div class='content'>
                <p>Merhaba {customerName},</p>
                <p>Rezervasyonunuz basariyla onaylandi. Asagida rezervasyon detaylarinizi bulabilirsiniz.</p>
                <div class='details'>
                    <div class='details-row'><span><strong>Tur:</strong></span><span>{tourName}</span></div>
                    <div class='details-row'><span><strong>Firma:</strong></span><span>{companyName}</span></div>
                    <div class='details-row'><span><strong>Destinasyon:</strong></span><span>{destination}</span></div>
                    <div class='details-row'><span><strong>Tarih:</strong></span><span>{startDate} - {endDate}</span></div>
                    <div class='details-row'><span><strong>Kisi Sayisi:</strong></span><span>{numberOfPeople}</span></div>
                    <div class='details-row'><span><strong>Toplam:</strong></span><span>{totalPrice} TL</span></div>
                </div>
                <p>Bizi tercih ettiginiz icin tesekkur ederiz. Iyi tatiller dileriz!</p>
            </div>")
        : GetEmailBaseTemplate(@"
            <div class='header'><h1>Your Reservation is Confirmed!</h1></div>
            <div class='content'>
                <p>Hello {customerName},</p>
                <p>Your reservation has been confirmed successfully. Below you can find your reservation details.</p>
                <div class='details'>
                    <div class='details-row'><span><strong>Tour:</strong></span><span>{tourName}</span></div>
                    <div class='details-row'><span><strong>Company:</strong></span><span>{companyName}</span></div>
                    <div class='details-row'><span><strong>Destination:</strong></span><span>{destination}</span></div>
                    <div class='details-row'><span><strong>Date:</strong></span><span>{startDate} - {endDate}</span></div>
                    <div class='details-row'><span><strong>Number of People:</strong></span><span>{numberOfPeople}</span></div>
                    <div class='details-row'><span><strong>Total:</strong></span><span>{totalPrice} TL</span></div>
                </div>
                <p>Thank you for choosing us. Have a great trip!</p>
            </div>");

    private static string GetReservationCancelledTemplateBody(string lang) => lang == "tr"
        ? GetEmailBaseTemplate(@"
            <div class='header'><h1>Rezervasyonunuz Iptal Edildi</h1></div>
            <div class='content'>
                <p>Merhaba {customerName},</p>
                <p>Rezervasyonunuz iptal edilmistir.</p>
                <div class='details'>
                    <div class='details-row'><span><strong>Tur:</strong></span><span>{tourName}</span></div>
                    <div class='details-row'><span><strong>Firma:</strong></span><span>{companyName}</span></div>
                    <div class='details-row'><span><strong>Destinasyon:</strong></span><span>{destination}</span></div>
                    <div class='details-row'><span><strong>Tarih:</strong></span><span>{startDate} - {endDate}</span></div>
                    <div class='details-row'><span><strong>Kisi Sayisi:</strong></span><span>{numberOfPeople}</span></div>
                    <div class='details-row'><span><strong>Toplam:</strong></span><span>{totalPrice} TL</span></div>
                </div>
                <p>Sorulariniz icin bizimle iletisime gecebilirsiniz.</p>
            </div>")
        : GetEmailBaseTemplate(@"
            <div class='header'><h1>Your Reservation has been Cancelled</h1></div>
            <div class='content'>
                <p>Hello {customerName},</p>
                <p>Your reservation has been cancelled.</p>
                <div class='details'>
                    <div class='details-row'><span><strong>Tour:</strong></span><span>{tourName}</span></div>
                    <div class='details-row'><span><strong>Company:</strong></span><span>{companyName}</span></div>
                    <div class='details-row'><span><strong>Destination:</strong></span><span>{destination}</span></div>
                    <div class='details-row'><span><strong>Date:</strong></span><span>{startDate} - {endDate}</span></div>
                    <div class='details-row'><span><strong>Number of People:</strong></span><span>{numberOfPeople}</span></div>
                    <div class='details-row'><span><strong>Total:</strong></span><span>{totalPrice} TL</span></div>
                </div>
                <p>For any questions, please contact us.</p>
            </div>");

    private static string GetReservationRejectedTemplateBody(string lang) => lang == "tr"
        ? GetEmailBaseTemplate(@"
            <div class='header'><h1>Rezervasyonunuz Reddedildi</h1></div>
            <div class='content'>
                <p>Merhaba {customerName},</p>
                <p>Uzulerek bildirmek isteriz ki rezervasyonunuz reddedilmistir.</p>
                <div class='details'>
                    <div class='details-row'><span><strong>Tur:</strong></span><span>{tourName}</span></div>
                    <div class='details-row'><span><strong>Firma:</strong></span><span>{companyName}</span></div>
                    <div class='details-row'><span><strong>Destinasyon:</strong></span><span>{destination}</span></div>
                    <div class='details-row'><span><strong>Tarih:</strong></span><span>{startDate} - {endDate}</span></div>
                    <div class='details-row'><span><strong>Kisi Sayisi:</strong></span><span>{numberOfPeople}</span></div>
                    <div class='details-row'><span><strong>Toplam:</strong></span><span>{totalPrice} TL</span></div>
                    <div class='details-row'><span><strong>Sebep:</strong></span><span>{rejectionReason}</span></div>
                </div>
                <p>Sorulariniz icin bizimle iletisime gecebilirsiniz.</p>
            </div>")
        : GetEmailBaseTemplate(@"
            <div class='header'><h1>Your Reservation has been Rejected</h1></div>
            <div class='content'>
                <p>Hello {customerName},</p>
                <p>We regret to inform you that your reservation has been rejected.</p>
                <div class='details'>
                    <div class='details-row'><span><strong>Tour:</strong></span><span>{tourName}</span></div>
                    <div class='details-row'><span><strong>Company:</strong></span><span>{companyName}</span></div>
                    <div class='details-row'><span><strong>Destination:</strong></span><span>{destination}</span></div>
                    <div class='details-row'><span><strong>Date:</strong></span><span>{startDate} - {endDate}</span></div>
                    <div class='details-row'><span><strong>Number of People:</strong></span><span>{numberOfPeople}</span></div>
                    <div class='details-row'><span><strong>Total:</strong></span><span>{totalPrice} TL</span></div>
                    <div class='details-row'><span><strong>Reason:</strong></span><span>{rejectionReason}</span></div>
                </div>
                <p>For any questions, please contact us.</p>
            </div>");
}
