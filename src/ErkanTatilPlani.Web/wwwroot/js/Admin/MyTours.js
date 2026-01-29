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
        isFeatured: false
    });

    // Delete
    self.deletingTour = ko.observable(null);

    // Modals
    var tourModal = null;
    var deleteModal = null;

    // Helper functions
    self.formatCurrency = function(value) {
        if (!value) return '0 TL';
        return new Intl.NumberFormat('tr-TR', { style: 'decimal' }).format(value) + ' TL';
    };

    // Turlari yukle
    self.loadTours = function() {
        self.isLoading(true);

        // Token'i al
        var token = localStorage.getItem('authToken');

        $.ajax({
            url: apiBaseUrl + '/api/tours/my',
            method: 'GET',
            headers: token ? { 'Authorization': 'Bearer ' + token } : {},
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
            isFeatured: false
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
            isFeatured: tour.isFeatured
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
        var token = localStorage.getItem('authToken');

        // CompanyId'yi ekle
        var userStr = localStorage.getItem('currentUser');
        if (userStr) {
            try {
                var user = JSON.parse(userStr);
                data.companyId = user.companyId;
            } catch (e) {}
        }

        var isEdit = self.isEditing();
        var url = isEdit ? apiBaseUrl + '/api/tours/' + self.editingTourId() : apiBaseUrl + '/api/tours';
        var method = isEdit ? 'PUT' : 'POST';

        if (isEdit) {
            data.id = self.editingTourId();
        }

        $.ajax({
            url: url,
            method: method,
            headers: token ? { 'Authorization': 'Bearer ' + token } : {},
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
        var token = localStorage.getItem('authToken');

        $.ajax({
            url: apiBaseUrl + '/api/tours/' + self.deletingTour().id,
            method: 'DELETE',
            headers: token ? { 'Authorization': 'Bearer ' + token } : {},
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

    // Init
    $(document).ready(function() {
        tourModal = new bootstrap.Modal(document.getElementById('tourModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
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
