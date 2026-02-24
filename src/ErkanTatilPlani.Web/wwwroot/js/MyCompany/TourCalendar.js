function TourCalendarViewModel() {
    var self = this;

    self.localizeDurationUnit = function(unit) {
        return TypeDefinitions.DurationUnits.localize(unit);
    };

    // Observables
    self.isLoading = ko.observable(true);
    self.currentYear = ko.observable(new Date().getFullYear());
    self.currentMonth = ko.observable(new Date().getMonth() + 1);
    self.tours = ko.observableArray([]);
    self.selectedTourId = ko.observable(null);
    self.calendarData = ko.observable(null);
    self.selectedDateDetail = ko.observable(null);

    var dateDetailModal = null;
    var initialized = false;

    // Computed
    self.monthYearLabel = ko.computed(function() {
        var months = ['Ocak', 'Subat', 'Mart', 'Nisan', 'Mayis', 'Haziran',
                      'Temmuz', 'Agustos', 'Eylul', 'Ekim', 'Kasim', 'Aralik'];
        return months[self.currentMonth() - 1] + ' ' + self.currentYear();
    });

    self.calendarWeeks = ko.computed(function() {
        var data = self.calendarData();
        if (!data) return [];

        var weeks = [];
        var firstDay = data.firstDayOfWeek;
        var daysInMonth = data.daysInMonth;

        var currentWeek = [];
        // Ayin basindaki bos gunler
        for (var i = 0; i < firstDay; i++) {
            currentWeek.push({ day: null, tours: [], isPast: false });
        }

        for (var d = 0; d < data.days.length; d++) {
            var dayInfo = data.days[d];
            currentWeek.push({
                day: dayInfo.day,
                isPast: dayInfo.isPast,
                tours: dayInfo.tours
            });
            if (currentWeek.length === 7) {
                weeks.push(currentWeek);
                currentWeek = [];
            }
        }

        // Ayin sonundaki bos gunler
        while (currentWeek.length > 0 && currentWeek.length < 7) {
            currentWeek.push({ day: null, tours: [], isPast: false });
        }
        if (currentWeek.length > 0) weeks.push(currentWeek);

        return weeks;
    });

    // Helpers
    self.getBadgeClass = function(tourDate) {
        if (!tourDate.isAvailable) return 'bg-secondary';
        if (!tourDate.maxCapacity) return 'bg-success';
        var pct = tourDate.occupancyPercent;
        if (pct >= 80) return 'bg-danger';
        if (pct >= 50) return 'bg-warning text-dark';
        return 'bg-success';
    };

    // Navigation
    self.prevMonth = function() {
        var m = self.currentMonth() - 1;
        var y = self.currentYear();
        if (m < 1) { m = 12; y--; }
        self.currentMonth(m);
        self.currentYear(y);
        self.loadCalendar();
    };

    self.nextMonth = function() {
        var m = self.currentMonth() + 1;
        var y = self.currentYear();
        if (m > 12) { m = 1; y++; }
        self.currentMonth(m);
        self.currentYear(y);
        self.loadCalendar();
    };

    self.goToday = function() {
        self.currentYear(new Date().getFullYear());
        self.currentMonth(new Date().getMonth() + 1);
        self.loadCalendar();
    };

    // Show popover
    self.showTourDatePopover = function(tourDate) {
        self.selectedDateDetail(tourDate);
        dateDetailModal.show();
    };

    // Tour filter change
    self.selectedTourId.subscribe(function() {
        if (initialized) self.loadCalendar();
    });

    // Load calendar data
    self.loadCalendar = function() {
        self.isLoading(true);
        var url = apiBaseUrl + '/api/tour-calendar?year=' + self.currentYear() + '&month=' + self.currentMonth();
        if (self.selectedTourId()) url += '&tourId=' + self.selectedTourId();

        $.ajax({
            url: url,
            method: 'GET',
            success: function(data) {
                self.calendarData(data);
                if (data.tours && self.tours().length === 0) {
                    self.tours(data.tours);
                }
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

    // Init
    $(document).ready(function() {
        dateDetailModal = new bootstrap.Modal(document.getElementById('dateDetailModal'));
        initialized = true;
        self.loadCalendar();
    });
}

ko.applyBindings(new TourCalendarViewModel(), document.getElementById('tourCalendarApp'));
