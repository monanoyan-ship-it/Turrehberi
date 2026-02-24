function ReservationsViewModel() {
    var self = this;

    self.reservations = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.selectedReservation = ko.observable(null);

    var detailsModal;

    self.localizeDurationUnit = function(unit) {
        return TypeDefinitions.DurationUnits.localize(unit);
    };

    self.getStatusText = function(status) {
        return ['Beklemede', 'Onaylandi', 'Iptal', 'Tamamlandi'][status] || 'Bilinmiyor';
    };

    self.getStatusClass = function(status) {
        return ['bg-warning text-dark', 'bg-success', 'bg-danger', 'bg-info'][status] || 'bg-secondary';
    };

    self.getPaymentText = function(status) {
        return ['Bekliyor', 'On Odeme', 'Tam Odeme', 'Basarisiz', 'Iade'][status] || 'Bilinmiyor';
    };

    self.getPaymentClass = function(status) {
        return ['bg-secondary', 'bg-warning text-dark', 'bg-success', 'bg-danger', 'bg-info'][status] || 'bg-secondary';
    };

    self.loadData = function() {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/reservations',
            method: 'GET',
            success: function(data) {
                self.reservations(data);
                self.isLoading(false);
            },
            error: function() {
                toastr.error('Veriler yuklenirken hata olustu');
                self.isLoading(false);
            }
        });
    };

    self.showDetails = function(reservation) {
        self.selectedReservation(reservation);
        detailsModal.show();
    };

    self.updateStatus = function(reservation, newStatus) {
        var statusText = newStatus === 1 ? 'onaylamak' : 'iptal etmek';
        if (!confirm('Bu rezervasyonu ' + statusText + ' istediginize emin misiniz?')) return;

        $.ajax({
            url: apiBaseUrl + '/api/reservations/' + reservation.id,
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({
                id: reservation.id,
                visitorId: reservation.visitorId,
                tourId: reservation.tourId,
                date: reservation.date,
                startTime: reservation.startTime,
                durationValue: reservation.durationValue,
                durationUnitId: reservation.durationUnitId,
                numberOfPeople: reservation.numberOfPeople,
                totalPrice: reservation.totalPrice,
                status: newStatus,
                notes: reservation.notes,
                createdAt: reservation.createdAt,
                isActive: reservation.isActive
            }),
            success: function() {
                self.loadData();
                toastr.success(newStatus === 1 ? 'Rezervasyon onaylandi' : 'Rezervasyon iptal edildi');
            },
            error: function() {
                toastr.error('Durum guncellenirken hata olustu');
            }
        });
    };

    $(document).ready(function() {
        detailsModal = new bootstrap.Modal(document.getElementById('detailsModal'));
        self.loadData();
    });
}

ko.applyBindings(new ReservationsViewModel(), document.getElementById('reservationsApp'));
