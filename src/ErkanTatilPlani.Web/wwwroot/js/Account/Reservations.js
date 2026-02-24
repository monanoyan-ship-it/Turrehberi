function ReservationsViewModel() {
    var self = this;

    self.reservations = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.activeTab = ko.observable('pending');

    // Computed: Bekleyen (Pending/Confirmed AND odenmemis AND gelecek tarihli)
    self.pendingReservations = ko.computed(function() {
        var today = new Date();
        today.setHours(0, 0, 0, 0);
        return self.reservations().filter(function(r) {
            if (r.statusId !== 0 && r.statusId !== 1) return false;
            var rDate = new Date(r.date + 'T00:00:00');
            rDate.setHours(0, 0, 0, 0);
            var isPaid = r.paymentStatusId === 1 || r.paymentStatusId === 2;
            return rDate >= today && !isPaid;
        });
    });

    // Computed: On odeme yaptiklarim (Pending/Confirmed AND DepositPaid)
    self.depositReservations = ko.computed(function() {
        return self.reservations().filter(function(r) {
            if (r.statusId !== 0 && r.statusId !== 1) return false;
            return r.paymentStatusId === 1; // DepositPaid
        });
    });

    // Computed: Planlanmis (Pending/Confirmed AND FullyPaid)
    self.plannedReservations = ko.computed(function() {
        return self.reservations().filter(function(r) {
            if (r.statusId !== 0 && r.statusId !== 1) return false;
            return r.paymentStatusId === 2; // FullyPaid
        });
    });

    // Computed: Incelenenler (Pending/Confirmed AND gecmis tarih AND odenmemis)
    self.browsedReservations = ko.computed(function() {
        var today = new Date();
        today.setHours(0, 0, 0, 0);
        return self.reservations().filter(function(r) {
            if (r.statusId !== 0 && r.statusId !== 1) return false;
            var rDate = new Date(r.date + 'T00:00:00');
            rDate.setHours(0, 0, 0, 0);
            var isPaid = r.paymentStatusId === 1 || r.paymentStatusId === 2;
            return rDate < today && !isPaid;
        });
    });

    // Computed: Tamamlanan
    self.completedReservations = ko.computed(function() {
        return self.reservations().filter(function(r) {
            return r.statusId === 3;
        });
    });

    // Computed: Iptal edilen
    self.cancelledReservations = ko.computed(function() {
        return self.reservations().filter(function(r) {
            return r.statusId === 2;
        });
    });

    self.getStatusText = function(status) {
        var statusMap = {
            'Pending': T('Status.Pending'),
            'Confirmed': T('Status.Confirmed'),
            'Cancelled': T('Status.Cancelled'),
            'Completed': T('Status.Completed')
        };
        return statusMap[status] || status;
    };

    self.loadReservations = function() {
        self.isLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/reservations/visitor/my',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            success: function(response) {
                self.reservations(response.reservations || []);
                self.isLoading(false);
            },
            error: function(xhr) {
                if (xhr.status === 401) {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                } else {
                    toastr.error(T('Common.Error'));
                }
                self.isLoading(false);
            }
        });
    };

    // Initialize
    self.loadReservations();
}

ko.applyBindings(new ReservationsViewModel(), document.getElementById('reservationsApp'));
