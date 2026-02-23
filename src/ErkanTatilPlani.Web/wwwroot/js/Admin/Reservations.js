function AdminReservationsViewModel() {
    var self = this;
    self.reservations = ko.observableArray([]);
    self.tours = ko.observableArray([]);
    self.filterStatus = ko.observable('');
    self.filterPaymentStatus = ko.observable('');
    self.filterTour = ko.observable(null);
    self.selectedReservation = ko.observable(null);
    var detailsModal;

    self.getStatusBadge = function(s) { return ['bg-warning text-dark', 'bg-success', 'bg-danger', 'bg-primary'][s] || 'bg-secondary'; };
    self.getStatusText = function(s) { return ['Beklemede', 'Onaylandi', 'Iptal', 'Tamamlandi'][s] || 'Bilinmiyor'; };

    self.getPaymentBadge = function(s) { return ['bg-secondary', 'bg-warning text-dark', 'bg-success', 'bg-danger', 'bg-info'][s] || 'bg-secondary'; };
    self.getPaymentText = function(s) { return ['Bekliyor', 'On Odeme', 'Tam Odeme', 'Basarisiz', 'Iade'][s] || 'Bilinmiyor'; };

    self.filteredReservations = ko.computed(function() {
        var status = self.filterStatus();
        var paymentStatus = self.filterPaymentStatus();
        var tour = self.filterTour();
        return self.reservations().filter(function(r) {
            var matchStatus = status === '' || r.status.toString() === status;
            var matchPayment = paymentStatus === '' || (r.paymentStatus !== undefined && r.paymentStatus.toString() === paymentStatus);
            var matchTour = !tour || r.tourId == tour;
            return matchStatus && matchPayment && matchTour;
        });
    });

    self.loadData = function() {
        $.when($.ajax({ url: apiBaseUrl + '/api/reservations' }), $.ajax({ url: apiBaseUrl + '/api/tours' }))
            .done(function(r, t) { self.reservations(r[0]); self.tours(t[0]); });
    };

    self.showDetails = function(r) { self.selectedReservation(r); detailsModal.show(); };

    self.updateStatus = function(reservation, newStatus) {
        var data = Object.assign({}, reservation);
        data.status = newStatus;
        $.ajax({ url: apiBaseUrl + '/api/reservations/' + reservation.id, method: 'PUT', contentType: 'application/json', data: JSON.stringify(data) })
            .done(function() { self.loadData(); toastr.success(newStatus === 1 ? T('Success.Confirmed') : T('Success.Cancelled')); })
            .fail(function() { toastr.error(T('Common.Error')); });
    };

    $(document).ready(function() { detailsModal = new bootstrap.Modal(document.getElementById('detailsModal')); self.loadData(); });
}
ko.applyBindings(new AdminReservationsViewModel(), document.getElementById('adminReservationsApp'));
