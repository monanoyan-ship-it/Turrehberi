function VisitorsViewModel() {
    var self = this;

    self.visitors = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.selectedVisitor = ko.observable(null);

    self.formData = ko.observable({
        id: 0,
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        address: '',
        identityNumber: '',
        birthDate: ''
    });

    var formModal, detailsModal, deleteModal;

    self.loadVisitors = function() {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/visitors',
            method: 'GET',
            success: function(data) {
                self.visitors(data);
                self.isLoading(false);
            },
            error: function() {
                toastr.error('Ziyaretciler yuklenirken hata olustu');
                self.isLoading(false);
            }
        });
    };

    self.openCreateModal = function() {
        self.isEditing(false);
        self.formData({
            id: 0,
            firstName: '',
            lastName: '',
            email: '',
            phone: '',
            address: '',
            identityNumber: '',
            birthDate: ''
        });
        formModal.show();
    };

    self.openEditModal = function(visitor) {
        self.isEditing(true);
        self.formData({
            id: visitor.id,
            firstName: visitor.firstName,
            lastName: visitor.lastName,
            email: visitor.email,
            phone: visitor.phone,
            address: visitor.address,
            identityNumber: visitor.identityNumber,
            birthDate: visitor.birthDate ? visitor.birthDate.split('T')[0] : '',
            createdAt: visitor.createdAt,
            isActive: visitor.isActive
        });
        formModal.show();
    };

    self.showDetails = function(visitor) {
        self.selectedVisitor(visitor);
        detailsModal.show();
    };

    self.openDeleteModal = function(visitor) {
        self.selectedVisitor(visitor);
        deleteModal.show();
    };

    self.saveVisitor = function() {
        var form = document.getElementById('visitorForm');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        self.isSaving(true);
        var data = self.formData();
        var isEdit = self.isEditing();

        $.ajax({
            url: apiBaseUrl + '/api/visitors' + (isEdit ? '/' + data.id : ''),
            method: isEdit ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function() {
                formModal.hide();
                self.loadVisitors();
                toastr.success(isEdit ? 'Ziyaretci guncellendi' : 'Ziyaretci eklendi');
                self.isSaving(false);
            },
            error: function() {
                toastr.error('Islem sirasinda hata olustu');
                self.isSaving(false);
            }
        });
    };

    self.deleteVisitor = function() {
        self.isSaving(true);
        var visitor = self.selectedVisitor();

        $.ajax({
            url: apiBaseUrl + '/api/visitors/' + visitor.id,
            method: 'DELETE',
            success: function() {
                deleteModal.hide();
                self.loadVisitors();
                toastr.success('Ziyaretci silindi');
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
        detailsModal = new bootstrap.Modal(document.getElementById('detailsModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        self.loadVisitors();
    });
}

ko.applyBindings(new VisitorsViewModel(), document.getElementById('visitorsApp'));
