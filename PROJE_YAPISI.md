# Erkan Tatil Plani - Proje Yapisi

## Genel Bakis

Erkan Tatil Plani, tur firmalari ve ziyaretciler icin gelistirilmis bir tur rezervasyon sistemidir. Sistem, firmalarin turlarini yonetmesine ve ziyaretcilerin bu turlara rezervasyon yapmasina olanak tanir.

---

## Teknoloji Yigini

| Katman | Teknoloji |
|--------|-----------|
| **Framework** | .NET 9.0 |
| **Web** | ASP.NET Core MVC + KnockoutJS SPA |
| **API** | ASP.NET Core Web API |
| **Veritabani** | PostgreSQL |
| **ORM** | Entity Framework Core 9.0 |
| **Frontend** | Bootstrap 5.3, Bootstrap Icons, KnockoutJS, jQuery, Toastr |
| **Mobil** | .NET MAUI (Android, iOS, Windows, macOS) |

---

## Proje Klasor Yapisi

```
ErkanTatilPlani/
├── ErkanTatilPlani.sln              # Ana solution dosyasi
├── PROJE_YAPISI.md                  # Bu dokuman
│
└── src/
    ├── ErkanTatilPlani.Core/        # Domain katmani
    │   └── Entities/
    │       ├── BaseEntity.cs
    │       ├── Company.cs
    │       ├── Tour.cs
    │       ├── Visitor.cs
    │       └── Reservation.cs
    │
    ├── ErkanTatilPlani.Data/        # Veri erisim katmani
    │   ├── Context/
    │   │   └── AppDbContext.cs
    │   └── Migrations/
    │
    ├── ErkanTatilPlani.API/         # REST API
    │   ├── Controllers/
    │   │   ├── CompaniesController.cs
    │   │   ├── ToursController.cs
    │   │   ├── VisitorsController.cs
    │   │   └── ReservationsController.cs
    │   ├── Program.cs               # Auto-migration dahil
    │   └── appsettings.json
    │
    ├── ErkanTatilPlani.Web/         # MVC + KnockoutJS SPA
    │   ├── Controllers/             # Sadece Index action
    │   │   ├── HomeController.cs
    │   │   ├── CompaniesController.cs
    │   │   ├── ToursController.cs
    │   │   ├── VisitorsController.cs
    │   │   └── ReservationsController.cs
    │   ├── Views/
    │   │   ├── Home/Index.cshtml    # Ana sayfa + featured turlar
    │   │   ├── Companies/Index.cshtml # SPA
    │   │   ├── Tours/Index.cshtml   # SPA
    │   │   ├── Visitors/Index.cshtml # SPA
    │   │   ├── Reservations/Index.cshtml # SPA
    │   │   └── Shared/_Layout.cshtml
    │   ├── wwwroot/lib/             # Client-side kutuphaneler
    │   │   ├── bootstrap/
    │   │   ├── bootstrap-icons/
    │   │   ├── knockout/
    │   │   ├── jquery/
    │   │   └── toastr/
    │   ├── libman.json              # Library manager config
    │   ├── Program.cs
    │   └── appsettings.json
    │
    └── ErkanTatilPlani.Mobile/      # MAUI Mobil Uygulama
        ├── Services/
        │   └── ApiService.cs
        ├── Views/
        └── MauiProgram.cs
```

---

## Entity (Varlik) Modelleri

