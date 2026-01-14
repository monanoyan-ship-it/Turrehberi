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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TaxNumber).IsUnique();
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Tours)
                  .HasForeignKey(e => e.CompanyId);
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
            entity.HasOne(e => e.Tour)
                  .WithMany(t => t.Reservations)
                  .HasForeignKey(e => e.TourId);
            entity.HasOne(e => e.Visitor)
                  .WithMany(v => v.Reservations)
                  .HasForeignKey(e => e.VisitorId);
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

        // Seed Data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Firmalar
        var companies = new[]
        {
            new Company { Id = 1, Name = "Ege Tur", Description = "Ege bolgesi turlari", Email = "info@egetur.com", Phone = "0232 555 1234", Address = "Izmir, Alsancak", Website = "www.egetur.com", TaxNumber = "1234567890", LogoUrl = "https://picsum.photos/seed/egetur/200", CreatedAt = now, IsActive = true },
            new Company { Id = 2, Name = "Karadeniz Gezileri", Description = "Karadeniz yayla turlari", Email = "info@karadenizgezileri.com", Phone = "0462 555 5678", Address = "Trabzon, Meydan", Website = "www.karadenizgezileri.com", TaxNumber = "2345678901", LogoUrl = "https://picsum.photos/seed/karadeniz/200", CreatedAt = now, IsActive = true },
            new Company { Id = 3, Name = "Akdeniz Turizm", Description = "Akdeniz sahil turlari", Email = "info@akdenizturizm.com", Phone = "0242 555 9012", Address = "Antalya, Konyaalti", Website = "www.akdenizturizm.com", TaxNumber = "3456789012", LogoUrl = "https://picsum.photos/seed/akdeniz/200", CreatedAt = now, IsActive = true },
            new Company { Id = 4, Name = "Kapadokya Balonlari", Description = "Kapadokya balon ve kultur turlari", Email = "info@kapadokyabalonlari.com", Phone = "0384 555 3456", Address = "Nevsehir, Goreme", Website = "www.kapadokyabalonlari.com", TaxNumber = "4567890123", LogoUrl = "https://picsum.photos/seed/kapadokya/200", CreatedAt = now, IsActive = true },
            new Company { Id = 5, Name = "Istanbul Turlari", Description = "Istanbul sehir ve bogazici turlari", Email = "info@istanbulturlari.com", Phone = "0212 555 7890", Address = "Istanbul, Sultanahmet", Website = "www.istanbulturlari.com", TaxNumber = "5678901234", LogoUrl = "https://picsum.photos/seed/istanbul/200", CreatedAt = now, IsActive = true }
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
        var passwordHash = "jZae725q0zKaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=";

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
            new Reservation { Id = 1, TourId = 1, VisitorId = 8, StartDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 2, TotalPrice = 1500, Status = ReservationStatus.Confirmed, Notes = "Ogle yemegi dahil", CreatedAt = now, IsActive = true },
            new Reservation { Id = 2, TourId = 4, VisitorId = 9, StartDate = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 4, TotalPrice = 4800, Status = ReservationStatus.Pending, Notes = "Aile gezisi", CreatedAt = now, IsActive = true },
            new Reservation { Id = 3, TourId = 10, VisitorId = 10, StartDate = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 2, TotalPrice = 7000, Status = ReservationStatus.Confirmed, Notes = "Balon ucusu sabah erken", CreatedAt = now, IsActive = true },
            new Reservation { Id = 4, TourId = 13, VisitorId = 11, StartDate = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 3, TotalPrice = 1350, Status = ReservationStatus.Completed, Notes = "", CreatedAt = now, IsActive = true },
            new Reservation { Id = 5, TourId = 8, VisitorId = 12, StartDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 2, TotalPrice = 2200, Status = ReservationStatus.Confirmed, Notes = "Balikadamla dalis isteniyor", CreatedAt = now, IsActive = true },
            new Reservation { Id = 6, TourId = 14, VisitorId = 8, StartDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc), NumberOfPeople = 5, TotalPrice = 1750, Status = ReservationStatus.Pending, Notes = "Ozel kutlama", CreatedAt = now, IsActive = true }
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
    }
}
