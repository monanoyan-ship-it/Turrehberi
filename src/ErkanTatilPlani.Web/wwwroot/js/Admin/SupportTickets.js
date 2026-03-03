function SupportTicketsViewModel() {
    var self = this;

    // Observables
    self.tickets = ko.observableArray([]);
    self.unreadCount = ko.observable(0);
    self.selectedTicket = ko.observable(null);
    self.replyText = ko.observable('');

    var replyModal = null;

    // Load data
    self.loadData = function () {
        $.ajax({
            url: apiBaseUrl + '/api/support',
            method: 'GET'
        }).done(function (data) {
            self.tickets(data);
            self.unreadCount(data.filter(function (t) { return !t.isRead; }).length);
        }).fail(function () {
            toastr.error('Destek mesajlari yuklenemedi');
        });
    };

    // Open reply modal
    self.openReplyModal = function (ticket) {
        self.selectedTicket(ticket);
        self.replyText(ticket.adminReply || '');

        // Mark as read
        if (!ticket.isRead) {
            $.ajax({
                url: apiBaseUrl + '/api/support/' + ticket.id + '/read',
                method: 'PUT'
            }).done(function () {
                self.loadData();
            });
        }

        replyModal.show();
    };

    // Send reply
    self.sendReply = function () {
        if (!self.replyText().trim()) {
            toastr.warning('Yanit alani bos olamaz');
            return;
        }

        $.ajax({
            url: apiBaseUrl + '/api/support/' + self.selectedTicket().id + '/reply',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ reply: self.replyText() })
        }).done(function () {
            toastr.success('Yanit gonderildi');
            replyModal.hide();
            self.loadData();
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Yanit gonderilemedi');
        });
    };

    // Delete
    self.deleteTicket = function (ticket) {
        if (!confirm('Bu mesaji silmek istediginize emin misiniz?')) return;

        $.ajax({
            url: apiBaseUrl + '/api/support/' + ticket.id,
            method: 'DELETE'
        }).done(function () {
            toastr.success('Mesaj silindi');
            self.loadData();
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Silme basarisiz');
        });
    };

    // Init
    $(document).ready(function () {
        replyModal = new bootstrap.Modal(document.getElementById('replyModal'));
        self.loadData();
    });
}

ko.applyBindings(new SupportTicketsViewModel(), document.getElementById('supportApp'));