### BaseEntity (Temel Sinif)
Tum entity'lerin miras aldigi temel sinif.

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
```

### Company (Firma)
Tur saglayici firmalar. Sadece admin tarafindan eklenir.

| Alan | Tip | Aciklama |
|------|-----|----------|
| Id | int | Benzersiz kimlik |
| Name | string | Firma adi |
| Description | string | Firma aciklamasi |
| Email | string | E-posta (unique) |
| Phone | string | Telefon |
| Address | string | Adres |
| Website | string | Web sitesi |
| LogoUrl | string | Logo URL |
| TaxNumber | string | Vergi numarasi (unique) |
| Tours | ICollection | Firmaya ait turlar |

### Tour (Tur)
Firmalar tarafindan sunulan turlar.

| Alan | Tip | Aciklama |
|------|-----|----------|
| Id | int | Benzersiz kimlik |
| Name | string | Tur adi |
| Description | string | Tur aciklamasi |
| Destination | string | Destinasyon |
| Price | decimal | Fiyat (TL) |
| DurationDays | int | Sure (gun) |
| MaxCapacity | int | Maksimum kapasite |
| ImageUrl | string | Resim URL |
| **IsFeatured** | **bool** | **Ana sayfada gosterilsin mi** |
| CompanyId | int | Firma ID (FK) |
| Company | Company | Firma (navigation) |
| Reservations | ICollection | Tura ait rezervasyonlar |

### Visitor (Ziyaretci)
Tur rezervasyonu yapan ziyaretciler.

| Alan | Tip | Aciklama |
|------|-----|----------|
| Id | int | Benzersiz kimlik |
| FirstName | string | Ad |
| LastName | string | Soyad |
| Email | string | E-posta (unique) |
| Phone | string | Telefon |
| IdentityNumber | string | TC Kimlik No |
| Reservations | ICollection | Ziyaretciye ait rezervasyonlar |

### Reservation (Rezervasyon)
Ziyaretcilerin turlara yaptigi rezervasyonlar.

| Alan | Tip | Aciklama |
|------|-----|----------|
| Id | int | Benzersiz kimlik |
| TourId | int | Tur ID (FK) |
| VisitorId | int | Ziyaretci ID (FK) |
| StartDate | DateTime | Baslangic tarihi |
| EndDate | DateTime | Bitis tarihi |
| NumberOfPeople | int | Kisi sayisi |
| TotalPrice | decimal | Toplam fiyat |
| Status | ReservationStatus | Durum |
| Notes | string | Notlar |
| Tour | Tour | Tur (navigation) |
| Visitor | Visitor | Ziyaretci (navigation) |

### ReservationStatus (Rezervasyon Durumu)
```csharp
public enum ReservationStatus
{
    Pending = 0,      // Beklemede
    Confirmed = 1,    // Onaylandi
    Cancelled = 2,    // Iptal
    Completed = 3     // Tamamlandi
}
```

---

## Entity Iliskileri

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Company   │ 1───N │    Tour     │ 1───N │ Reservation │
│   (Firma)   │       │   (Tur)     │       │(Rezervasyon)│
└─────────────┘       └─────────────┘       └──────┬──────┘
                                                   │
                                                   N
                                                   │
                                            ┌──────┴──────┐
                                            │   Visitor   │
                                            │ (Ziyaretci) │
                                            └─────────────┘

Iliski Aciklamasi:
- Bir Firma birden fazla Tur'a sahip olabilir (1:N)
- Bir Tur birden fazla Rezervasyon'a sahip olabilir (1:N)
- Bir Ziyaretci birden fazla Rezervasyon yapabilir (1:N)
```

---

## API Endpoint'leri

### Companies (Firmalar)
| Method | Endpoint | Aciklama |
|--------|----------|----------|
| GET | /api/companies | Tum firmalari listele |
| GET | /api/companies/{id} | Firma detayi |
| GET | /api/companies/{id}/tours | Firmaya ait turlar |
| POST | /api/companies | Yeni firma ekle |
| PUT | /api/companies/{id} | Firma guncelle |
| DELETE | /api/companies/{id} | Firma sil (soft delete) |

### Tours (Turlar)
| Method | Endpoint | Aciklama |
|--------|----------|----------|
| GET | /api/tours | Tum turlari listele |
| **GET** | **/api/tours/featured** | **One cikan turlari listele** |
| GET | /api/tours/{id} | Tur detayi |
| POST | /api/tours | Yeni tur ekle |
| PUT | /api/tours/{id} | Tur guncelle |
| DELETE | /api/tours/{id} | Tur sil (soft delete) |

### Visitors (Ziyaretciler)
| Method | Endpoint | Aciklama |
|--------|----------|----------|
| GET | /api/visitors | Tum ziyaretcileri listele |
| GET | /api/visitors/{id} | Ziyaretci detayi |
| POST | /api/visitors | Yeni ziyaretci ekle |
| PUT | /api/visitors/{id} | Ziyaretci guncelle |

### Reservations (Rezervasyonlar)
| Method | Endpoint | Aciklama |
|--------|----------|----------|
| GET | /api/reservations | Tum rezervasyonlari listele |
| GET | /api/reservations/{id} | Rezervasyon detayi |
| POST | /api/reservations | Yeni rezervasyon ekle |
| PUT | /api/reservations/{id} | Rezervasyon guncelle |
| PATCH | /api/reservations/{id}/status | Durum guncelle |

---

## KnockoutJS SPA Mimarisi

Web uygulamasi Single Page Application (SPA) yaklasimi ile gelistirilmistir. Her sayfa icin ayri bir KnockoutJS ViewModel kullanilir.

### Binding Yapisi
Her sayfanin icerigini bir container div ile sarmalayin ve `ko.applyBindings` fonksiyonuna bu div'i ikinci parametre olarak verin:

```html
<div id="companiesApp">
    <!-- Sayfa icerigi -->
</div>

@section Scripts {
<script>
function CompaniesViewModel() {
    var self = this;
    // ViewModel kodu...
}

// Binding'i sadece ilgili div'e uygula
ko.applyBindings(new CompaniesViewModel(), document.getElementById('companiesApp'));
</script>
}
```

### ViewModel Yapisi
Her ViewModel su temel ogeleri icerir:

