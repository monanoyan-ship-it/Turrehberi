function CompanyDashboardViewModel() {
    var self = this;

    // Observables
    self.isLoading = ko.observable(true);
    self.stats = ko.observable({
        totalTours: 0,
        activeTours: 0,
        totalReservations: 0,
        pendingReservations: 0,
        confirmedReservations: 0,
        completedReservations: 0,
        totalRevenue: 0,
        monthlyRevenue: 0,
        totalReviews: 0,
        averageRating: 0
    });
    self.recentReservations = ko.observableArray([]);
    self.recentReviews = ko.observableArray([]);
    self.tourPerformance = ko.observableArray([]);

    // Firma ayarlari
    self.companySettings = ko.observable({
        depositPercentage: 30
    });
    self.isSavingSettings = ko.observable(false);

    // TODO: Gercek auth sisteminden alinacak
    self.companyId = ko.observable(null);

    // Helper functions
    self.formatCurrency = function(value) {
        if (!value) return '0 TL';
        return new Intl.NumberFormat('tr-TR', { style: 'decimal' }).format(value) + ' TL';
    };

    self.formatDate = function(dateString) {
        if (!dateString) return '';
        var date = new Date(dateString);
        return date.toLocaleDateString('tr-TR');
    };

    self.getStars = function(rating) {
        var html = '';
        for (var i = 0; i < 5; i++) {
            if (i < rating) {
                html += '<i class="bi bi-star-fill"></i>';
            } else {
                html += '<i class="bi bi-star"></i>';
            }
        }
        return html;
    };

    self.getStatusBadgeClass = function(status) {
        var classes = {
            'Pending': 'bg-warning',
            'Confirmed': 'bg-success',
            'Cancelled': 'bg-danger',
            'Completed': 'bg-info'
        };
        return classes[status] || 'bg-secondary';
    };

    self.getStatusText = function(status) {
        var keys = {
            'Pending': 'Status.Pending',
            'Confirmed': 'Status.Confirmed',
            'Cancelled': 'Status.Cancelled',
            'Completed': 'Status.Completed'
        };
        return keys[status] ? T(keys[status]) : status;
    };

    // Dashboard verilerini yukle
    self.loadDashboard = function() {
        self.isLoading(true);

        // Firma ID'sini al (localStorage'dan veya session'dan)
        var userStr = localStorage.getItem('user');
        if (userStr) {
            try {
                var user = JSON.parse(userStr);
                if (user.companyId) {
                    self.companyId(user.companyId);
                }
            } catch (e) {
                console.error('User info read failed:', e);
            }
        }

        if (!self.companyId()) {
            // Test icin varsayilan firma ID (gercek uygulamada kaldirılacak)
            self.companyId(1);
        }

        $.ajax({
            url: apiBaseUrl + '/api/companies/' + self.companyId() + '/dashboard',
            method: 'GET',
            success: function(data) {
                self.stats(data.stats);
                self.recentReservations(data.recentReservations || []);
                self.recentReviews(data.recentReviews || []);
                self.tourPerformance(data.tourPerformance || []);

                // Firma ayarlarini yukle
                if (data.settings) {
                    self.companySettings({
                        depositPercentage: data.settings.depositPercentage || 30
                    });
                }

                self.isLoading(false);
            },
            error: function(xhr) {
                console.error('Dashboard load failed:', xhr);
                toastr.error(T('Common.Error'));
                self.isLoading(false);
            }
        });
    };

    // Firma ayarlarini kaydet
    self.saveCompanySettings = function() {
        var settings = self.companySettings();

        // Validasyon
        if (settings.depositPercentage < 10 || settings.depositPercentage > 100) {
            toastr.warning(T('Validation.DepositPercentRange'));
            return;
        }

        self.isSavingSettings(true);

        $.ajax({
            url: apiBaseUrl + '/api/companies/' + self.companyId() + '/settings',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({
                depositPercentage: parseInt(settings.depositPercentage)
            }),
            success: function(response) {
                toastr.success(T('Success.SettingsSaved'));
                self.isSavingSettings(false);
            },
            error: function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
                self.isSavingSettings(false);
            }
        });
    };

    // Sayfa yuklendiginde calistir
    self.loadDashboard();
}


$(document).ready(function() {
    ko.applyBindings(new CompanyDashboardViewModel(), document.getElementById('companyDashboardApp'));
});
