function MyReservationsViewModel() {
    var self = this;

    // Observables
    self.isLoading = ko.observable(true);
    self.reservations = ko.observableArray([]);
    self.stats = ko.observable({ total: 0, pending: 0, confirmed: 0, cancelled: 0, completed: 0 });
    self.currentFilter = ko.observable('all');
    self.selectedReservation = ko.observable(null);

    // Modals
    var detailModal = null;

    // Helper functions
    self.formatCurrency = function(value) {
        if (!value) return '0 TL';
        return new Intl.NumberFormat('tr-TR', { style: 'decimal' }).format(value) + ' TL';
    };

    self.formatDate = function(dateString) {
        if (!dateString) return '';
        var date = new Date(dateString);
        var text = date.toLocaleDateString('tr-TR');
        var h = date.getUTCHours();
        var m = date.getUTCMinutes();
        if (h || m) text += ' ' + String(h).padStart(2,'0') + ':' + String(m).padStart(2,'0');
        return text;
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

    // Rezervasyonlari yukle
    self.loadReservations = function(status) {
        self.isLoading(true);
        var url = apiBaseUrl + '/api/reservations/my';
        if (status && status !== 'all') {
            url += '?status=' + status;
        }

        $.ajax({
            url: url,
            method: 'GET',
            success: function(data) {
                self.reservations(data.reservations || []);
                self.stats(data.stats || { total: 0, pending: 0, confirmed: 0, cancelled: 0, completed: 0 });
                self.isLoading(false);
            },
            error: function(xhr) {
                console.error('Rezervasyonlar yuklenemedi:', xhr);
                if (xhr.status === 401) {
                    toastr.error(T('Login.Error.Required') || 'Giris yapmaniz gerekiyor');
                } else if (xhr.status === 403) {
                    toastr.warning(xhr.responseJSON?.message || 'Firma sahibi degilsiniz');
                } else {
                    toastr.error(T('Common.Error') || 'Bir hata olustu');
                }
                self.isLoading(false);
            }
        });
    };

    // Filtrele
    self.filterByStatus = function(status) {
        self.currentFilter(status);
        self.loadReservations(status);
    };

    // Detay goster
    self.showDetails = function(reservation) {
        self.selectedReservation(reservation);
        detailModal.show();
    };

    // Durum guncelle
    self.updateStatus = function(reservation, newStatus) {

        $.ajax({
            url: apiBaseUrl + '/api/reservations/my/' + reservation.id + '/status',
            method: 'PATCH',
            contentType: 'application/json',
            data: JSON.stringify({ status: newStatus }),
            success: function() {
                var messages = {
                    'Confirmed': T('MyReservations.ConfirmSuccess') || 'Rezervasyon onaylandi',
                    'Cancelled': T('MyReservations.CancelSuccess') || 'Rezervasyon iptal edildi',
                    'Completed': T('MyReservations.CompleteSuccess') || 'Rezervasyon tamamlandi'
                };
                toastr.success(messages[newStatus] || 'Durum guncellendi');
                self.loadReservations(self.currentFilter());
            },
            error: function(xhr) {
                console.error('Durum guncellenemedi:', xhr);
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Bir hata olustu');
            }
        });
    };

    // Init
    $(document).ready(function() {
        detailModal = new bootstrap.Modal(document.getElementById('detailModal'));
        self.loadReservations();
    });
}

ko.applyBindings(new MyReservationsViewModel(), document.getElementById('myReservationsApp'));
