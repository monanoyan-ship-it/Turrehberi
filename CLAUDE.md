# Erkan Tatil Plani - Claude Kurallari

## ZORUNLU: ClaudeManager Entegrasyonu

Bu projenin TUM kurallari, tercihleri ve hata gecmisi **ClaudeManager**'da tutulur. CLAUDE.md her okunduğunda ClaudeManager patterns'lari da okunmalidir.

**Session basladiginda ILKIS olarak** asagidaki komutu calistir:

```bash
curl -s "http://127.0.0.1:41847/api/guide?cwd=c:/Users/Ahmet/source/repos/monanoyan-ship-it/Turrehberi"
```

Donen ciktidaki tum kurallari, hatalari ve tercihleri oku ve session boyunca uygula. Bu adim ZORUNLUDUR.

### Pattern kaydetme/guncelleme

Kullanici yeni kural belirtirse, hata yaptigini soylerse veya tercih bildirirse kaydet:

- Pattern tipleri: sadece `"rule"`, `"mistake"`, `"preference"` kabul edilir
- Kullanici bir hata yaptigini soylerse → `"mistake"` olarak kaydet
- Kullanici bir tercih belirtirse → `"preference"` olarak kaydet
- Tekrarlanan bir pattern fark edersen → `"rule"` olarak kaydet

```bash
# Kaydet
curl -X POST http://127.0.0.1:41847/api/patterns -H "Content-Type: application/json" \
  -d '{"project_id":14,"type":"TYPE","title":"BASLIK","description":"ACIKLAMA"}'

# Guncelle
curl -X PUT http://127.0.0.1:41847/api/patterns/ID -H "Content-Type: application/json" \
  -d '{"title":"BASLIK","description":"ACIKLAMA"}'

# Sil
curl -X DELETE http://127.0.0.1:41847/api/patterns/ID
```

### Journal - Gunluk bilgiler

```bash
# Oku
curl -s http://127.0.0.1:41847/api/projects/14/journal

# Yaz
curl -X POST http://127.0.0.1:41847/api/projects/14/journal -H "Content-Type: application/json" \
  -d '{"title":"BASLIK","content":"ICERIK","category":"genel|teknik|karar|arastirma"}'
```

### Notes - Kritik bilgiler (hesap, API key, sifre, kredi)

```bash
# Oku
curl -s http://127.0.0.1:41847/api/projects/14/notes

# Yaz
curl -X POST http://127.0.0.1:41847/api/projects/14/notes -H "Content-Type: application/json" \
  -d '{"category":"teknik","title":"BASLIK","content":"ICERIK"}'
```

### Roadmap (Fazlar ve Gorevler)

```bash
# Ozet (ONCE BUNU KULLAN - faz basliklari + gorev listesi)
curl -s http://127.0.0.1:41847/api/projects/14/roadmap/summary

# Tam detay (detail + risks dahil, sadece gerektiginde)
curl -s http://127.0.0.1:41847/api/projects/14/roadmap

# Gorev ekle
curl -X POST http://127.0.0.1:41847/api/phases/FAZ_ID/tasks -H "Content-Type: application/json" \
  -d '{"task_no":"X.Y","title":"BASLIK","detail":"DETAY"}'

# Gorevi tamamlandi olarak isaretle
curl -X PUT http://127.0.0.1:41847/api/tasks/GOREV_ID -H "Content-Type: application/json" \
  -d '{"status":"completed"}'
```

### Arama

```bash
# Gecmis prompt'lari ara
curl -s "http://127.0.0.1:41847/api/search?q=ARAMA_TERIMI&project=14"

# Analitik veriler
curl -s http://127.0.0.1:41847/api/projects/14/analytics
```

### Dashboard

ClaudeManager dashboard: `http://127.0.0.1:41847/`
