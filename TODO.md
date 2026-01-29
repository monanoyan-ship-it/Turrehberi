# Erkan Tatil Plani - Yapilacaklar

## Kontrol Edilecek

### Cift Startup Projesi (API + Web)
- [ ] Visual Studio'yu yeniden ac ve toolbar'daki startup dropdown'da **"API + Web"** profilini kontrol et
- [ ] `ErkanTatilPlani.slnLaunch` dosyasi solution dizininde mevcut
- [ ] Profil gorunuyorsa sec ve F5 ile calistir (API: 7078, Web: 7080)
- [ ] **Profil gorunmuyorsa elle ayarla:**
  1. Solution Explorer'da **Solution** satirina sag tikla
  2. **"Configure Startup Projects..."** sec
  3. **"Multiple startup projects"** secenegini isaretle
  4. `ErkanTatilPlani.API` → Action: **Start**
  5. `ErkanTatilPlani.Web` → Action: **Start**
  6. Diger projeler (Core, Data) → Action: **None**
  7. **OK**'a bas
  8. Not: Bu ozellik VS 2022 17.11+ gerektirir. Eski surumde Preview Features'tan "Enable Multi-Project Launch Profiles" acilmali

---

## Oncelikli (Yuksek)

### Firma Onay Is Akisi
- [x] Admin panelinde firma basvurularini listeleyen sayfa
- [x] Firma onaylama/reddetme UI
- [x] Firma sahiplerine onay durumu bildirimi (toast mesajlari)
- [x] Onay bekleyen firmalar icin kisitlamalar (tur ekleyememe)
- [x] Sozlesme yukleme alani ve yonetimi

### Tur Yorum Sistemi
- [x] ReviewsController API endpoint'leri olustur
  - [x] GET /api/tours/{id}/reviews - Tur yorumlarini listele
  - [x] POST /api/tours/{id}/reviews - Yeni yorum ekle
  - [x] PUT /api/reviews/{id} - Yorum guncelle
  - [x] DELETE /api/reviews/{id} - Yorum sil
  - [x] POST /api/reviews/{id}/helpful - Yardimci oyla
  - [x] POST /api/reviews/{id}/report - Sikayet et
  - [x] POST /api/reviews/{id}/reply - Yanit ekle
- [x] Tur detay sayfasinda yorum listesi KnockoutJS komponenti
- [x] Yorum yazma formu (puan secimi, pros/cons)
- [x] Yorum filtreleme ve siralama
- [x] Firma yaniti ozelligi
- [ ] Fotograf yukleme (sonraki asamada)
- [x] Yorum moderasyon paneli (Admin)

### Firma Profil Sayfasi (SEO + Web Sitesi)
- [x] Public firma profil sayfasi (/Firmalar/{slug} veya /Companies/{id})
- [x] SEO meta tag'leri (title, description, Open Graph, Twitter Card)
- [x] Firma hakkinda tam bilgi (logo, aciklama, iletisim, adres)
- [x] Firmanin turlari listesi
- [x] Firmanin yorumlari ve ortalama puani
- [ ] Galeri/Fotograf bolumu (sonraki asamada)
- [ ] Iletisim formu (sonraki asamada)
- [x] Schema.org yapilandirmasi (TravelAgency, AggregateRating)
- [x] Google indekslenebilir URL yapisi (slug tabanli)

### Rezervasyon Iyilestirmeleri
- [x] Rezervasyon onay/red email bildirimleri
- [x] Rezervasyon detay sayfasi
- [x] Odeme entegrasyonu (Iyzico)

---

## Orta Oncelik

### Kullanici Deneyimi
- [x] Profil sayfasi gelistirmeleri
- [x] Sifre degistirme
- [x] Sifremi unuttum fonksiyonu
- [x] Email dogrulama sistemi
- [x] Kullanici avatar yukleme

### Firma Ozellikleri
- [x] Firma dashboard'u (istatistikler)
- [x] Tur yonetim sayfasi (CRUD)
- [x] Rezervasyon yonetimi
- [x] Yorumlara yanit verme arayuzu

### Arama ve Filtreleme
- [x] Gelismis tur arama (tarih, fiyat, destinasyon)
- [x] Firma arama
- [x] Harita entegrasyonu (turlar haritada)

---

## Dusuk Oncelik

### SEO ve Performans
- [x] Meta tag'ler ve Open Graph
- [x] Sitemap.xml
- [x] robots.txt
- [x] Resim optimizasyonu (lazy loading)
- [x] Cache mekanizmasi

### Mobil Uygulama (MAUI)
- [ ] Login/Register ekranlari
- [ ] Tur listesi ve detay
- [ ] Rezervasyon yapma
- [ ] Push notifications

### Diger
- [ ] Blog/Haber modulu
- [ ] SSS (Sikca Sorulan Sorular)
- [ ] Canli destek entegrasyonu
- [ ] Sosyal medya paylasimi
- [x] Favori turlar ozelligi

---

## Teknik Borc

### 1. Exception Handling Middleware
- [x] Global exception handler middleware olustur
- [x] Development'ta detayli hata, Production'da genel mesaj
- [x] Hata loglama entegrasyonu
- [x] ProblemDetails formatinda JSON response

### 2. Loglama Sistemi (Veritabani)
- [x] AppLog entity ve DbSet
- [x] IAppLogService interface
- [x] AppLogService implementasyonu
- [x] LogsController (Admin - log goruntuleme)
- [x] Exception middleware veritabani entegrasyonu
- [x] Log temizleme (cleanup) endpoint

### 3. Rate Limiting
- [x] .NET built-in Rate Limiting middleware
- [x] Global limit: 100 request/dakika (IP bazli)
- [x] Auth endpoint'leri icin ozel limit: 10 request/dakika
- [x] Sensitive endpoint'ler icin limit: 5 request/dakika
- [x] 429 Too Many Requests response

### 4. Swagger Iyilestirmeleri
- [x] XML dokumantasyon yorumlari
- [x] JWT Bearer authentication destegi
- [x] Swagger UI (/swagger)
- [x] API bilgileri (title, description, contact)
- [ ] Endpoint gruplama (sonraki asamada)

### 5. Unit Test'ler
- [x] ErkanTatilPlani.Tests projesi olustur (xUnit)
- [x] Moq ile mock'lama
- [x] CacheService testleri (10 test)
- [x] Entity testleri (11 test)
- [ ] Daha fazla test eklenebilir

### 6. Integration Test'ler
- [ ] ErkanTatilPlani.IntegrationTests projesi olustur (sonraki asamada)
- [ ] WebApplicationFactory ile API testleri
- [ ] In-memory database
- [ ] Auth flow testleri

---

*Son Guncelleme: Ocak 2026*
