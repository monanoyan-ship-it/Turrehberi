function AdminCompaniesViewModel() {
    var self = this;

    // Veriler
    self.companies = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.selectedCompany = ko.observable(null);
    self.filterStatus = ko.observable(null);

    // Form verileri
    self.formData = ko.observable({
        id: 0, name: '', email: '', phone: '', address: '',
        website: '', taxNumber: '', logoUrl: '', description: '',
        isActive: true, statusId: 1
    });

    // Onay form verileri
    self.reviewNotes = ko.observable('');
    self.rejectionReason = ko.observable('');
    self.suspendReason = ko.observable('');
    self.reactivateNotes = ko.observable('');

    // Sozlesme upload verileri
    self.selectedFile = ko.observable(null);
    self.isUploading = ko.observable(false);
    self.uploadProgress = ko.observable(0);

    // Modal referanslari
    var formModal, deleteModal, approveModal, rejectModal, suspendModal, reactivateModal, detailsModal, contractModal;

    // Bekleyen sayisi
    self.pendingCount = ko.computed(function() {
        return self.companies().filter(function(c) { return c.statusId === 0; }).length;
    });

    // Filtrelenmis firmalar
    self.filteredCompanies = ko.computed(function() {
        var status = self.filterStatus();
        if (status === null) return self.companies();
        return self.companies().filter(function(c) { return c.statusId === status; });
    });

    // Durum badge'i
    self.getStatusBadge = function(statusId) {
        var statuses = {
            0: '<i class="bi bi-hourglass-split me-1"></i>Bekliyor',
            1: '<i class="bi bi-check-circle me-1"></i>Onaylandi',
            2: '<i class="bi bi-x-circle me-1"></i>Reddedildi',
            3: '<i class="bi bi-pause-circle me-1"></i>Askida'
        };
        return statuses[statusId] || 'Bilinmiyor';
    };

    self.getStatusBadgeClass = function(statusId) {
        var classes = {
            0: 'bg-warning text-dark',
            1: 'bg-success',
            2: 'bg-danger',
            3: 'bg-secondary'
        };
        return classes[statusId] || 'bg-secondary';
    };

    // Tarih formatlama
    self.formatDate = function(dateStr) {
        if (!dateStr) return '-';
        var date = new Date(dateStr);
        return date.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    };

    // Kisa tarih formatlama (sadece gun/ay)
    self.formatDateShort = function(dateStr) {
        if (!dateStr) return '';
        var date = new Date(dateStr);
        return date.toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit' });
    };

    // Veri yukleme
    self.loadData = function() {
        $.ajax({ url: apiBaseUrl + '/api/companies', method: 'GET' })
            .done(function(data) {
                self.companies(data);
                self.isLoading(false);
            })
            .fail(function() {
                toastr.error('Veriler yuklenemedi');
                self.isLoading(false);
            });
    };

    // Modal acma fonksiyonlari
    self.openCreateModal = function() {
        self.isEditing(false);
        self.formData({ id: 0, name: '', email: '', phone: '', address: '', website: '', taxNumber: '', logoUrl: '', description: '', isActive: true, statusId: 1 });
        formModal.show();
    };

    self.openEditModal = function(c) {
        self.isEditing(true);
        self.formData(Object.assign({}, c));
        formModal.show();
    };

    self.openDeleteModal = function(c) {
        self.selectedCompany(c);
        deleteModal.show();
    };

    self.openDetailsModal = function(c) {
        self.selectedCompany(c);
        detailsModal.show();
    };

    self.openApproveModal = function(c) {
        self.selectedCompany(c);
        self.reviewNotes('');
        approveModal.show();
    };

    self.openRejectModal = function(c) {
        self.selectedCompany(c);
        self.rejectionReason('');
        rejectModal.show();
    };

    self.openSuspendModal = function(c) {
        self.selectedCompany(c);
        self.suspendReason('');
        suspendModal.show();
    };

    self.openReactivateModal = function(c) {
        self.selectedCompany(c);
        self.reactivateNotes('');
        reactivateModal.show();
    };

    self.openContractModal = function(c) {
        self.selectedCompany(c);
        self.selectedFile(null);
        self.uploadProgress(0);
        // Dosya inputunu temizle
        var fileInput = document.getElementById('contractFileInput');
        if (fileInput) fileInput.value = '';
        contractModal.show();
    };

    // Dosya secildiginde
    self.onFileSelected = function(data, event) {
        var file = event.target.files[0];
        if (file) {
            // Dosya tipi kontrolu
            if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
                toastr.error('Sadece PDF dosyalari yuklenebilir');
                event.target.value = '';
                self.selectedFile(null);
                return;
            }
            // Boyut kontrolu (10MB)
            if (file.size > 10 * 1024 * 1024) {
                toastr.error('Dosya boyutu 10MB\'i gecemez');
                event.target.value = '';
                self.selectedFile(null);
                return;
            }
            self.selectedFile(file);
        } else {
            self.selectedFile(null);
        }
    };

    // Sozlesme yukle
    self.uploadContract = function() {
        if (!self.selectedFile()) {
            toastr.warning('Lutfen bir dosya secin');
            return;
        }

        var formData = new FormData();
        formData.append('file', self.selectedFile());

        self.isUploading(true);
        self.uploadProgress(0);

        $.ajax({
            url: apiBaseUrl + '/api/companies/' + self.selectedCompany().id + '/upload-contract',
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            xhr: function() {
                var xhr = new window.XMLHttpRequest();
                xhr.upload.addEventListener('progress', function(e) {
                    if (e.lengthComputable) {
                        var percent = Math.round((e.loaded / e.total) * 100);
                        self.uploadProgress(percent);
                    }
                }, false);
                return xhr;
            }
        })
        .done(function(response) {
            toastr.success(response.message || 'Sozlesme basariyla yuklendi');
            // Secili firmayi guncelle
            self.selectedCompany().contractFileUrl = response.contractFileUrl;
            self.selectedCompany().contractUploadedAt = response.contractUploadedAt;
            // Listeyi yenile
            self.loadData();
            // Formu temizle
            self.selectedFile(null);
            var fileInput = document.getElementById('contractFileInput');
            if (fileInput) fileInput.value = '';
            self.isUploading(false);
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Yukleme hatasi';
            toastr.error(msg);
            self.isUploading(false);
        });
    };

    // Sozlesme sil
    self.deleteContract = function() {
        if (!confirm('Sozlesmeyi silmek istediginizden emin misiniz?')) return;

        self.isUploading(true);
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + self.selectedCompany().id + '/contract',
            method: 'DELETE'
        })
        .done(function(response) {
            toastr.success(response.message || 'Sozlesme basariyla silindi');
            // Secili firmayi guncelle
            self.selectedCompany().contractFileUrl = '';
            self.selectedCompany().contractUploadedAt = null;
            // Listeyi yenile
            self.loadData();
            self.isUploading(false);
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Silme hatasi';
            toastr.error(msg);
            self.isUploading(false);
        });
    };

    // CRUD islemleri
    self.saveCompany = function() {
        self.isSaving(true);
        var data = self.formData();
        $.ajax({
            url: apiBaseUrl + '/api/companies' + (self.isEditing() ? '/' + data.id : ''),
            method: self.isEditing() ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data)
        })
        .done(function() {
            formModal.hide();
            self.loadData();
            toastr.success('Kaydedildi');
            self.isSaving(false);
        })
        .fail(function() {
            toastr.error('Hata olustu');
            self.isSaving(false);
        });
    };

    self.deleteCompany = function() {
        self.isSaving(true);
        $.ajax({ url: apiBaseUrl + '/api/companies/' + self.selectedCompany().id, method: 'DELETE' })
            .done(function() {
                deleteModal.hide();
                self.loadData();
                toastr.success('Silindi');
                self.isSaving(false);
            })
            .fail(function() {
                toastr.error('Hata olustu');
                self.isSaving(false);
            });
    };

    // Onay islemleri
    self.approveCompany = function() {
        self.isSaving(true);
        var userId = window.currentUser ? window.currentUser.id : null;
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + self.selectedCompany().id + '/approve',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ reviewedById: userId, reviewNotes: self.reviewNotes() })
        })
        .done(function(response) {
            approveModal.hide();
            self.loadData();
            toastr.success(response.message || 'Firma onaylandi');
            self.isSaving(false);
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu';
            toastr.error(msg);
            self.isSaving(false);
        });
    };

    self.rejectCompany = function() {
        if (!self.rejectionReason()) {
            toastr.warning('Red sebebi zorunludur');
            return;
        }
        self.isSaving(true);
        var userId = window.currentUser ? window.currentUser.id : null;
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + self.selectedCompany().id + '/reject',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ reviewedById: userId, rejectionReason: self.rejectionReason() })
        })
        .done(function(response) {
            rejectModal.hide();
            self.loadData();
            toastr.success(response.message || 'Firma reddedildi');
            self.isSaving(false);
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu';
            toastr.error(msg);
            self.isSaving(false);
        });
    };

    self.suspendCompany = function() {
        if (!self.suspendReason()) {
            toastr.warning('Askiya alma sebebi zorunludur');
            return;
        }
        self.isSaving(true);
        var userId = window.currentUser ? window.currentUser.id : null;
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + self.selectedCompany().id + '/suspend',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ reviewedById: userId, reason: self.suspendReason() })
        })
        .done(function(response) {
            suspendModal.hide();
            self.loadData();
            toastr.success(response.message || 'Firma askiya alindi');
            self.isSaving(false);
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu';
            toastr.error(msg);
            self.isSaving(false);
        });
    };

    self.reactivateCompany = function() {
        self.isSaving(true);
        var userId = window.currentUser ? window.currentUser.id : null;
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + self.selectedCompany().id + '/reactivate',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ reviewedById: userId, reviewNotes: self.reactivateNotes() })
        })
        .done(function(response) {
            reactivateModal.hide();
            self.loadData();
            toastr.success(response.message || 'Firma tekrar aktiflendi');
            self.isSaving(false);
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu';
            toastr.error(msg);
            self.isSaving(false);
        });
    };

    // Sayfa yuklendiginde
    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        approveModal = new bootstrap.Modal(document.getElementById('approveModal'));
        rejectModal = new bootstrap.Modal(document.getElementById('rejectModal'));
        suspendModal = new bootstrap.Modal(document.getElementById('suspendModal'));
        reactivateModal = new bootstrap.Modal(document.getElementById('reactivateModal'));
        detailsModal = new bootstrap.Modal(document.getElementById('detailsModal'));
        contractModal = new bootstrap.Modal(document.getElementById('contractModal'));
        self.loadData();
    });
}

ko.applyBindings(new AdminCompaniesViewModel(), document.getElementById('adminCompaniesApp'));
