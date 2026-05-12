function MyToursViewModel() {
    var self = this;

    function createEmptyTourFormData() {
        return {
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
            isSustainable: false,
            sustainabilityScore: 0,
            sustainabilityInfo: '',
            isWheelchairAccessible: false,
            mobilityLevel: '0',
            accessibilityInfo: '',
            meetingPointLat: '',
            meetingPointLng: '',
            meetingPointAddress: ''
        };
    }

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
    self.formData = ko.observable(createEmptyTourFormData());

    // Delete
    self.deletingTour = ko.observable(null);

    // Date management (virtual dates from schedules)
    self.managingTourId = ko.observable(null);
    self.managingTourName = ko.observable('');
    self.managedDates = ko.observableArray([]);
    self.isLoadingDates = ko.observable(false);
    self.dateFilterFrom = ko.observable('');
    self.dateFilterTo = ko.observable('');
    self.dateFilterOnlyReserved = ko.observable(false);
    self.activeDateShortcut = ko.observable('thisMonth');

    // Tarih yardimci fonksiyonlari
    function fmtDate(d) { return d.toISOString().substring(0, 10); }
    function addDays(d, n) { var r = new Date(d); r.setDate(r.getDate() + n); return r; }
    function monthStart(d) { return new Date(d.getFullYear(), d.getMonth(), 1); }
    function monthEnd(d) { return new Date(d.getFullYear(), d.getMonth() + 1, 0); }

    self.dateShortcuts = [
        { key: 'today', label: T('DateShortcut.Today') || 'Bugun' },
        { key: 'tomorrow', label: T('DateShortcut.Tomorrow') || 'Yarin' },
        { key: 'thisWeek', label: T('DateShortcut.ThisWeek') || 'Bu hafta' },
        { key: 'nextWeek', label: T('DateShortcut.NextWeek') || 'Gelecek hafta' },
        { key: 'thisMonth', label: T('DateShortcut.ThisMonth') || 'Bu ay' },
        { key: 'nextMonth', label: T('DateShortcut.NextMonth') || 'Gelecek ay' },
        { key: 'next3Months', label: T('DateShortcut.Next3Months') || '3 ay' },
    ];

    self.applyDateShortcut = function(shortcut) {
        var today = new Date();
        today.setHours(0, 0, 0, 0);
        var from, to;
        var dow = today.getDay() || 7; // Pazartesi=1
        switch (shortcut.key) {
            case 'today':
                from = to = today; break;
            case 'tomorrow':
                from = to = addDays(today, 1); break;
            case 'thisWeek':
                from = addDays(today, 1 - dow);
                to = addDays(from, 6); break;
            case 'nextWeek':
                from = addDays(today, 8 - dow);
                to = addDays(from, 6); break;
            case 'thisMonth':
                from = monthStart(today);
                to = monthEnd(today); break;
            case 'nextMonth':
                var nm = new Date(today.getFullYear(), today.getMonth() + 1, 1);
                from = nm;
                to = monthEnd(nm); break;
            case 'next3Months':
                from = today;
                to = monthEnd(new Date(today.getFullYear(), today.getMonth() + 2, 1)); break;
        }
        self.dateFilterFrom(fmtDate(from));
        self.dateFilterTo(fmtDate(to));
        self.activeDateShortcut(shortcut.key);
        self.filterManagedDates();
    };
    // DurationUnits TypeDefinition (SystemName degerlerini kullanir)
    self.getDurationUnits = function() {
        return [
            { value: 'Hour', label: T('TourDate.DurationHour') },
            { value: 'Day', label: T('TourDate.DurationDay') },
            { value: 'Week', label: T('TourDate.DurationWeek') },
            { value: 'Month', label: T('TourDate.DurationMonth') }
        ];
    };
    self.durationUnits = ko.observableArray(self.getDurationUnits());
    self.getWeekDays = function() {
        return [
            { value: 1, label: T('WeekDay.Mon') },
            { value: 2, label: T('WeekDay.Tue') },
            { value: 3, label: T('WeekDay.Wed') },
            { value: 4, label: T('WeekDay.Thu') },
            { value: 5, label: T('WeekDay.Fri') },
            { value: 6, label: T('WeekDay.Sat') },
            { value: 0, label: T('WeekDay.Sun') }
        ];
    };
    self.weekDays = ko.observableArray(self.getWeekDays());

    // ===============================================
    // SCHEDULE (TAKVIM SABLONU) YONETIMI
    // ===============================================
    self.schedules = ko.observableArray([]);
    self.isLoadingSchedules = ko.observable(false);
    self.isSavingSchedule = ko.observable(false);
    self.editingScheduleId = ko.observable(null);
    self.lockedDurationUnit = ko.observable(null); // kilitli sure tipi (varsa)
    self.isDurationUnitLocked = ko.observable(false);
    self.scheduleFormData = ko.observable({
        daysOfWeek: [],
        startTime: '09:00',
        durationValue: 1,
        durationUnit: 'Day',
        price: '',
        maxCapacity: '',
        validFrom: '',
        validTo: ''
    });
    self.scheduleSelectedDays = ko.observableArray([]);

    self.loadSchedules = function(tourId) {
        self.isLoadingSchedules(true);
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/schedules',
            method: 'GET',
            success: function(data) {
                // Her schedule icin gunleri parse et
                var items = (data || []).map(function(s) {
                    s.daysOfWeekParsed = JSON.parse(s.daysOfWeekJson || '[]');
                    return s;
                });
                self.schedules(items);

                // DurationUnit kilitleme: aktif schedule varsa kilitle
                if (items.length > 0) {
                    self.lockedDurationUnit(items[0].durationUnit);
                    self.isDurationUnitLocked(true);
                } else {
                    self.lockedDurationUnit(null);
                    self.isDurationUnitLocked(false);
                }

                // Duzenleme modunda degilse formdaki durationUnit'i senkronize et
                if (!self.editingScheduleId()) {
                    var fd = self.scheduleFormData();
                    fd.durationUnit = self.isDurationUnitLocked() ? self.lockedDurationUnit() : 'Day';
                    self.scheduleFormData(fd);
                    self.scheduleFormData.valueHasMutated();
                }

                self.isLoadingSchedules(false);
            },
            error: function() {
                self.schedules([]);
                self.lockedDurationUnit(null);
                self.isDurationUnitLocked(false);
                self.isLoadingSchedules(false);
            }
        });
    };

    self.resetScheduleForm = function() {
        self.editingScheduleId(null);
        self.scheduleSelectedDays([]);
        self.scheduleFormData({
            daysOfWeek: [],
            startTime: '09:00',
            durationValue: 1,
            durationUnit: self.isDurationUnitLocked() ? self.lockedDurationUnit() : 'Day',
            price: '',
            maxCapacity: '',
            validFrom: '',
            validTo: ''
        });
    };

    self.editSchedule = function(schedule) {
        self.editingScheduleId(schedule.id);
        self.scheduleSelectedDays(schedule.daysOfWeekParsed.map(String));
        self.scheduleFormData({
            daysOfWeek: schedule.daysOfWeekParsed,
            startTime: schedule.startTime || '09:00',
            durationValue: schedule.durationValue || 1,
            durationUnit: schedule.durationUnit || 'Day',
            price: schedule.price || '',
            maxCapacity: schedule.maxCapacity || '',
            validFrom: schedule.validFrom ? schedule.validFrom.substring(0, 10) : '',
            validTo: schedule.validTo ? schedule.validTo.substring(0, 10) : ''
        });
    };

    self.saveSchedule = function() {
        var fd = self.scheduleFormData();
        var selectedDays = self.scheduleSelectedDays().map(function(d) { return parseInt(d); });

        if (selectedDays.length === 0) {
            toastr.warning(T('Validation.DaysOfWeekRequired'));
            return;
        }
        if (!fd.startTime) {
            toastr.warning(T('Validation.StartTimeRequired'));
            return;
        }
        if (!fd.durationValue || parseInt(fd.durationValue) < 1) {
            toastr.warning(T('Validation.DurationRequired'));
            return;
        }
        if (!fd.price || parseFloat(fd.price) <= 0) {
            toastr.warning(T('Schedule.PriceRequired'));
            return;
        }
        if (!fd.validFrom || !fd.validTo) {
            toastr.warning(T('Validation.ValidDateRangeRequired'));
            return;
        }

        self.isSavingSchedule(true);

        var requestData = {
            daysOfWeek: selectedDays,
            startTime: fd.startTime || '09:00',
            durationValue: parseInt(fd.durationValue) || 1,
            durationUnit: self.isDurationUnitLocked() ? self.lockedDurationUnit() : (fd.durationUnit || 'Day'),
            price: parseFloat(fd.price),
            maxCapacity: fd.maxCapacity ? parseInt(fd.maxCapacity) : null,
            validFrom: fd.validFrom,
            validTo: fd.validTo
        };

        var isEdit = !!self.editingScheduleId();
        var url = isEdit
            ? apiBaseUrl + '/api/tour-schedules/' + self.editingScheduleId()
            : apiBaseUrl + '/api/tours/' + self.managingTourId() + '/schedules';
        var method = isEdit ? 'PUT' : 'POST';

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function(response) {
                toastr.success(T(response.message) || T('Common.Save'));
                self.resetScheduleForm();
                self.loadSchedules(self.managingTourId());
                self.isSavingSchedule(false);
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
                self.isSavingSchedule(false);
            }
        });
    };

    self.deleteSchedule = function(schedule) {
        if (!confirm(T('TourSchedule.DeleteConfirm'))) return;

        $.ajax({
            url: apiBaseUrl + '/api/tour-schedules/' + schedule.id,
            method: 'DELETE',
            success: function(response) {
                toastr.success(T(response.message) || T('Common.Delete'));
                self.loadSchedules(self.managingTourId());
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
            }
        });
    };

    self.formatDaysOfWeek = function(daysArray) {
        var dayLabels = {
            0: T('WeekDay.Sun'), 1: T('WeekDay.Mon'), 2: T('WeekDay.Tue'),
            3: T('WeekDay.Wed'), 4: T('WeekDay.Thu'), 5: T('WeekDay.Fri'), 6: T('WeekDay.Sat')
        };
        return (daysArray || []).map(function(d) { return dayLabels[d] || d; }).join(', ');
    };

    self.formatScheduleTime = function(schedule) {
        var start = schedule.startTime || '09:00';
        var dv = schedule.durationValue || 1;
        var du = schedule.durationUnit || 'Day';
        return start + ' (' + dv + ' ' + T('TourDate.Duration' + du) + ')';
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

    function createModal(id) {
        var element = document.getElementById(id);
        return element ? new bootstrap.Modal(element) : null;
    }

    // Helper functions
    self.formatCurrency = function(value) {
        if (!value) return '0 TL';
        return new Intl.NumberFormat('tr-TR', { style: 'decimal' }).format(value) + ' TL';
    };

    self.localizeDurationUnit = function(unit) {
        if (window.TypeDefinitions && TypeDefinitions.DurationUnits) {
            return TypeDefinitions.DurationUnits.localize(unit);
        }
        var fallback = {
            Hour: T('TourDate.DurationHour') || 'Saat',
            Day: T('TourDate.DurationDay') || 'Gun',
            Week: T('TourDate.DurationWeek') || 'Hafta',
            Month: T('TourDate.DurationMonth') || 'Ay'
        };
        if (fallback[unit] && fallback[unit].indexOf('TourDate.') !== 0) return fallback[unit];
        return unit || '';
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

    function normalizeTour(tour) {
        tour = tour || {};
        tour.id = tour.id || 0;
        tour.name = tour.name || '';
        tour.description = tour.description || '';
        tour.destination = tour.destination || '';
        tour.price = Number(tour.price) || 0;
        tour.durationDays = Number(tour.durationDays) || 0;
        tour.maxCapacity = Number(tour.maxCapacity) || 0;
        tour.reservationCount = Number(tour.reservationCount) || 0;
        tour.averageRating = Number(tour.averageRating) || 0;
        tour.reviewCount = Number(tour.reviewCount) || 0;
        tour.imageUrl = tour.imageUrl || '';
        tour.isActive = tour.isActive !== false;
        tour.isFeatured = !!tour.isFeatured;
        tour.difficultyId = tour.difficultyId || 1;
        tour.categoryId = tour.categoryId || 0;
        tour.guideLanguages = tour.guideLanguages || '';
        tour.inclusions = tour.inclusions || '';
        tour.exclusions = tour.exclusions || '';
        tour.isSustainable = !!tour.isSustainable;
        tour.sustainabilityScore = Number(tour.sustainabilityScore) || 0;
        tour.sustainabilityInfo = tour.sustainabilityInfo || '';
        tour.isWheelchairAccessible = !!tour.isWheelchairAccessible;
        tour.mobilityLevel = tour.mobilityLevel || 0;
        tour.accessibilityInfo = tour.accessibilityInfo || '';
        tour.meetingPointLat = tour.meetingPointLat || '';
        tour.meetingPointLng = tour.meetingPointLng || '';
        tour.meetingPointAddress = tour.meetingPointAddress || '';
        return tour;
    }

    // Turlari yukle
    self.loadTours = function() {
        self.isLoading(true);

        // Token'i al
        $.ajax({
            url: apiBaseUrl + '/api/tours/my',
            method: 'GET',
            success: function(data) {
                var tours = (data.tours || []).map(normalizeTour);
                self.tours(tours);
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
            },
            complete: function() {
                self.isLoading(false);
            }
        });
    };

    // Modal'i ac - Yeni tur
    self.openAddModal = function() {
        self.isEditing(false);
        self.editingTourId(null);
        self.formData(createEmptyTourFormData());
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
            isSustainable: tour.isSustainable || false,
            sustainabilityScore: tour.sustainabilityScore || 0,
            sustainabilityInfo: tour.sustainabilityInfo || '',
            isWheelchairAccessible: tour.isWheelchairAccessible || false,
            mobilityLevel: String(tour.mobilityLevel || 0),
            accessibilityInfo: tour.accessibilityInfo || '',
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

        // Surdurulebilirlik ve erisilebilirlik alanlari
        data.sustainabilityScore = parseInt(data.sustainabilityScore) || 0;
        data.sustainabilityInfo = data.sustainabilityInfo || null;
        data.mobilityLevel = parseInt(data.mobilityLevel) || 0;
        data.accessibilityInfo = data.accessibilityInfo || null;

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
        self.dateFilterOnlyReserved(false);
        // Default: bu ay
        var today = new Date();
        self.dateFilterFrom(fmtDate(monthStart(today)));
        self.dateFilterTo(fmtDate(monthEnd(today)));
        self.activeDateShortcut('thisMonth');
        self.resetScheduleForm();
        self.loadManagedDates(tour.id);
        self.loadSchedules(tour.id);
        dateManageModal.show();
    };

    // Sanal tarihleri yukle (schedule'lardan uretilir)
    self.loadManagedDates = function(tourId) {
        self.isLoadingDates(true);
        var params = {};
        if (self.dateFilterFrom()) params.from = self.dateFilterFrom();
        if (self.dateFilterTo()) params.to = self.dateFilterTo();
        if (self.dateFilterOnlyReserved()) params.onlyWithReservations = true;
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/dates',
            method: 'GET',
            data: params,
            success: function(data) {
                self.managedDates(data || []);
                self.isLoadingDates(false);
            },
            error: function() {
                self.managedDates([]);
                self.isLoadingDates(false);
            }
        });
    };

    self.filterManagedDates = function() {
        self.activeDateShortcut(null);
        var tourId = self.managingTourId();
        if (tourId) self.loadManagedDates(tourId);
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

    // Locale yuklendikten sonra label'lari guncelle
    onLocaleReady.push(function() {
        self.durationUnits(self.getDurationUnits());
        self.weekDays(self.getWeekDays());
    });

    // Init
    self.init = function() {
        tourModal = createModal('tourModal');
        deleteModal = createModal('deleteModal');
        dateManageModal = createModal('dateManageModal');
        photoModal = createModal('photoModal');
        self.loadTours();
    };
}

var myToursRoot = document.getElementById('myToursApp');
if (myToursRoot) {
    var myToursVM = new MyToursViewModel();
    ko.applyBindings(myToursVM, myToursRoot);
    myToursVM.init();
}
