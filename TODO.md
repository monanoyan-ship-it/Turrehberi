# Erkan Tatil Plani - Yapilacaklar

## Oncelikli (Yuksek)

### Firma Onay Is Akisi
- [ ] Admin panelinde firma basvurularini listeleyen sayfa
- [ ] Firma onaylama/reddetme UI
- [ ] Firma sahiplerine onay durumu bildirimi (toast mesajlari)
- [ ] Onay bekleyen firmalar icin kisitlamalar (tur ekleyememe)
- [ ] Sozlesme yukleme alani ve yonetimi

### Tur Yorum Sistemi
- [ ] ReviewsController API endpoint'leri olustur
  - [ ] GET /api/tours/{id}/reviews - Tur yorumlarini listele
  - [ ] POST /api/tours/{id}/reviews - Yeni yorum ekle
  - [ ] PUT /api/reviews/{id} - Yorum guncelle
  - [ ] DELETE /api/reviews/{id} - Yorum sil
  - [ ] POST /api/reviews/{id}/helpful - Yardimci oyla
  - [ ] POST /api/reviews/{id}/report - Sikayet et
  - [ ] POST /api/reviews/{id}/reply - Yanit ekle
- [ ] Tur detay sayfasinda yorum listesi KnockoutJS komponenti
- [ ] Yorum yazma formu (puan secimi, pros/cons, fotograf yukleme)
- [ ] Yorum filtreleme ve siralama
- [ ] Firma yaniti ozelligi
- [ ] Yorum moderasyon paneli (Admin)

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