```javascript
function ExampleViewModel() {
    var self = this;

    // Observable veriler
    self.items = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.selectedItem = ko.observable(null);
    self.formData = ko.observable({ /* form alanlari */ });

    // Modal referanslari
    var formModal, detailsModal, deleteModal;

    // CRUD islemleri
    self.loadData = function() { /* API'den veri yukle */ };
    self.openCreateModal = function() { /* Yeni kayit formu */ };
    self.openEditModal = function(item) { /* Duzenleme formu */ };
    self.showDetails = function(item) { /* Detay modali */ };
    self.openDeleteModal = function(item) { /* Silme onay modali */ };
    self.saveItem = function() { /* POST/PUT */ };
    self.deleteItem = function() { /* DELETE */ };

    // Baslangic
    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        detailsModal = new bootstrap.Modal(document.getElementById('detailsModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        self.loadData();
    });
}
```

### Sayfa Yapilari

| Sayfa | Container ID | ViewModel |
|-------|--------------|-----------|
| Ana Sayfa | homeApp | HomeViewModel |
| Firmalar | companiesApp | CompaniesViewModel |
| Turlar | toursApp | ToursViewModel |
| Ziyaretciler | visitorsApp | VisitorsViewModel |
| Rezervasyonlar | reservationsApp | ReservationsViewModel |

---

## Ana Sayfa Ozellikleri

### Ust Reklam Bandi
- Gradient animasyonlu reklam alani
- Rastgele one cikan tur gosterimi
- Tiklayinca turlar sayfasina yonlendirme

### Featured Turlar Carousel
- Bootstrap carousel ile gosterim
- Sadece `IsFeatured = true` olan turlar
- Resim, fiyat, destinasyon bilgileri

### Istatistik Kartlari
- Aktif tur sayisi
- Firma sayisi
- Ziyaretci sayisi

---

## Mimari Yapi

```
┌─────────────────────────────────────────────────────────────┐
│                        KULLANICI                            │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
        ┌─────────┐     ┌─────────┐     ┌─────────┐
        │   Web   │     │ Mobile  │     │ Diger   │
        │(KO SPA) │     │ (MAUI)  │     │ Client  │
        └────┬────┘     └────┬────┘     └────┬────┘
             │               │               │
             └───────────────┼───────────────┘
                             │
                             ▼ HTTP/REST
                    ┌─────────────────┐
                    │      API        │
                    │  (Web API)      │
                    │ + Auto-Migration│
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │      Data       │
                    │  (EF Core)      │
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │   PostgreSQL    │
                    │   Database      │
                    └─────────────────┘
```

---

## Yapilandirma

### API (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ErkanTatilPlaniDB;Username=postgres;Password=***"
  }
}
```

### Web (appsettings.json)
```json
{
  "ApiBaseUrl": "https://localhost:7001"
}
```

### Client-Side Kutuphaneler (libman.json)
```json
{
  "version": "1.0",
  "defaultProvider": "cdnjs",
  "libraries": [
    { "library": "twitter-bootstrap@5.3.2", "destination": "wwwroot/lib/bootstrap/" },
    { "library": "knockout@3.5.1", "destination": "wwwroot/lib/knockout/" },
    { "library": "bootstrap-icons@1.11.1", "destination": "wwwroot/lib/bootstrap-icons/" },
    { "library": "jquery@3.7.1", "destination": "wwwroot/lib/jquery/" },
    { "library": "toastr.js@2.1.4", "destination": "wwwroot/lib/toastr/" }
  ]
}
```

---

## Otomatik Migration

API projesi baslatildiginda otomatik olarak bekleyen migration'lar uygulanir:

```csharp
// Program.cs (API)
var app = builder.Build();

// Auto-migration on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}
```

---

## Calistirma

### Veritabani Migration (Manuel)
```bash
cd src/ErkanTatilPlani.Data
dotnet ef migrations add MigrationName --startup-project ../ErkanTatilPlani.API
```

### API Baslatma
```bash
cd src/ErkanTatilPlani.API
dotnet run
# https://localhost:7001
```

### Web Baslatma
```bash
cd src/ErkanTatilPlani.Web
dotnet run
# https://localhost:5001
```

### Client Kutuphanelerini Yukleme
```bash
cd src/ErkanTatilPlani.Web
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
libman restore
```

---

## Notlar

- Web projesi API uzerinden veri alir (jQuery AJAX)
- Controller'larda SQL kodu yoktur
- Silme islemleri soft delete olarak yapilir (IsActive = false)
- Tum entity'ler BaseEntity'den turetilir
- Her sayfa icin ayri KO container div kullanilir (binding cakismasini onler)
- Featured turlar admin tarafindan `IsFeatured` alani ile belirlenir
- Publish'te otomatik migration calisir

---

*Son Guncelleme: Ocak 2026*
