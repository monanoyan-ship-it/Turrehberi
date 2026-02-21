# Erkan Tatil Plani - Claude Kurallari

## ZORUNLU: ClaudeManager Entegrasyonu

Bu projenin TUM kurallari, tercihleri ve hata gecmisi **ClaudeManager**'da tutulur. CLAUDE.md her okunduğunda ClaudeManager patterns'lari da okunmalidir.

**Session basladiginda ILKIS olarak** asagidaki komutu calistir:

```bash
curl -s http://127.0.0.1:41847/api/projects/14/patterns
```

Donen JSON'daki tum pattern'leri (rule, mistake, preference) oku ve session boyunca uygula. Bu adim ZORUNLUDUR.

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
curl -X POST http://127.0.0.1:41847/api/projects/14/journal -H "Content-Type: application/json" \
  -d '{"title":"BASLIK","content":"ICERIK"}'
```

### Notes - Kritik bilgiler (hesap, API key, sifre, kredi)

```bash
curl -X POST http://127.0.0.1:41847/api/projects/14/notes -H "Content-Type: application/json" \
  -d '{"category":"teknik","title":"BASLIK","content":"ICERIK"}'
```

### Roadmap (Fazlar ve Gorevler)

```bash
# Tum fazlari ve gorevleri gor
curl -s http://127.0.0.1:41847/api/projects/14/roadmap

# Gorevi tamamlandi olarak isaretle
curl -X PUT http://127.0.0.1:41847/api/tasks/TASK_ID -H "Content-Type: application/json" \
  -d '{"status":"completed"}'
```

### Dashboard

ClaudeManager dashboard: `http://127.0.0.1:41847/`
