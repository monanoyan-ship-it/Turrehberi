function AdminVisitorsViewModel() {
    var self = this;
    self.visitors = ko.observableArray([]);
    self.companies = ko.observableArray([]);
    self.isSaving = ko.observable(false);
    self.isEditing = ko.observable(false);
    self.selectedVisitor = ko.observable(null);
    self.searchTerm = ko.observable('');
    self.filterUserType = ko.observable('');
    self.formData = ko.observable({ id: 0, firstName: '', lastName: '', email: '', phone: '', identityNumber: '', userTypeId: 0, companyId: null, isActive: true });
    var formModal, deleteModal;

    self.getUserTypeBadge = function(type) { return ['bg-secondary', 'bg-info', 'bg-warning text-dark', 'bg-danger'][type] || 'bg-secondary'; };
    self.getUserTypeText = function(type) { return ['Ziyaretci', 'Firma Sahibi', 'Personel', 'Admin'][type] || 'Bilinmiyor'; };

    self.filteredVisitors = ko.computed(function() {
        var search = self.searchTerm().toLowerCase();
        var type = self.filterUserType();
        return self.visitors().filter(function(v) {
            var matchSearch = !search || (v.firstName + ' ' + v.lastName + ' ' + v.email).toLowerCase().indexOf(search) > -1;
            var matchType = type === '' || v.userTypeId.toString() === type;
            return matchSearch && matchType;
        });
    });

    self.loadData = function() {
        $.when($.ajax({ url: apiBaseUrl + '/api/visitors' }), $.ajax({ url: apiBaseUrl + '/api/companies' }))
            .done(function(v, c) { self.visitors(v[0]); self.companies(c[0]); });
    };

    self.openCreateModal = function() { self.isEditing(false); self.formData({ id: 0, firstName: '', lastName: '', email: '', phone: '', identityNumber: '', userTypeId: 0, companyId: null, isActive: true }); formModal.show(); };
    self.openEditModal = function(v) { self.isEditing(true); self.formData(Object.assign({}, v)); formModal.show(); };
    self.openDeleteModal = function(v) { self.selectedVisitor(v); deleteModal.show(); };

    self.saveVisitor = function() {
        self.isSaving(true);
        var data = self.formData();
        $.ajax({ url: apiBaseUrl + '/api/visitors' + (self.isEditing() ? '/' + data.id : ''), method: self.isEditing() ? 'PUT' : 'POST', contentType: 'application/json', data: JSON.stringify(data) })
            .done(function() { formModal.hide(); self.loadData(); toastr.success('Kaydedildi'); self.isSaving(false); })
            .fail(function() { toastr.error('Hata'); self.isSaving(false); });
    };

    self.deleteVisitor = function() {
        $.ajax({ url: apiBaseUrl + '/api/visitors/' + self.selectedVisitor().id, method: 'DELETE' })
            .done(function() { deleteModal.hide(); self.loadData(); toastr.success('Silindi'); });
    };

    $(document).ready(function() { formModal = new bootstrap.Modal(document.getElementById('formModal')); deleteModal = new bootstrap.Modal(document.getElementById('deleteModal')); self.loadData(); });
}
ko.applyBindings(new AdminVisitorsViewModel(), document.getElementById('adminVisitorsApp'));
