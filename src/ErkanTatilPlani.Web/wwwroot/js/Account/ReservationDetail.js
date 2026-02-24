function ReservationDetailViewModel() {
    var self = this;

    self.reservation = ko.observable(null);
    self.isLoading = ko.observable(true);
    self.isCancelling = ko.observable(false);
    self.isPaymentLoading = ko.observable(false);

    var reservationId = parseInt(document.getElementById('reservationDetailApp').getAttribute('data-reservation-id')) || 0;

    self.formatDate = function(dateStr) {
        if (!dateStr) return '-';
        var d = new Date(dateStr + 'T00:00:00');
        return d.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' });
    };

    self.localizeDurationUnit = function(unit) {
        return TypeDefinitions.DurationUnits.localize(unit);
    };

    self.getStatusText = function(status) {
        var statusMap = {
            'Pending': T('Status.Pending'),
            'Confirmed': T('Status.Confirmed'),
            'Cancelled': T('Status.Cancelled'),
            'Completed': T('Status.Completed')
        };
        return statusMap[status] || status;
    };

    self.loadReservation = function() {
        self.isLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/reservations/visitor/my/' + reservationId,
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            success: function(response) {
                self.reservation(response);
                self.isLoading(false);
            },
            error: function(xhr) {
                if (xhr.status === 401) {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                } else {
                    self.reservation(null);
                }
                self.isLoading(false);
            }
        });
    };

    self.cancelReservation = function() {
        if (!confirm(T('ReservationDetail.CancelConfirm'))) return;

        self.isCancelling(true);

        $.ajax({
            url: apiBaseUrl + '/api/reservations/visitor/my/' + reservationId + '/cancel',
            method: 'PUT',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            success: function(response) {
                toastr.success(T('ReservationDetail.CancelSuccess'));
                self.loadReservation();
                self.isCancelling(false);
            },
            error: function(xhr) {
                var message = xhr.responseJSON?.message || T('Common.Error');
                toastr.error(message);
                self.isCancelling(false);
            }
        });
    };

    self.initiatePayment = function() {
        self.isPaymentLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/payments/initialize/' + reservationId,
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            success: function(response) {
                if (response.success && response.paymentPageUrl) {
                    window.location.href = response.paymentPageUrl;
                } else {
                    toastr.error(response.message || T('Payment.Error'));
                    self.isPaymentLoading(false);
                }
            },
            error: function(xhr) {
                var message = xhr.responseJSON?.message || T('Common.Error');
                toastr.error(message);
                self.isPaymentLoading(false);
            }
        });
    };

    self.initiateRemainingPayment = function() {
        self.isPaymentLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/payments/initialize-remaining/' + reservationId,
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            success: function(response) {
                if (response.success && response.paymentPageUrl) {
                    window.location.href = response.paymentPageUrl;
                } else {
                    toastr.error(response.message || T('Payment.Error'));
                    self.isPaymentLoading(false);
                }
            },
            error: function(xhr) {
                var message = xhr.responseJSON?.message || T('Common.Error');
                toastr.error(message);
                self.isPaymentLoading(false);
            }
        });
    };

    // Tarih degistirme
    self.newDate = ko.observable('');
    self.isChangingDate = ko.observable(false);

    self.changeDate = function() {
        if (!self.newDate()) {
            toastr.warning(T('DateChange.SelectNew'));
            return;
        }

        self.isChangingDate(true);
        $.ajax({
            url: apiBaseUrl + '/api/reservations/visitor/my/' + reservationId + '/change-date',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ newStartDate: self.newDate() }),
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
        }).done(function() {
            toastr.success(T('DateChange.Success'));
            self.loadReservation();
            self.isChangingDate(false);
        }).fail(function(xhr) {
            toastr.error(xhr.responseJSON?.message || T('Common.Error'));
            self.isChangingDate(false);
        });
    };

    // QR kod olusturma (client-side)
    self.generateQrCode = function() {
        var r = self.reservation();
        if (r && r.qrToken && typeof QRCode !== 'undefined') {
            var container = document.getElementById('qrCodeContainer');
            if (container) {
                container.innerHTML = '';
                new QRCode(container, {
                    text: window.location.origin + '/api/reservations/' + reservationId + '/verify?token=' + r.qrToken,
                    width: 180,
                    height: 180,
                    colorDark: '#000000',
                    colorLight: '#ffffff'
                });
            }
        }
    };

    // Reservation yuklendikten sonra QR kod olustur
    self.reservation.subscribe(function(r) {
        if (r && r.qrToken) {
            setTimeout(function() { self.generateQrCode(); }, 100);
        }
    });

    // Initialize
    if (reservationId > 0) {
        self.loadReservation();
    } else {
        self.isLoading(false);
    }
}

ko.applyBindings(new ReservationDetailViewModel(), document.getElementById('reservationDetailApp'));
