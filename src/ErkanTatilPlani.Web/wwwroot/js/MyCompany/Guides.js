function GuidesViewModel() {
    var self = this;

    // Observables
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isDeleting = ko.observable(false);
    self.guides = ko.observableArray([]);

    // Form
    self.isEditing = ko.observable(false);
    self.editingGuideId = ko.observable(null);
    self.formData = ko.observable({
        firstName: '', lastName: '', phone: '', email: '',
        photoUrl: '', languages: '', bio: '', experienceYears: ''
    });

    // Detail
    self.detailGuide = ko.observable(null);
    self.tourDatesForAssign = ko.observableArray([]);
    self.selectedTourDateId = ko.observable(null);
    self.assignNotes = ko.observable('');

    // Delete
    self.deletingGuide = ko.observable(null);

    // Modals
    var guideModal = null;
    var detailModal = null;
    var deleteGuideModal = null;

    // Load guides
    self.loadData = function() {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/guides',
            method: 'GET',
            success: function(data) {
                self.guides(data);
                self.isLoading(false);
            },
            error: function(xhr) {
                if (xhr.status === 401) {
                    toastr.error(T('Login.Error.Required') || 'Giris yapmaniz gerekiyor');
                } else {
                    toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Bir hata olustu');
                }
                self.isLoading(false);
            }
        });
    };

    // Load tour dates for assignment dropdown
    self.loadTourDates = function() {
        $.ajax({
            url: apiBaseUrl + '/api/tours/my',
            method: 'GET',
            success: function(data) {
                var tours = data.tours || [];
                if (tours.length === 0) {
                    self.tourDatesForAssign([]);
                    return;
                }
                var requests = tours.map(function(tour) {
                    return $.ajax({
                        url: apiBaseUrl + '/api/tours/' + tour.id + '/dates',
                        method: 'GET'
                    }).then(function(tourDates) {
                        return { tour: tour, dates: tourDates };
                    });
                });
                $.when.apply($, requests).then(function() {
                    var results = tours.length === 1 ? [arguments[0]] : Array.prototype.slice.call(arguments);
                    var dates = [];
                    results.forEach(function(r) {
                        (r.dates || []).forEach(function(td) {
                            if (td.isAvailable && new Date(td.startDate) >= new Date()) {
                                dates.push({
                                    id: td.id,
                                    label: r.tour.name + ' - ' + new Date(td.startDate).toLocaleDateString('tr-TR')
                                });
                            }
                        });
                    });
                    self.tourDatesForAssign(dates);
                });
            }
        });
    };

    // Open add modal
    self.openAddModal = function() {
        self.isEditing(false);
        self.editingGuideId(null);
        self.formData({
            firstName: '', lastName: '', phone: '', email: '',
            photoUrl: '', languages: '', bio: '', experienceYears: ''
        });
        guideModal.show();
    };

    // Open edit modal
    self.openEditModal = function(guide) {
        self.isEditing(true);
        self.editingGuideId(guide.id);
        self.formData({
            firstName: guide.firstName,
            lastName: guide.lastName,
            phone: guide.phone || '',
            email: guide.email || '',
            photoUrl: guide.photoUrl || '',
            languages: guide.languages || '',
            bio: guide.bio || '',
            experienceYears: guide.experienceYears || ''
        });
        guideModal.show();
    };

    // Open detail modal
    self.openDetailModal = function(guide) {
        $.ajax({
            url: apiBaseUrl + '/api/guides/' + guide.id,
            method: 'GET',
            success: function(data) {
                self.detailGuide(data);
                self.selectedTourDateId(null);
                self.assignNotes('');
                self.loadTourDates();
                detailModal.show();
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
            }
        });
    };

    // Save guide
    self.saveGuide = function() {
        var data = self.formData();
        if (!data.firstName || !data.lastName) {
            toastr.warning(T('Common.Required') || 'Ad ve soyad zorunlu');
            return;
        }

        self.isSaving(true);
        var isEdit = self.isEditing();
        var url = isEdit ? apiBaseUrl + '/api/guides/' + self.editingGuideId() : apiBaseUrl + '/api/guides';
        var method = isEdit ? 'PUT' : 'POST';

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify({
                firstName: data.firstName,
                lastName: data.lastName,
                phone: data.phone || null,
                email: data.email || null,
                photoUrl: data.photoUrl || null,
                languages: data.languages || '',
                bio: data.bio || null,
                experienceYears: data.experienceYears ? parseInt(data.experienceYears) : null
            }),
            success: function() {
                toastr.success(isEdit ? (T('Guide.UpdateSuccess') || 'Rehber guncellendi') : (T('Guide.AddSuccess') || 'Rehber eklendi'));
                guideModal.hide();
                self.loadData();
                self.isSaving(false);
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
                self.isSaving(false);
            }
        });
    };

    // Confirm delete
    self.confirmDelete = function(guide) {
        self.deletingGuide(guide);
        deleteGuideModal.show();
    };

    // Delete guide
    self.deleteGuide = function() {
        if (!self.deletingGuide()) return;
        self.isDeleting(true);

        $.ajax({
            url: apiBaseUrl + '/api/guides/' + self.deletingGuide().id,
            method: 'DELETE',
            success: function() {
                toastr.success(T('Guide.DeleteSuccess') || 'Rehber silindi');
                deleteGuideModal.hide();
                self.loadData();
                self.isDeleting(false);
                self.deletingGuide(null);
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
                self.isDeleting(false);
            }
        });
    };

    // Assign guide to tour date
    self.assignGuide = function() {
        if (!self.selectedTourDateId() || !self.detailGuide()) return;

        $.ajax({
            url: apiBaseUrl + '/api/guides/' + self.detailGuide().id + '/assign',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                tourDateId: self.selectedTourDateId(),
                notes: self.assignNotes() || null
            }),
            success: function() {
                toastr.success(T('Guide.AssignSuccess') || 'Rehber atandi');
                self.selectedTourDateId(null);
                self.assignNotes('');
                // Refresh detail
                self.openDetailModal(self.detailGuide());
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
            }
        });
    };

    // Remove assignment
    self.removeAssignment = function(assignment) {
        $.ajax({
            url: apiBaseUrl + '/api/guides/assignments/' + assignment.id,
            method: 'DELETE',
            success: function() {
                toastr.success(T('Guide.AssignmentRemoved') || 'Atama kaldirildi');
                self.openDetailModal(self.detailGuide());
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
            }
        });
    };

    // Init
    $(document).ready(function() {
        guideModal = new bootstrap.Modal(document.getElementById('guideModal'));
        detailModal = new bootstrap.Modal(document.getElementById('detailModal'));
        deleteGuideModal = new bootstrap.Modal(document.getElementById('deleteGuideModal'));
        self.loadData();
    });
}

if (typeof T === 'undefined') {
    window.T = function(key) { return null; };
}

ko.applyBindings(new GuidesViewModel(), document.getElementById('guidesApp'));
