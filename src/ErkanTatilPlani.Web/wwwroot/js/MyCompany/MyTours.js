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

    // Batch date
    self.isSavingBatch = ko.observable(false);
    self.batchPeriodStart = ko.observable('');
    self.batchPeriodEnd = ko.observable('');
    self.batchPrice = ko.observable('');
    self.batchMaxCapacity = ko.observable('');
    self.batchSelectedDays = ko.observableArray([]);
    // DurationUnits TypeDefinition (SystemName degerlerini kullanir)
    self.durationUnits = [
        { value: 'Hour', label: T('TourDate.DurationHour') },
        { value: 'Day', label: T('TourDate.DurationDay') },
        { value: 'Week', label: T('TourDate.DurationWeek') },
        { value: 'Month', label: T('TourDate.DurationMonth') }
    ];
    self.batchTimeSlots = ko.observableArray([
        { startTime: ko.observable('09:00'), durationValue: ko.observable(1), durationUnit: ko.observable('Day') }
    ]);
    self.batchPreview = ko.observableArray([]);
    self.batchPreviewTotal = ko.observable(0);
    self.batchPreviewDayCount = ko.observable(0);
    self.batchPreviewMore = ko.observable(0);
    self.weekDays = [
        { value: 1, label: T('WeekDay.Mon') },
        { value: 2, label: T('WeekDay.Tue') },
        { value: 3, label: T('WeekDay.Wed') },
        { value: 4, label: T('WeekDay.Thu') },
        { value: 5, label: T('WeekDay.Fri') },
        { value: 6, label: T('WeekDay.Sat') },
        { value: 0, label: T('WeekDay.Sun') }
    ];

    self.addTimeSlot = function() {
        self.batchTimeSlots.push({
            startTime: ko.observable('09:00'),
            durationValue: ko.observable(1),
            durationUnit: ko.observable('Day')
        });
    };

    self.removeTimeSlot = function(slot) {
        if (self.batchTimeSlots().length <= 1) {
            toastr.warning(T('TourDate.MinOneSlot'));
            return;
        }
        self.batchTimeSlots.remove(slot);
    };

    // Cover photo
    self.isUploadingCover = ko.observable(false);

    // Photo management
    self.photoTourId = ko.observable(null);
    self.photoTourName = ko.observable('');
    self.tourPhotos = ko.observableArray([]);
    self.isLoadingPhotos = ko.observable(false);
    self.isUploadingPhoto = ko.observable(false);
    self.photoTitle = ko.observable('');
    self.photoIsCover = ko.observable(false);

    // Modals
    var tourModal = null;
    var deleteModal = null;
    var dateManageModal = null;
    var photoModal = null;

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
                console.error('Tours load failed:', xhr);
                if (xhr.status === 401) {
                    toastr.error(T('Login.Error.Required'));
                } else if (xhr.status === 403) {
                    toastr.warning(T(xhr.responseJSON?.message) || T('Error.NotCompanyOwner'));
                } else {
                    toastr.error(T('Common.Error'));
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
            toastr.warning(T('Common.Required'));
            return;
        }

        self.isSaving(true);

        // CompanyId'yi ekle
        var userStr = localStorage.getItem('user');
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
                toastr.success(isEdit ? T('MyTours.UpdateSuccess') : T('MyTours.AddSuccess'));
                tourModal.hide();
                self.loadTours();
                self.isSaving(false);
            },
            error: function(xhr) {
                console.error('Tour save failed:', xhr);
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
                self.isSaving(false);
            }
        });
    };

    // Cover photo upload
    self.uploadCoverPhoto = function() {
        var tourId = self.editingTourId();
        if (!tourId) {
            toastr.warning(T('MyTours.CoverPhotoAfterSave'));
            return;
        }

        var fileInput = document.getElementById('tourCoverPhotoFile');
        if (!fileInput.files || !fileInput.files[0]) {
            toastr.warning(T('MyTours.SelectFile'));
            return;
        }

        self.isUploadingCover(true);
        var formData = new FormData();
        formData.append('file', fileInput.files[0]);

        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/cover-photo',
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(data) {
                toastr.success(T('MyTours.CoverPhotoUploaded'));
                var fd = self.formData();
                fd.imageUrl = data.imageUrl;
                self.formData(fd);
                self.formData.valueHasMutated();
                fileInput.value = '';
                self.isUploadingCover(false);
                self.loadTours();
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
                self.isUploadingCover(false);
            }
        });
    };

    // Cover photo delete
    self.deleteCoverPhoto = function() {
        var tourId = self.editingTourId();
        if (!tourId) return;

        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/cover-photo',
            method: 'DELETE',
            success: function() {
                toastr.success(T('MyTours.CoverPhotoDeleted'));
                var fd = self.formData();
                fd.imageUrl = '';
                self.formData(fd);
                self.formData.valueHasMutated();
                self.loadTours();
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
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
                toastr.success(T('MyTours.DeleteSuccess'));
                deleteModal.hide();
                self.loadTours();
                self.isDeleting(false);
                self.deletingTour(null);
            },
            error: function(xhr) {
                console.error('Tour delete failed:', xhr);
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
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
            toastr.warning(T('Common.Required'));
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
                toastr.success(T('TourDate.AddDate'));
                self.dateFormData({ startDate: '', endDate: '', price: '', maxCapacity: '' });
                self.loadManagedDates(self.managingTourId());
                self.isSavingDate(false);
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
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
                toastr.success(T('Common.Delete'));
                self.loadManagedDates(self.managingTourId());
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
            }
        });
    };

    // ===============================================
    // TOPLU TARIH OLUSTURMA
    // ===============================================

    self.previewBatchDates = function() {
        var periodStart = self.batchPeriodStart();
        var periodEnd = self.batchPeriodEnd();
        if (!periodStart || !periodEnd) {
            toastr.warning(T('Common.Required'));
            return;
        }

        var start = new Date(periodStart);
        var end = new Date(periodEnd);
        if (start >= end) {
            toastr.warning(T('TourDate.EndAfterStart'));
            return;
        }

        var selectedDays = self.batchSelectedDays().map(function(d) { return parseInt(d); });
        var slots = self.batchTimeSlots();
        var dayNames = [T('WeekDay.Sun'), T('WeekDay.Mon'), T('WeekDay.Tue'), T('WeekDay.Wed'), T('WeekDay.Thu'), T('WeekDay.Fri'), T('WeekDay.Sat')];
        var grouped = {};
        var totalCount = 0;
        var current = new Date(start);

        function calculateEndTime(startTime, durationValue, durationUnit) {
            var parts = startTime.split(':');
            var h = parseInt(parts[0]); var m = parseInt(parts[1] || '0');
            var totalMinutes = h * 60 + m;
            switch (durationUnit) {
                case 'Hour': totalMinutes += durationValue * 60; break;
                case 'Day': totalMinutes += durationValue * 24 * 60; break;
                case 'Week': totalMinutes += durationValue * 7 * 24 * 60; break;
                case 'Month': totalMinutes += durationValue * 30 * 24 * 60; break;
                default: totalMinutes += durationValue * 24 * 60;
            }
            var endH = Math.floor(totalMinutes / 60) % 24;
            var endM = totalMinutes % 60;
            return (endH < 10 ? '0' : '') + endH + ':' + (endM < 10 ? '0' : '') + endM;
        }

        while (current <= end) {
            var dayMatch = selectedDays.length === 0 || selectedDays.indexOf(current.getDay()) !== -1;
            if (dayMatch) {
                var dayKey = current.toLocaleDateString('tr-TR');
                var dayLabel = current.getDate() + ' ' + current.toLocaleDateString('tr-TR', { month: 'short' }) + ' ' + dayNames[current.getDay()];
                var slotList = [];
                for (var i = 0; i < slots.length; i++) {
                    var st = slots[i].startTime();
                    var dv = parseInt(slots[i].durationValue()) || 1;
                    var du = slots[i].durationUnit();
                    var et = calculateEndTime(st, dv, du);
                    slotList.push({ start: st, end: et });
                    totalCount++;
                }
                grouped[dayKey] = { label: dayLabel, slotList: slotList, dayOfWeek: current.getDay() };
            }
            if (selectedDays.length === 0) {
                current.setDate(current.getDate() + 7);
            } else {
                current.setDate(current.getDate() + 1);
            }
        }

        var previewItems = [];
        var keys = Object.keys(grouped);
        var displayKeys = keys.slice(0, 30);
        for (var k = 0; k < displayKeys.length; k++) {
            var g = grouped[displayKeys[k]];
            previewItems.push({ dayLabel: g.label, slots: g.slotList, dayOfWeek: g.dayOfWeek });
        }

        self.batchPreviewTotal(totalCount);
        self.batchPreviewDayCount(keys.length);
        self.batchPreviewMore(keys.length > 30 ? keys.length - 30 : 0);
        self.batchPreview(previewItems);
    };

    self.createBatchDates = function() {
        var periodStart = self.batchPeriodStart();
        var periodEnd = self.batchPeriodEnd();
        if (!periodStart || !periodEnd) {
            toastr.warning(T('Common.Required'));
            return;
        }

        self.isSavingBatch(true);

        var timeSlotsData = self.batchTimeSlots().map(function(slot) {
            return {
                startTime: slot.startTime(),
                durationValue: parseInt(slot.durationValue()) || 1,
                durationUnit: slot.durationUnit()
            };
        });

        var requestData = {
            periodStartDate: periodStart,
            periodEndDate: periodEnd,
            timeSlots: timeSlotsData,
            price: self.batchPrice() ? parseFloat(self.batchPrice()) : null,
            maxCapacity: self.batchMaxCapacity() ? parseInt(self.batchMaxCapacity()) : null
        };

        var selectedDays = self.batchSelectedDays().map(function(d) { return parseInt(d); });
        if (selectedDays.length > 0) {
            requestData.daysOfWeek = selectedDays;
        } else {
            requestData.repeatEveryDays = 7;
        }

        $.ajax({
            url: apiBaseUrl + '/api/tours/' + self.managingTourId() + '/dates/batch',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function(response) {
                toastr.success(T(response.message) || T('TourDate.BatchSuccess'));
                self.batchPeriodStart('');
                self.batchPeriodEnd('');
                self.batchPrice('');
                self.batchMaxCapacity('');
                self.batchSelectedDays([]);
                self.batchTimeSlots([{ startTime: ko.observable('09:00'), durationValue: ko.observable(1), durationUnit: ko.observable('Day') }]);
                self.batchPreview([]);
                self.batchPreviewTotal(0);
                self.batchPreviewDayCount(0);
                self.batchPreviewMore(0);
                self.loadManagedDates(self.managingTourId());
                self.isSavingBatch(false);
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
                self.isSavingBatch(false);
            }
        });
    };

    // ===============================================
    // FOTOGRAF YONETIMI
    // ===============================================

    self.openPhotoModal = function(tour) {
        self.photoTourId(tour.id);
        self.photoTourName(tour.name);
        self.photoTitle('');
        self.photoIsCover(false);
        self.loadTourPhotos(tour.id);
        photoModal.show();
    };

    self.loadTourPhotos = function(tourId) {
        self.isLoadingPhotos(true);
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/photos',
            method: 'GET',
            success: function(data) {
                self.tourPhotos(data || []);
                self.isLoadingPhotos(false);
            },
            error: function() {
                self.tourPhotos([]);
                self.isLoadingPhotos(false);
            }
        });
    };

    self.uploadPhoto = function() {
        var fileInput = document.getElementById('photoFile');
        if (!fileInput.files || !fileInput.files[0]) {
            toastr.warning(T('MyTours.SelectFile'));
            return;
        }

        self.isUploadingPhoto(true);

        var formData = new FormData();
        formData.append('file', fileInput.files[0]);
        formData.append('title', self.photoTitle() || '');
        formData.append('isCover', self.photoIsCover());

        $.ajax({
            url: apiBaseUrl + '/api/tours/' + self.photoTourId() + '/photos',
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(data) {
                toastr.success(T('MyTours.PhotoUploadSuccess'));
                fileInput.value = '';
                self.photoTitle('');
                self.photoIsCover(false);
                self.loadTourPhotos(self.photoTourId());
                self.isUploadingPhoto(false);
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
                self.isUploadingPhoto(false);
            }
        });
    };

    self.deletePhoto = function(photo) {
        if (!confirm(T('Confirm.DeletePhoto'))) return;

        $.ajax({
            url: apiBaseUrl + '/api/tours/' + self.photoTourId() + '/photos/' + photo.id,
            method: 'DELETE',
            success: function() {
                toastr.success(T('MyTours.PhotoDeleteSuccess'));
                self.loadTourPhotos(self.photoTourId());
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
            }
        });
    };

    self.setCoverPhoto = function(photo) {
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + self.photoTourId() + '/photos/' + photo.id + '/cover',
            method: 'PUT',
            success: function() {
                toastr.success(T('MyTours.CoverPhotoUpdated'));
                self.loadTourPhotos(self.photoTourId());
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
            }
        });
    };

    // Init
    $(document).ready(function() {
        tourModal = new bootstrap.Modal(document.getElementById('tourModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        dateManageModal = new bootstrap.Modal(document.getElementById('dateManageModal'));
        photoModal = new bootstrap.Modal(document.getElementById('photoModal'));
        self.loadTours();
    });
}

ko.applyBindings(new MyToursViewModel(), document.getElementById('myToursApp'));
