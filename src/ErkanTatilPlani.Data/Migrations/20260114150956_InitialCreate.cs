using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ErkanTatilPlani.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: false),
                    TaxNumber = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LanguageCulture = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UniqueSeoCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    FlagIcon = table.Column<string>(type: "text", nullable: true),
                    Rtl = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Destination = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tours_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    IdentityNumber = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    UserTypeId = table.Column<int>(type: "integer", nullable: false),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitors_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocaleStringResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LanguageId = table.Column<int>(type: "integer", nullable: false),
                    ResourceName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ResourceValue = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocaleStringResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocaleStringResources_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TourId = table.Column<int>(type: "integer", nullable: false),
                    VisitorId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "integer", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Visitors_VisitorId",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Address", "CreatedAt", "Description", "Email", "IsActive", "LogoUrl", "Name", "Phone", "TaxNumber", "UpdatedAt", "Website" },
                values: new object[,]
                {
                    { 1, "Izmir, Alsancak", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ege bolgesi turlari", "info@egetur.com", true, "https://picsum.photos/seed/egetur/200", "Ege Tur", "0232 555 1234", "1234567890", null, "www.egetur.com" },
                    { 2, "Trabzon, Meydan", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Karadeniz yayla turlari", "info@karadenizgezileri.com", true, "https://picsum.photos/seed/karadeniz/200", "Karadeniz Gezileri", "0462 555 5678", "2345678901", null, "www.karadenizgezileri.com" },
                    { 3, "Antalya, Konyaalti", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Akdeniz sahil turlari", "info@akdenizturizm.com", true, "https://picsum.photos/seed/akdeniz/200", "Akdeniz Turizm", "0242 555 9012", "3456789012", null, "www.akdenizturizm.com" },
                    { 4, "Nevsehir, Goreme", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kapadokya balon ve kultur turlari", "info@kapadokyabalonlari.com", true, "https://picsum.photos/seed/kapadokya/200", "Kapadokya Balonlari", "0384 555 3456", "4567890123", null, "www.kapadokyabalonlari.com" },
                    { 5, "Istanbul, Sultanahmet", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Istanbul sehir ve bogazici turlari", "info@istanbulturlari.com", true, "https://picsum.photos/seed/istanbul/200", "Istanbul Turlari", "0212 555 7890", "5678901234", null, "www.istanbulturlari.com" }
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "CreatedAt", "DisplayOrder", "FlagIcon", "IsActive", "IsDefault", "LanguageCulture", "Name", "Rtl", "UniqueSeoCode", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, "fi fi-tr", true, true, "tr-TR", "Turkce", false, "tr", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, "fi fi-us", true, false, "en-US", "English", false, "en", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, "fi fi-de", true, false, "de-DE", "Deutsch", false, "de", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, "fi fi-ru", true, false, "ru-RU", "Русский", false, "ru", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, "fi fi-es", true, false, "es-ES", "Espanol", false, "es", null }
                });

            migrationBuilder.InsertData(
                table: "Visitors",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Email", "FirstName", "IdentityNumber", "IsActive", "LastName", "PasswordHash", "Phone", "UpdatedAt", "UserTypeId" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@erkantatilplani.com", "Sistem", "11111111111", true, "Admin", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0532 111 1111", null, 3 },
                    { 2, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "staff@erkantatilplani.com", "Personel", "22222222222", true, "Kullanici", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0533 222 2222", null, 2 },
                    { 8, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "zeynep@gmail.com", "Zeynep", "88888888888", true, "Arslan", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0539 888 8888", null, 0 },
                    { 9, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mustafa@gmail.com", "Mustafa", "99999999999", true, "Sahin", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0530 999 9999", null, 0 },
                    { 10, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "elif@gmail.com", "Elif", "10101010101", true, "Yildiz", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0531 000 0000", null, 0 },
                    { 11, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "emre@gmail.com", "Emre", "12121212121", true, "Koc", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0541 111 1111", null, 0 },
                    { 12, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "selin@gmail.com", "Selin", "13131313131", true, "Aydin", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0542 222 2222", null, 0 }
                });

            migrationBuilder.InsertData(
                table: "Tours",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Description", "Destination", "DurationDays", "ImageUrl", "IsActive", "IsFeatured", "MaxCapacity", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Efes antik kenti ve Meryem Ana evi gezisi", "Selcuk, Izmir", 1, "https://picsum.photos/seed/efes/800/600", true, true, 40, "Efes Antik Kent Turu", 750m, null },
                    { 2, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cesme ve Alacati sokaklarinda keyifli bir gun", "Cesme, Izmir", 1, "https://picsum.photos/seed/cesme/800/600", true, false, 30, "Cesme-Alacati Turu", 500m, null },
                    { 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Beyaz travertenler ve Hierapolis antik kenti", "Pamukkale, Denizli", 2, "https://picsum.photos/seed/pamukkale/800/600", true, true, 35, "Pamukkale Turu", 900m, null },
                    { 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Uzungol ve cevresindeki yaylalar", "Uzungol, Trabzon", 3, "https://picsum.photos/seed/uzungol/800/600", true, true, 25, "Uzungol Turu", 1200m, null },
                    { 5, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ayder yaylasi ve sicak su kaynaklari", "Ayder, Rize", 4, "https://picsum.photos/seed/ayder/800/600", true, false, 20, "Ayder Yaylasi Turu", 1400m, null },
                    { 6, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sumela Manastiri ve Trabzon gezisi", "Macka, Trabzon", 2, "https://picsum.photos/seed/sumela/800/600", true, true, 30, "Sumela Manastiri", 800m, null },
                    { 7, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kemer koylarinda tekne turu", "Kemer, Antalya", 1, "https://picsum.photos/seed/kemer/800/600", true, false, 50, "Kemer Tekne Turu", 600m, null },
                    { 8, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Batan sehir Kekova ve Kas", "Kas, Antalya", 2, "https://picsum.photos/seed/kas/800/600", true, true, 25, "Kas-Kekova Turu", 1100m, null },
                    { 9, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Olimpos antik kenti ve Yanaras alevi", "Olimpos, Antalya", 2, "https://picsum.photos/seed/olimpos/800/600", true, false, 30, "Olimpos-Yanaras Turu", 850m, null },
                    { 10, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Peri bacalari uzerinde balon deneyimi", "Goreme, Nevsehir", 1, "https://picsum.photos/seed/balon/800/600", true, true, 16, "Kapadokya Balon Turu", 3500m, null },
                    { 11, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yeralti sehirleri ve vadiler", "Kapadokya, Nevsehir", 3, "https://picsum.photos/seed/vadiler/800/600", true, true, 30, "Kapadokya Kultur Turu", 1800m, null },
                    { 12, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kapadokya vadilerinde ATV macerasi", "Goreme, Nevsehir", 1, "https://picsum.photos/seed/atv/800/600", true, false, 20, "ATV Safari", 700m, null },
                    { 13, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sultanahmet, Ayasofya, Topkapi", "Sultanahmet, Istanbul", 1, "https://picsum.photos/seed/sultanahmet/800/600", true, true, 40, "Tarihi Yarimada Turu", 450m, null },
                    { 14, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Istanbul Bogazinda tekne gezisi", "Bogaz, Istanbul", 1, "https://picsum.photos/seed/bogaz/800/600", true, true, 60, "Bogaz Turu", 350m, null },
                    { 15, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Buyukada ve Heybeliada gezisi", "Adalar, Istanbul", 1, "https://picsum.photos/seed/adalar/800/600", true, false, 45, "Prensesler Adalari", 400m, null }
                });

            migrationBuilder.InsertData(
                table: "Visitors",
                columns: new[] { "Id", "CompanyId", "CreatedAt", "Email", "FirstName", "IdentityNumber", "IsActive", "LastName", "PasswordHash", "Phone", "UpdatedAt", "UserTypeId" },
                values: new object[,]
                {
                    { 3, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ahmet@egetur.com", "Ahmet", "33333333333", true, "Yilmaz", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0534 333 3333", null, 1 },
                    { 4, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "mehmet@karadenizgezileri.com", "Mehmet", "44444444444", true, "Kaya", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0535 444 4444", null, 1 },
                    { 5, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "fatma@akdenizturizm.com", "Fatma", "55555555555", true, "Demir", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0536 555 5555", null, 1 },
                    { 6, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ali@kapadokyabalonlari.com", "Ali", "66666666666", true, "Celik", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0537 666 6666", null, 1 },
                    { 7, 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ayse@istanbulturlari.com", "Ayse", "77777777777", true, "Ozturk", "AQAAAAIAAYagAAAAELbJmz5DIWnN7p8HA8VQl8dZl5qQBqCmHr5gw0wEj7FdhFfP3g==", "0538 777 7777", null, 1 }
                });

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "CreatedAt", "EndDate", "IsActive", "Notes", "NumberOfPeople", "StartDate", "Status", "TotalPrice", "TourId", "UpdatedAt", "VisitorId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "Ogle yemegi dahil", 2, new DateTime(2026, 2, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1500m, 1, null, 8 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 12, 0, 0, 0, 0, DateTimeKind.Utc), true, "Aile gezisi", 4, new DateTime(2026, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc), 0, 4800m, 4, null, 9 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Utc), true, "Balon ucusu sabah erken", 2, new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Utc), 1, 7000m, 10, null, 10 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), true, "", 3, new DateTime(2026, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc), 3, 1350m, 13, null, 11 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 2, 0, 0, 0, 0, DateTimeKind.Utc), true, "Balikadamla dalis isteniyor", 2, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2200m, 8, null, 12 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 25, 0, 0, 0, 0, DateTimeKind.Utc), true, "Ozel kutlama", 5, new DateTime(2026, 3, 25, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1750m, 14, null, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Email",
                table: "Companies",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TaxNumber",
                table: "Companies",
                column: "TaxNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_LanguageCulture",
                table: "Languages",
                column: "LanguageCulture",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_UniqueSeoCode",
                table: "Languages",
                column: "UniqueSeoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocaleStringResources_LanguageId_ResourceName",
                table: "LocaleStringResources",
                columns: new[] { "LanguageId", "ResourceName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TourId",
                table: "Reservations",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_VisitorId",
                table: "Reservations",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_CompanyId",
                table: "Tours",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_CompanyId",
                table: "Visitors",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_Email",
                table: "Visitors",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocaleStringResources");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropTable(
                name: "Visitors");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
