function AdminReviewsViewModel() {
    var self = this;

    // Veriler
    self.reviews = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.selectedReview = ko.observable(null);
    self.filterStatus = ko.observable(null);
    self.searchTerm = ko.observable('');

    // Sayfalama
    self.currentPage = ko.observable(1);
    self.totalPages = ko.observable(1);
    self.totalCount = ko.observable(0);

    // Istatistikler
    self.stats = ko.observable({ pending: 0, approved: 0, rejected: 0, flagged: 0 });

    // Moderasyon form verileri
    self.moderationNote = ko.observable('');
    self.rejectionReason = ko.observable('');

    // Modal referanslari
    var approveModal, rejectModal, flagModal, detailsModal;

    // Durum badge'i
    self.getStatusBadge = function(statusId) {
        var statuses = {
            0: '<i class="bi bi-hourglass-split me-1"></i>Bekliyor',
            1: '<i class="bi bi-check-circle me-1"></i>Onaylandi',
            2: '<i class="bi bi-x-circle me-1"></i>Reddedildi',
            3: '<i class="bi bi-flag me-1"></i>Inceleniyor'
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

    // Filtre degistiginde
    self.filterStatus.subscribe(function() {
        self.currentPage(1);
        self.loadData();
    });

    // Veri yukleme
    self.loadData = function() {
        self.isLoading(true);
        var params = {
            page: self.currentPage(),
            pageSize: 20
        };
        if (self.filterStatus() !== null) {
            params.statusId = self.filterStatus();
        }
        if (self.searchTerm()) {
            params.search = self.searchTerm();
        }

        $.ajax({
            url: apiBaseUrl + '/api/admin/reviews',
            method: 'GET',
            data: params
        })
        .done(function(data) {
            self.reviews(data.reviews);
            self.totalCount(data.pagination.totalCount);
            self.totalPages(data.pagination.totalPages);
            self.stats(data.stats);
            self.isLoading(false);
        })
        .fail(function() {
            toastr.error('Veriler yuklenemedi');
            self.isLoading(false);
        });
    };

    // Modal acma fonksiyonlari
    self.openApproveModal = function(r) {
        self.selectedReview(r);
        self.moderationNote('');
        approveModal.show();
    };

    self.openRejectModal = function(r) {
        self.selectedReview(r);
        self.rejectionReason('');
        self.moderationNote('');
        rejectModal.show();
    };

    self.openFlagModal = function(r) {
        self.selectedReview(r);
        self.moderationNote('');
        flagModal.show();
    };

    self.openDetailsModal = function(r) {
        self.selectedReview(r);
        detailsModal.show();
    };

    // Moderasyon islemleri
    self.approveReview = function() {
        self.isSaving(true);
        var userId = window.currentUser ? window.currentUser.id : null;
        $.ajax({
            url: apiBaseUrl + '/api/reviews/' + self.selectedReview().id + '/approve',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ moderatedById: userId, note: self.moderationNote() })
        })
        .done(function(response) {
            approveModal.hide();
            self.loadData();
            toastr.success(response.message || 'Yorum onaylandi');
            self.isSaving(false);
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu';
            toastr.error(msg);
            self.isSaving(false);
        });
    };

    self.rejectReview = function() {
        if (!self.rejectionReason()) {
            toastr.warning('Red sebebi zorunludur');
            return;
        }
        self.isSaving(true);
        var userId = window.currentUser ? window.currentUser.id : null;
        $.ajax({
            url: apiBaseUrl + '/api/reviews/' + self.selectedReview().id + '/reject',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                moderatedById: userId,
                rejectionReason: self.rejectionReason(),
                note: self.moderationNote()
            })
        })
        .done(function(response) {
            rejectModal.hide();
            self.loadData();
            toastr.success(response.message || 'Yorum reddedildi');
            self.isSaving(false);
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu';
            toastr.error(msg);
            self.isSaving(false);
        });
    };

    self.flagReview = function() {
        self.isSaving(true);
        var userId = window.currentUser ? window.currentUser.id : null;
        $.ajax({
            url: apiBaseUrl + '/api/reviews/' + self.selectedReview().id + '/flag',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ moderatedById: userId, note: self.moderationNote() })
        })
        .done(function(response) {
            flagModal.hide();
            self.loadData();
            toastr.success(response.message || 'Yorum incelemeye alindi');
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
        approveModal = new bootstrap.Modal(document.getElementById('approveModal'));
        rejectModal = new bootstrap.Modal(document.getElementById('rejectModal'));
        flagModal = new bootstrap.Modal(document.getElementById('flagModal'));
        detailsModal = new bootstrap.Modal(document.getElementById('detailsModal'));
        self.loadData();
    });
}

ko.applyBindings(new AdminReviewsViewModel(), document.getElementById('adminReviewsApp'));
