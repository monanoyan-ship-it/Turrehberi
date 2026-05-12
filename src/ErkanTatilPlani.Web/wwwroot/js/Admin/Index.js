function DashboardViewModel() {
    var self = this;

    self.stats = ko.observable({
        totalTours: 0,
        totalCompanies: 0,
        totalVisitors: 0,
        pendingReservations: 0
    });

    self.recentReservations = ko.observableArray([]);
    self.featuredTours = ko.observableArray([]);
    self.isCacheClearing = ko.observable(false);

    self.getStatusBadge = function(status) {
        var badges = {
            0: 'bg-warning text-dark', // Pending
            1: 'bg-success',           // Confirmed
            2: 'bg-danger',            // Cancelled
            3: 'bg-primary'            // Completed
        };
        return badges[status] || 'bg-secondary';
    };

    self.getStatusText = function(status) {
        var texts = {
            0: 'Beklemede',
            1: 'Onaylandi',
            2: 'Iptal',
            3: 'Tamamlandi'
        };
        return texts[status] || 'Bilinmiyor';
    };

    self.formatReservationDate = function(date, startTime) {
        if (!date || date.indexOf('0001-01-01') === 0) {
            return '-';
        }

        return startTime ? date + ' ' + startTime.toString().substring(0, 5) : date;
    };

    // Cache temizleme fonksiyonlari
    self.clearAllCache = function() {
        if (!confirm(T('Confirm.ClearAllCache'))) return;
        self.isCacheClearing(true);
        $.ajax({
            url: apiBaseUrl + '/api/cache/clear',
            method: 'POST',
            success: function(response) {
                toastr.success(T(response.message) || T('Success.CacheCleared'));
                self.loadData(); // Verileri yenile
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
            },
            complete: function() {
                self.isCacheClearing(false);
            }
        });
    };

    self.clearCache = function(type) {
        self.isCacheClearing(true);
        $.ajax({
            url: apiBaseUrl + '/api/cache/clear/' + type,
            method: 'POST',
            success: function(response) {
                toastr.success(T(response.message) || T('Success.CacheCleared'));
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
            },
            complete: function() {
                self.isCacheClearing(false);
            }
        });
    };

    self.loadData = function() {
        // Paralel istekler
        $.when(
            $.ajax({ url: apiBaseUrl + '/api/tours', method: 'GET' }),
            $.ajax({ url: apiBaseUrl + '/api/companies', method: 'GET' }),
            $.ajax({ url: apiBaseUrl + '/api/visitors', method: 'GET' }),
            $.ajax({ url: apiBaseUrl + '/api/reservations', method: 'GET' })
        ).done(function(toursRes, companiesRes, visitorsRes, reservationsRes) {
            var tours = toursRes[0].tours || toursRes[0];
            var companies = companiesRes[0];
            var visitors = visitorsRes[0];
            var reservations = reservationsRes[0];

            // Istatistikler
            self.stats({
                totalTours: tours.length,
                totalCompanies: companies.length,
                totalVisitors: visitors.length,
                pendingReservations: reservations.filter(function(r) { return r.status === 0; }).length
            });

            // Son 5 rezervasyon
            var sorted = reservations.sort(function(a, b) {
                return new Date(b.createdAt) - new Date(a.createdAt);
            });
            self.recentReservations(sorted.slice(0, 5));

            // One cikan turlar
            var featured = tours.filter(function(t) { return t.isFeatured; });
            self.featuredTours(featured.slice(0, 5));
        }).fail(function() {
            toastr.error(T('Error.DataLoadFailed'));
        });
    };

    self.loadData();
}

ko.applyBindings(new DashboardViewModel(), document.getElementById('dashboardApp'));
