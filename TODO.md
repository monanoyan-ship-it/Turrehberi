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
- [ ] Rezervasyon onay/red email bildirimleri
- [ ] Rezervasyon detay sayfasi
- [ ] Odeme entegrasyonu (Stripe/Iyzico)

---

## Orta Oncelik

### Kullanici Deneyimi
- [ ] Profil sayfasi gelistirmeleri
- [ ] Sifre degistirme
- [ ] Sifremi unuttum fonksiyonu
- [ ] Email dogrulama sistemi
- [ ] Kullanici avatar yukleme

### Firma Ozellikleri
- [ ] Firma dashboard'u (istatistikler)
- [ ] Tur yonetim sayfasi (CRUD)
- [ ] Rezervasyon yonetimi
- [ ] Yorumlara yanit verme arayuzu

### Arama ve Filtreleme
- [ ] Gelismis tur arama (tarih, fiyat, destinasyon)
- [ ] Firma arama
- [ ] Harita entegrasyonu (turlar haritada)

---

## Dusuk Oncelik

### SEO ve Performans
- [ ] Meta tag'ler ve Open Graph
- [ ] Sitemap.xml
- [ ] Resim optimizasyonu (lazy loading)
- [ ] Cache mekanizmasi

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
- [ ] Favori turlar ozelligi

---

## Teknik Borc

- [ ] Unit test'ler yaz
- [ ] Integration test'ler yaz
- [ ] API dokumantasyonu (Swagger iyilestirmeleri)
- [ ] Loglama sistemi (Serilog)
- [ ] Exception handling middleware
- [ ] Rate limiting

---

*Son Guncelleme: Ocak 2026*
