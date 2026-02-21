function MyToursViewModel() {
    var self = this;

    // Observables
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isDeleting = ko.observable(false);
    self.tours = ko.observableArray([]);
    self.canManageTours = ko.observable(false);
    self.companyId = ko.observable(null);

    // Form
    self.isEditing = ko.observable(false);
    self.editingTourId = ko.observable(null);
    self.formData = ko.observable({
        name: '',
        description: '',
        destination: '',
        price: 0,
        durationDays: 1,
        maxCapacity: 20,
        imageUrl: '',
        isFeatured: false,
        difficultyId: '1',
        categoryId: '0',
        guideLanguages: '',
        inclusions: '',
        exclusions: '',
        meetingPointLat: '',
        meetingPointLng: '',
        meetingPointAddress: ''
    });

    // Delete
    self.deletingTour = ko.observable(null);

    // Date management
    self.managingTourId = ko.observable(null);
    self.managingTourName = ko.observable('');
    self.managedDates = ko.observableArray([]);
    self.isLoadingDates = ko.observable(false);
    self.isSavingDate = ko.observable(false);
    self.dateFormData = ko.observable({
        startDate: '',
        endDate: '',
        price: '',
        maxCapacity: ''
    });

    // Modals
    var tourModal = null;
    var deleteModal = null;
    var dateManageModal = null;

    // Helper functions
    self.formatCurrency = function(value) {
        if (!value) return '0 TL';
        return new Intl.NumberFormat('tr-TR', { style: 'decimal' }).format(value) + ' TL';
    };

    // Capacity helpers
    self.capacityPercent = function(dateItem) {
        if (!dateItem.maxCapacity || dateItem.maxCapacity === 0) return 0;
        return Math.round((dateItem.bookedCount / dateItem.maxCapacity) * 100);
    };

    self.capacityColor = function(dateItem) {
        var pct = self.capacityPercent(dateItem);
        if (pct >= 80) return 'bg-danger';
        if (pct >= 50) return 'bg-warning';
        return 'bg-success';
    };

    // Turlari yukle
    self.loadTours = function() {
        self.isLoading(true);

        // Token'i al
        $.ajax({
            url: apiBaseUrl + '/api/tours/my',
            method: 'GET',
            success: function(data) {
                self.tours(data.tours || []);
                self.canManageTours(data.canManageTours);
                self.isLoading(false);
            },
            error: function(xhr) {
                console.error('Turlar yuklenemedi:', xhr);
                if (xhr.status === 401) {
                    toastr.error(T('Login.Error.Required') || 'Giris yapmaniz gerekiyor');
                } else if (xhr.status === 403) {
                    var response = xhr.responseJSON;
                    toastr.warning(response?.message || 'Firma sahibi degilsiniz');
                } else {
                    toastr.error(T('Common.Error') || 'Bir hata olustu');
                }
                self.isLoading(false);
            }
        });
    };

    // Modal'i ac - Yeni tur
    self.openAddModal = function() {
        self.isEditing(false);
        self.editingTourId(null);
        self.formData({
            name: '',
            description: '',
            destination: '',
            price: 0,
            durationDays: 1,
            maxCapacity: 20,
            imageUrl: '',
            isFeatured: false,
            difficultyId: '1',
            categoryId: '0',
            guideLanguages: '',
            inclusions: '',
            exclusions: ''
        });
        tourModal.show();
    };

    // Modal'i ac - Duzenle
    self.openEditModal = function(tour) {
        self.isEditing(true);
        self.editingTourId(tour.id);
        self.formData({
            name: tour.name,
            description: tour.description,
            destination: tour.destination,
            price: tour.price,
            durationDays: tour.durationDays,
            maxCapacity: tour.maxCapacity,
            imageUrl: tour.imageUrl,
            isFeatured: tour.isFeatured,
            difficultyId: String(tour.difficultyId || 1),
            categoryId: String(tour.categoryId || 0),
            guideLanguages: tour.guideLanguages || '',
            inclusions: tour.inclusions || '',
            exclusions: tour.exclusions || '',
            meetingPointLat: tour.meetingPointLat || '',
            meetingPointLng: tour.meetingPointLng || '',
            meetingPointAddress: tour.meetingPointAddress || ''
        });
        tourModal.show();
    };

    // Tur kaydet
    self.saveTour = function() {
        var data = self.formData();

        // Validasyon
        if (!data.name || !data.destination || !data.price || !data.durationDays || !data.maxCapacity) {
            toastr.warning(T('Common.Required') || 'Zorunlu alanlari doldurun');
            return;
        }

        self.isSaving(true);

        // CompanyId'yi ekle
        var userStr = localStorage.getItem('currentUser');
        if (userStr) {
            try {
                var user = JSON.parse(userStr);
                data.companyId = user.companyId;
            } catch (e) {}
        }

        // Yeni alanlari int'e cevir
        data.difficultyId = parseInt(data.difficultyId) || 1;
        data.categoryId = parseInt(data.categoryId) || 0;

        // Meeting point alanlari
        data.meetingPointLat = data.meetingPointLat ? parseFloat(data.meetingPointLat) : null;
        data.meetingPointLng = data.meetingPointLng ? parseFloat(data.meetingPointLng) : null;
        data.meetingPointAddress = data.meetingPointAddress || null;

        var isEdit = self.isEditing();
        var url = isEdit ? apiBaseUrl + '/api/tours/' + self.editingTourId() : apiBaseUrl + '/api/tours';
        var method = isEdit ? 'PUT' : 'POST';

        if (isEdit) {
            data.id = self.editingTourId();
        }

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function() {
                toastr.success(isEdit ? (T('MyTours.UpdateSuccess') || 'Tur guncellendi') : (T('MyTours.AddSuccess') || 'Tur eklendi'));
                tourModal.hide();
                self.loadTours();
                self.isSaving(false);
            },
            error: function(xhr) {
                console.error('Tur kaydedilemedi:', xhr);
                var response = xhr.responseJSON;
                toastr.error(response?.message || T('Common.Error') || 'Bir hata olustu');
                self.isSaving(false);
            }
        });
    };

    // Silme onay modal
    self.confirmDelete = function(tour) {
        self.deletingTour(tour);
        deleteModal.show();
    };

    // Turu sil
    self.deleteTour = function() {
        if (!self.deletingTour()) return;

        self.isDeleting(true);

        $.ajax({
            url: apiBaseUrl + '/api/tours/' + self.deletingTour().id,
            method: 'DELETE',
            success: function() {
                toastr.success(T('MyTours.DeleteSuccess') || 'Tur silindi');
                deleteModal.hide();
                self.loadTours();
                self.isDeleting(false);
                self.deletingTour(null);
            },
            error: function(xhr) {
                console.error('Tur silinemedi:', xhr);
                var response = xhr.responseJSON;
                toastr.error(response?.message || T('Common.Error') || 'Bir hata olustu');
                self.isDeleting(false);
            }
        });
    };

    // Tarih yonetim modalini ac
    self.openDateManageModal = function(tour) {
        self.managingTourId(tour.id);
        self.managingTourName(tour.name);
        self.dateFormData({ startDate: '', endDate: '', price: '', maxCapacity: '' });
        self.loadManagedDates(tour.id);
        dateManageModal.show();
    };

    // Yonetim tarihlerini yukle
    self.loadManagedDates = function(tourId) {
        self.isLoadingDates(true);
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/dates/manage',
            method: 'GET',
            success: function(data) {
                self.managedDates(data);
                self.isLoadingDates(false);
            },
            error: function() {
                self.managedDates([]);
                self.isLoadingDates(false);
            }
        });
    };

    // Yeni tarih kaydet
    self.saveTourDate = function() {
        var data = self.dateFormData();
        if (!data.startDate || !data.endDate) {
            toastr.warning(T('Common.Required') || 'Baslangic ve bitis tarihi zorunlu');
            return;
        }
        self.isSavingDate(true);
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + self.managingTourId() + '/dates',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                startDate: data.startDate,
                endDate: data.endDate,
                price: data.price ? parseFloat(data.price) : null,
                maxCapacity: data.maxCapacity ? parseInt(data.maxCapacity) : null,
                isAvailable: true
            }),
            success: function() {
                toastr.success(T('TourDate.AddDate') || 'Tarih eklendi');
                self.dateFormData({ startDate: '', endDate: '', price: '', maxCapacity: '' });
                self.loadManagedDates(self.managingTourId());
                self.isSavingDate(false);
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
                self.isSavingDate(false);
            }
        });
    };

    // Tarih sil
    self.deleteTourDate = function(dateItem) {
        $.ajax({
            url: apiBaseUrl + '/api/tour-dates/' + dateItem.id,
            method: 'DELETE',
            success: function() {
                toastr.success(T('Common.Delete') || 'Tarih silindi');
                self.loadManagedDates(self.managingTourId());
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
            }
        });
    };

    // Init
    $(document).ready(function() {
        tourModal = new bootstrap.Modal(document.getElementById('tourModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        dateManageModal = new bootstrap.Modal(document.getElementById('dateManageModal'));
        self.loadTours();
    });
}

// Localization helper
if (typeof T === 'undefined') {
    window.T = function(key) {
        return null;
    };
}

ko.applyBindings(new MyToursViewModel(), document.getElementById('myToursApp'));
