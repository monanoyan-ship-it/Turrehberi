# Kod ve Mimari Pattern'leri

Bu dosya projede kullanilan kod pattern'lerini ve kurallari icerir.

---

## JavaScript Dosya Ayrimi

**CSHTML dosyalarinda inline JavaScript YAZILMAZ!**

### Kural
- Tum JavaScript kodu ayri `.js` dosyalarinda olmali
- JS dosyalari `wwwroot/js/{KlasorAdi}/{SayfaAdi}.js` konumunda olmali
- CSHTML'de sadece script referansi olmali

### Dosya Konumlari

| View Dosyasi | JS Dosyasi |
|--------------|------------|
| `Views/Admin/EmailTemplates.cshtml` | `wwwroot/js/Admin/EmailTemplates.js` |
| `Views/Admin/Companies.cshtml` | `wwwroot/js/Admin/Companies.js` |
| `Views/Home/Index.cshtml` | `wwwroot/js/Home/Index.js` |

### CSHTML Icerigi

```html
@section Scripts {
<script src="~/js/Admin/SayfaAdi.js"></script>
}
```

### JS Dosyasi Icerigi

```javascript
function SayfaAdiViewModel() {
    var self = this;

    // Observables
    self.data = ko.observableArray([]);
    self.isLoading = ko.observable(true);

    // Fonksiyonlar
    self.loadData = function() {
        // ...
    };

    // Init
    self.loadData();
}

ko.applyBindings(new SayfaAdiViewModel(), document.getElementById('sayfaAdiApp'));
```

---

## KnockoutJS ViewModel Pattern

### Container ID Kurali
- Her sayfa icin benzersiz container ID kullan
- ID formati: `{sayfaAdi}App` (ornek: `emailTemplatesApp`, `companiesApp`)

### ViewModel Yapisi
```javascript
function XyzViewModel() {
    var self = this;

    // 1. Observables (veri)
    self.items = ko.observableArray([]);
    self.selectedItem = ko.observable(null);
    self.isLoading = ko.observable(false);
    self.isSaving = ko.observable(false);

    // 2. Computed (hesaplanan)
    self.filteredItems = ko.computed(function() {
        // ...
    });

    // 3. Helper fonksiyonlar
    self.formatDate = function(dateStr) {
        // ...
    };

    // 4. CRUD fonksiyonlar
    self.loadData = function() { };
    self.saveItem = function() { };
    self.deleteItem = function() { };

    // 5. Modal fonksiyonlar
    self.openCreateModal = function() { };
    self.openEditModal = function(item) { };

    // 6. Init - sayfa yuklendiginde
    $(document).ready(function() {
        // Modal'lari baslat
        // Veriyi yukle
        self.loadData();
    });
}

ko.applyBindings(new XyzViewModel(), document.getElementById('xyzApp'));
```

---

## Bootstrap Modal Kullanimi

### Modal Referansi
```javascript
var formModal, deleteModal;

$(document).ready(function() {
    formModal = new bootstrap.Modal(document.getElementById('formModal'));
    deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
});
```

### Modal Acma/Kapama
```javascript
formModal.show();  // Ac
formModal.hide();  // Kapat
```

---

## API Cagrilari

### GET - Veri Cekme
```javascript
$.ajax({
    url: apiBaseUrl + '/api/xyz',
    method: 'GET'
})
.done(function(data) {
    self.items(data);
})
.fail(function(xhr) {
    toastr.error('Veriler yuklenemedi');
});
```

### POST/PUT - Kaydetme
```javascript
$.ajax({
    url: apiBaseUrl + '/api/xyz' + (isEdit ? '/' + id : ''),
    method: isEdit ? 'PUT' : 'POST',
    contentType: 'application/json',
    data: JSON.stringify(data)
})
.done(function() {
    toastr.success('Kaydedildi');
    self.loadData();
})
.fail(function(xhr) {
    var msg = xhr.responseJSON?.message || 'Hata olustu';
    toastr.error(msg);
});
```

### DELETE - Silme
```javascript
$.ajax({
    url: apiBaseUrl + '/api/xyz/' + id,
    method: 'DELETE'
})
.done(function() {
    toastr.success('Silindi');
    self.loadData();
});
```

---

*Son Guncelleme: Ocak 2026*
