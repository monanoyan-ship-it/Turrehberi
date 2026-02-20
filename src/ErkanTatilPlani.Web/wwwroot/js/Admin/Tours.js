function AdminToursViewModel() {
    var self = this;

    self.tours = ko.observableArray([]);
    self.companies = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.selectedTour = ko.observable(null);

    // Filters
    self.searchTerm = ko.observable('');
    self.filterCompany = ko.observable(null);
    self.filterFeatured = ko.observable('');

    self.formData = ko.observable({
        id: 0,
        companyId: null,
        name: '',
        destination: '',
        description: '',
        price: 0,
        durationDays: 1,
        maxCapacity: 0,
        imageUrl: '',
        isFeatured: false,
        isActive: true,
        difficultyId: '1',
        categoryId: '0',
        guideLanguages: '',
        inclusions: '',
        exclusions: ''
    });

    var formModal, deleteModal;

    self.filteredTours = ko.computed(function() {
        var tours = self.tours();
        var search = self.searchTerm().toLowerCase();
        var company = self.filterCompany();
        var featured = self.filterFeatured();

        return tours.filter(function(t) {
            var matchSearch = !search ||
                t.name.toLowerCase().indexOf(search) > -1 ||
                t.destination.toLowerCase().indexOf(search) > -1;
            var matchCompany = !company || t.companyId == company;
            var matchFeatured = featured === '' || t.isFeatured.toString() === featured;
            return matchSearch && matchCompany && matchFeatured;
        });
    });

    self.loadData = function() {
        self.isLoading(true);
        $.when(
            $.ajax({ url: apiBaseUrl + '/api/tours', method: 'GET' }),
            $.ajax({ url: apiBaseUrl + '/api/companies', method: 'GET' })
        ).done(function(toursRes, companiesRes) {
            self.tours(toursRes[0]);
            self.companies(companiesRes[0]);
            self.isLoading(false);
        }).fail(function() {
            toastr.error('Veriler yuklenirken hata olustu');
            self.isLoading(false);
        });
    };

    self.openCreateModal = function() {
        self.isEditing(false);
        self.formData({
            id: 0,
            companyId: null,
            name: '',
            destination: '',
            description: '',
            price: 0,
            durationDays: 1,
            maxCapacity: 0,
            imageUrl: '',
            isFeatured: false,
            isActive: true,
            difficultyId: '1',
            categoryId: '0',
            guideLanguages: '',
            inclusions: '',
            exclusions: ''
        });
        formModal.show();
    };

    self.openEditModal = function(tour) {
        self.isEditing(true);
        self.formData({
            id: tour.id,
            companyId: tour.companyId,
            name: tour.name,
            destination: tour.destination,
            description: tour.description,
            price: tour.price,
            durationDays: tour.durationDays,
            maxCapacity: tour.maxCapacity,
            imageUrl: tour.imageUrl,
            isFeatured: tour.isFeatured,
            isActive: tour.isActive,
            createdAt: tour.createdAt,
            difficultyId: String(tour.difficultyId || 1),
            categoryId: String(tour.categoryId || 0),
            guideLanguages: tour.guideLanguages || '',
            inclusions: tour.inclusions || '',
            exclusions: tour.exclusions || ''
        });
        formModal.show();
    };

    self.openDeleteModal = function(tour) {
        self.selectedTour(tour);
        deleteModal.show();
    };

    self.saveTour = function() {
        var form = document.getElementById('tourForm');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        self.isSaving(true);
        var data = self.formData();
        data.difficultyId = parseInt(data.difficultyId) || 1;
        data.categoryId = parseInt(data.categoryId) || 0;
        var isEdit = self.isEditing();

        $.ajax({
            url: apiBaseUrl + '/api/tours' + (isEdit ? '/' + data.id : ''),
            method: isEdit ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function() {
                formModal.hide();
                self.loadData();
                toastr.success(isEdit ? 'Tur guncellendi' : 'Tur eklendi');
                self.isSaving(false);
            },
            error: function() {
                toastr.error('Islem sirasinda hata olustu');
                self.isSaving(false);
            }
        });
    };

    self.deleteTour = function() {
        self.isSaving(true);
        var tour = self.selectedTour();

        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tour.id,
            method: 'DELETE',
            success: function() {
                deleteModal.hide();
                self.loadData();
                toastr.success('Tur silindi');
                self.isSaving(false);
            },
            error: function() {
                toastr.error('Silme sirasinda hata olustu');
                self.isSaving(false);
            }
        });
    };

    $(document).ready(function() {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        self.loadData();
    });
}

ko.applyBindings(new AdminToursViewModel(), document.getElementById('adminToursApp'));
