function GuidesViewModel() {
    var self = this;

    // Observables
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.isDeleting = ko.observable(false);
    self.guides = ko.observableArray([]);

    // Available languages (from API)
    self.availableLanguages = ko.observableArray([]);
    self.selectedLanguages = ko.observableArray([]);

    // Form
    self.isEditing = ko.observable(false);
    self.editingGuideId = ko.observable(null);
    self.formData = ko.observable({
        firstName: '', lastName: '', phone: '', email: '',
        bio: '', experienceYears: ''
    });

    // Detail
    self.detailGuide = ko.observable(null);
    self.tourDatesForAssign = ko.observableArray([]);
    self.selectedTourDateId = ko.observable(null);
    self.assignNotes = ko.observable('');

    // Photo modal
    self.isUploadingPhoto = ko.observable(false);
    self.photoGuideId = ko.observable(null);
    self.photoGuideName = ko.observable('');
    self.photoGuideUrl = ko.observable('');

    // Assign modal
    self.assignGuideId = ko.observable(null);
    self.assignGuideName = ko.observable('');
    self.assignGuideAssignments = ko.observableArray([]);

    // Delete
    self.deletingGuide = ko.observable(null);

    // Modals
    var guideModal = null;
    var detailModal = null;
    var deleteGuideModal = null;
    var photoModal = null;
    var assignModal = null;

    // Helper: parse languages JSON string to array
    self.parseLanguages = function(langStr) {
        if (!langStr) return [];
        try {
            var parsed = JSON.parse(langStr);
            return Array.isArray(parsed) ? parsed : [];
        } catch (e) {
            // Eski format (comma-separated) icin fallback
            return langStr.split(',').map(function(s) { return s.trim(); }).filter(Boolean);
        }
    };

    // Helper: get language display name
    self.getLanguageName = function(code) {
        var lang = self.availableLanguages().find(function(l) { return l.code === code; });
        return lang ? lang.nativeName : code.toUpperCase();
    };

    // Helper: get language flag
    self.getLanguageFlag = function(code) {
        var lang = self.availableLanguages().find(function(l) { return l.code === code; });
        return lang ? lang.flagEmoji : '';
    };

    // Toggle language selection
    self.toggleLanguage = function(lang) {
        var codes = self.selectedLanguages();
        var idx = codes.indexOf(lang.code);
        if (idx >= 0) {
            codes.splice(idx, 1);
        } else {
            codes.push(lang.code);
        }
        self.selectedLanguages(codes.slice());
    };

    // Check if language is selected
    self.isLanguageSelected = function(code) {
        return self.selectedLanguages().indexOf(code) >= 0;
    };

    // Load available languages
    self.loadLanguages = function() {
        $.ajax({
            url: apiBaseUrl + '/api/localization/languages',
            method: 'GET',
            success: function(data) {
                self.availableLanguages(data);
            }
        });
    };

    // Load guides
    self.loadData = function() {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/guides',
            method: 'GET',
            success: function(data) {
                // Her rehberin languages alanini parse et
                data.forEach(function(g) {
                    g.languageList = self.parseLanguages(g.languages);
                });
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
                                var startDt = new Date(td.startDate);
                                var dateStr = startDt.toLocaleDateString('tr-TR');
                                var timeStr = startDt.toLocaleTimeString('tr-TR', {hour:'2-digit', minute:'2-digit'});
                                dates.push({
                                    id: td.id,
                                    label: r.tour.name + ' - ' + dateStr + ' ' + timeStr
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
        self.selectedLanguages([]);
        self.formData({
            firstName: '', lastName: '', phone: '', email: '',
            bio: '', experienceYears: ''
        });
        guideModal.show();
    };

    // Open edit modal
    self.openEditModal = function(guide) {
        self.isEditing(true);
        self.editingGuideId(guide.id);
        self.selectedLanguages(self.parseLanguages(guide.languages).slice());
        self.formData({
            firstName: guide.firstName,
            lastName: guide.lastName,
            phone: guide.phone || '',
            email: guide.email || '',
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
                languages: JSON.stringify(self.selectedLanguages()),
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

    // Open assign modal
    self.openAssignModal = function(guide) {
        self.assignGuideId(guide.id);
        self.assignGuideName(guide.firstName + ' ' + guide.lastName);
        self.selectedTourDateId(null);
        self.assignNotes('');
        self.assignGuideAssignments([]);
        self.loadTourDates();

        // Rehberin mevcut atamalarini yukle
        $.ajax({
            url: apiBaseUrl + '/api/guides/' + guide.id,
            method: 'GET',
            success: function(data) {
                self.assignGuideAssignments(data.assignments || []);
            }
        });

        assignModal.show();
    };

    // Assign guide from assign modal
    self.assignGuideFromModal = function() {
        if (!self.selectedTourDateId() || !self.assignGuideId()) return;

        $.ajax({
            url: apiBaseUrl + '/api/guides/' + self.assignGuideId() + '/assign',
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
                // Atama listesini yenile
                $.ajax({
                    url: apiBaseUrl + '/api/guides/' + self.assignGuideId(),
                    method: 'GET',
                    success: function(data) {
                        self.assignGuideAssignments(data.assignments || []);
                    }
                });
                self.loadData();
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
            }
        });
    };

    // Remove assignment from assign modal
    self.removeAssignmentFromModal = function(assignment) {
        $.ajax({
            url: apiBaseUrl + '/api/guides/assignments/' + assignment.id,
            method: 'DELETE',
            success: function() {
                toastr.success(T('Guide.AssignmentRemoved') || 'Atama kaldirildi');
                self.assignGuideAssignments.remove(assignment);
                self.loadData();
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
            }
        });
    };

    // Open photo modal
    self.openPhotoModal = function(guide) {
        self.photoGuideId(guide.id);
        self.photoGuideName(guide.firstName + ' ' + guide.lastName);
        self.photoGuideUrl(guide.photoUrl || '');
        photoModal.show();
    };

    // Upload guide photo
    self.uploadGuidePhoto = function() {
        var guideId = self.photoGuideId();
        if (!guideId) return;

        var fileInput = document.getElementById('guidePhotoFile');
        if (!fileInput.files || !fileInput.files[0]) {
            toastr.warning(T('MyTours.SelectFile') || 'Lutfen bir dosya secin');
            return;
        }

        self.isUploadingPhoto(true);
        var formData = new FormData();
        formData.append('file', fileInput.files[0]);

        $.ajax({
            url: apiBaseUrl + '/api/guides/' + guideId + '/photo',
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(data) {
                toastr.success(T('Guide.PhotoUploadSuccess') || 'Fotograf yuklendi');
                self.photoGuideUrl(data.photoUrl);
                fileInput.value = '';
                self.isUploadingPhoto(false);
                self.loadData();
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
                self.isUploadingPhoto(false);
            }
        });
    };

    // Delete guide photo
    self.deleteGuidePhoto = function() {
        var guideId = self.photoGuideId();
        if (!guideId) return;

        $.ajax({
            url: apiBaseUrl + '/api/guides/' + guideId + '/photo',
            method: 'DELETE',
            success: function() {
                toastr.success(T('Guide.PhotoDeleteSuccess') || 'Fotograf silindi');
                self.photoGuideUrl('');
                self.loadData();
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Hata olustu');
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
        photoModal = new bootstrap.Modal(document.getElementById('photoModal'));
        assignModal = new bootstrap.Modal(document.getElementById('assignModal'));
        self.loadLanguages();
        self.loadData();
    });
}

if (typeof T === 'undefined') {
    window.T = function(key) { return null; };
}

ko.applyBindings(new GuidesViewModel(), document.getElementById('guidesApp'));
