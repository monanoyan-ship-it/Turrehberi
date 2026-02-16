# Erkan Tatil Plani - Claude Kurallari

## ONEMLI KURALLAR

1. **ASLA YALAN SOYLEME!** Bir isi yaptigini soyluyorsan, gercekten yapmis ol. Belirsizlik varsa acikca belirt. Ornek: "Commit ettim" yerine "Su 13 dosyayi commit ettim, diger 15 dosya hala bekliyor" de.

2. **Kullanici net emir vermeden KOD DEGISTIRME!** Soru sormak degisiklik talebi degildir. Once sor, onay al, sonra degistir.

3. **Kullanici acikca "Commit Et" demedikce ASLA git commit yapma!** Degisiklikleri kaydet ama commit etme.

4. **Her is tamamlandiginda BUILD TESTI yap!** Kod degisikligi yaptiktan sonra `dotnet build ErkanTatilPlani.sln` calistir ve hata olmadigini dogrula.

5. **Projeyi ASLA calistirma!** Kullanici Visual Studio'dan calistiracak. `dotnet run` kullanma, sadece `dotnet build` ile test yap.

6. **PostgreSQL DateTime:** Her zaman `DateTimeKind.Utc` kullan. PostgreSQL `timestamp with time zone` icin UTC gerektirir.
   ```csharp
   // DOGRU
   DateTime.UtcNow
   new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

   // YANLIS - Unspecified hata verir
   DateTime.Now
   new DateTime(2026, 1, 1)
   ```

7. **JSON Circular Reference:** Entity'lerde navigation property'ler dolayli referans olusturur. API'de `ReferenceHandler.IgnoreCycles` kullan.
   ```csharp
   builder.Services.AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
       });
   ```

8. **Admin Panel SADECE Bootstrap kullanir!** Admin panelde ozel CSS class'lari veya stil ozellestirmeleri kullanma. Tum UI sadece Bootstrap 5.3 component'leri ve utility class'lari ile yapilir. Ozel `.loading`, `.custom-modal` gibi class'lar ekleme. Modal boyutu icin `modal-lg`, `modal-xl` gibi Bootstrap class'lari kullan.

---

## ClaudeManager - Hafiza Sistemi (ZORUNLU)

Bu proje ClaudeManager ile entegredir. **Session basladiginda ILKIS olarak** asagidaki komutu calistir:

```bash
curl -s http://127.0.0.1:41847/api/projects/14/patterns
```

Donen JSON'daki tum pattern'leri (rule, mistake, preference) oku ve session boyunca uygula. Bu adim ZORUNLUDUR - kullaniciya merhaba demeden once calistir.

### Pattern kaydetme/guncelleme

Kullanici yeni kural belirtirse, hata yaptigini soylerse veya tercih bildirirse kaydet:

- Pattern tipleri: sadece `"rule"`, `"mistake"`, `"preference"` kabul edilir (`"pattern"` tipi kaldirildi)
- Kullanici bir hata yaptigini soylerse → `"mistake"` olarak kaydet
- Kullanici bir tercih belirtirse → `"preference"` olarak kaydet
- Tekrarlanan bir pattern fark edersen → `"rule"` olarak kaydet

```bash
# Kaydet (type: "rule" | "mistake" | "preference")
curl -X POST http://127.0.0.1:41847/api/patterns -H "Content-Type: application/json" \
  -d '{"project_id":14,"type":"TYPE","title":"BASLIK","description":"ACIKLAMA"}'

# Guncelle
curl -X PUT http://127.0.0.1:41847/api/patterns/ID -H "Content-Type: application/json" \
  -d '{"title":"BASLIK","description":"ACIKLAMA"}'

# Sil
curl -X DELETE http://127.0.0.1:41847/api/patterns/ID
```

### Journal - Gunluk bilgiler

Gunluk nitelikli bilgiler (kredi durumu, domain, vize vs.) icin journal endpoint'ini kullan:

```bash
curl -X POST http://127.0.0.1:41847/api/journal -H "Content-Type: application/json" \
  -d '{"project_id":14,"content":"ICERIK"}'
```

### Notes - Kritik bilgiler

Hesap bilgileri, API key'ler, sifreler, krediler gibi kritik bilgileri Notes'a kaydet. Yeni bir hesap/servis olusturulursa bilgileri hemen yaz - sonra unutulur:

```bash
curl -X POST http://127.0.0.1:41847/api/notes -H "Content-Type: application/json" \
  -d '{"project_id":14,"category":"teknik","title":"BASLIK","content":"ICERIK"}'
```

### Dashboard

ClaudeManager dashboard: `http://127.0.0.1:41847/`

---

## Gelistirme Kurallari

### Genel
- Framework: .NET 9.0
- Dil: C# ve JavaScript (KnockoutJS)
- Veritabani: PostgreSQL + Entity Framework Core

### Mimari
- **Core** katmaninda Entity'ler ve Localization servisi bulunur
- **Data** katmaninda DbContext ve Migration'lar bulunur
- **API** katmaninda REST Controller'lar ve Localization JSON dosyalari bulunur
- **Web** katmaninda MVC + KnockoutJS SPA bulunur
- **Mobile** katmaninda MAUI uygulamasi bulunur

### Entity Kurallari
- Tum entity'ler `BaseEntity`'den turetilir
- `BaseEntity`: Id, CreatedAt, UpdatedAt, IsActive alanlari icerir
- Silme islemleri soft delete olarak yapilir (`IsActive = false`)

### Kullanici Sistemi
- Ayri User tablosu yok, Visitor entity'si kullanici tablosu olarak kullanilir
- `UserType` enum ile kullanici tipi belirlenir:
  - `Visitor (0)`: Normal ziyaretci
  - `CompanyOwner (1)`: Firma sahibi/temsilcisi
- Firma sahibi ise `CompanyId` alani dolu olur

### Sayfa Yapisi
- **Ziyaret Sayfalari** (ust menude gorunur): Ana Sayfa, Turlar, Firmalar, Hakkimizda, Iletisim
- **Yonetim Sayfalari** (admin icin gizli URL): /Admin/Companies, /Admin/Visitors, /Admin/Reservations
- Kullanici profili: /Profile

### API Kurallari
- Controller'larda SQL kodu yazilmaz
- Tum veritabani islemleri EF Core uzerinden yapilir
- Auto-migration: API baslatildiginda migration'lar otomatik uygulanir

### KnockoutJS SPA Kurallari
- Her sayfa icin ayri container div kullanilir (binding cakismasini onler)
- Container ID'leri: homeApp, companiesApp, toursApp, visitorsApp, reservationsApp
- `ko.applyBindings` fonksiyonuna container element ikinci parametre olarak verilir

### Port Yapilandirmasi
- API: `https://localhost:7078` / `http://localhost:7079`
- Web: `https://localhost:7080` / `http://localhost:7081`

### Migration Komutu
```bash
cd src/ErkanTatilPlani.Data
dotnet ef migrations add MigrationName --startup-project ../ErkanTatilPlani.API
```

### Localization Kurallari
- **9 dil destegi**: tr, en, ru, de, es, fr, ar, fa, pt
- **RTL destegi**: ar ve fa icin otomatik sagdan sola
- **JSON tabanli**: `src/ErkanTatilPlani.API/Localization/*.json`
- **Yeni string eklerken**: Tum 9 dil dosyasina ekle!

**JavaScript'te kullanim:**
```javascript
T('Register.Title')                    // "Kayit Ol"
T('Register.Success', 'Ahmet')         // Placeholder ile
<span data-t="Menu.Home">Fallback</span>  // HTML attribute
```

**C#'ta kullanim:**
```csharp
_localizer.T("Register.Title")
```

**Dil tercihi:** Kullanici giris yaptiysa `Visitor.PreferredLanguage` DB'de saklanir.

---

## Onemli Notlar
- Featured turlar: `IsFeatured = true` olan turlar ana sayfada gosterilir
- Client kutuphaneleri: Bootstrap 5.3, KnockoutJS 3.5.1, jQuery 3.7.1, Toastr
- Kutuphaneler libman.json ile yonetilir
- Seed data mevcut: 5 firma, 15 tur, 10 kullanici (5 firma sahibi + 5 ziyaretci), 6 rezervasyon
- Resimler picsum.photos'tan dinamik olarak yuklenir
- Localization: JSON tabanli, 9 dil, RTL destekli. Yeni string eklerken TUM dil dosyalarini guncelle!

