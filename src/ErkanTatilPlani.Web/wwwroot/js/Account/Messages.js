function MessagesViewModel() {
    var self = this;

    // Observables
    self.conversations = ko.observableArray([]);
    self.messages = ko.observableArray([]);
    self.selectedConversationId = ko.observable(null);
    self.selectedConversation = ko.observable(null);
    self.isLoading = ko.observable(false);
    self.isLoadingMessages = ko.observable(false);
    self.isSending = ko.observable(false);
    self.isCreating = ko.observable(false);
    self.newMessage = ko.observable('');
    self.totalUnread = ko.observable(0);

    // Yeni konusma
    self.companies = ko.observableArray([]);
    self.companyTours = ko.observableArray([]);
    self.newConvCompanyId = ko.observable(null);
    self.newConvTourId = ko.observable(null);
    self.newConvSubject = ko.observable('');
    self.newConvMessage = ko.observable('');

    // Computed
    self.isConversationClosed = ko.computed(function () {
        var conv = self.selectedConversation();
        return conv && conv.isClosedByCompany;
    });

    // Firma secildiginde turlarini yukle
    self.newConvCompanyId.subscribe(function (companyId) {
        self.companyTours([]);
        self.newConvTourId(null);
        if (companyId) {
            $.ajax({
                url: apiBaseUrl + '/api/tours?companyId=' + companyId + '&pageSize=100',
                method: 'GET'
            }).done(function (data) {
                self.companyTours(data.tours || data || []);
            });
        }
    });

    // Helper functions
    self.formatDate = function (dateStr) {
        if (!dateStr) return '';
        var d = new Date(dateStr);
        var now = new Date();
        var diff = Math.floor((now - d) / 1000);
        if (diff < 60) return diff + 's';
        if (diff < 3600) return Math.floor(diff / 60) + 'm';
        if (diff < 86400) return Math.floor(diff / 3600) + 'h';
        if (diff < 604800) return Math.floor(diff / 86400) + 'd';
        return d.toLocaleDateString('tr-TR');
    };

    self.formatDateTime = function (dateStr) {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' });
    };

    // CRUD functions
    self.loadConversations = function () {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/messages/conversations/visitor',
            method: 'GET'
        }).done(function (data) {
            self.conversations(data.conversations || []);
            // Toplam okunmamis hesapla
            var total = 0;
            (data.conversations || []).forEach(function (c) { total += c.unreadCount || 0; });
            self.totalUnread(total);
        }).fail(function () {
            toastr.error(T('Message.LoadError') || 'Konusmalar yuklenemedi');
        }).always(function () {
            self.isLoading(false);
        });
    };

    self.selectConversation = function (conv) {
        self.selectedConversationId(conv.id);
        self.selectedConversation(conv);
        self.loadMessages(conv.id);
        // Okundu isaretle
        if (conv.unreadCount > 0) {
            $.ajax({
                url: apiBaseUrl + '/api/messages/' + conv.id + '/read',
                method: 'PUT'
            }).done(function () {
                conv.unreadCount = 0;
                self.loadConversations();
            });
        }
    };

    self.loadMessages = function (conversationId) {
        self.isLoadingMessages(true);
        $.ajax({
            url: apiBaseUrl + '/api/messages/' + conversationId + '?pageSize=100',
            method: 'GET'
        }).done(function (data) {
            // Mesajlari ters sira (en eski ustte)
            var msgs = (data.messages || []).reverse();
            self.messages(msgs);
            // En alta scroll
            setTimeout(function () {
                var container = document.getElementById('messageContainer');
                if (container) container.scrollTop = container.scrollHeight;
            }, 100);
        }).fail(function () {
            toastr.error(T('Message.LoadMessagesError') || 'Mesajlar yuklenemedi');
        }).always(function () {
            self.isLoadingMessages(false);
        });
    };

    self.sendMessage = function () {
        var content = self.newMessage();
        if (!content || !content.trim()) return;

        self.isSending(true);
        $.ajax({
            url: apiBaseUrl + '/api/messages',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                conversationId: self.selectedConversationId(),
                content: content.trim()
            })
        }).done(function () {
            self.newMessage('');
            self.loadMessages(self.selectedConversationId());
            self.loadConversations();
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || T('Message.SendError') || 'Mesaj gonderilemedi');
        }).always(function () {
            self.isSending(false);
        });
    };

    self.onMessageKeypress = function (data, event) {
        if (event.keyCode === 13 && !event.shiftKey) {
            self.sendMessage();
            return false;
        }
        return true;
    };

    self.createConversation = function () {
        if (!self.newConvCompanyId() || !self.newConvSubject() || !self.newConvMessage()) {
            toastr.warning(T('Message.FillRequired') || 'Tum alanlari doldurun');
            return;
        }

        self.isCreating(true);
        $.ajax({
            url: apiBaseUrl + '/api/messages/conversations',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                companyId: self.newConvCompanyId(),
                tourId: self.newConvTourId(),
                subject: self.newConvSubject(),
                message: self.newConvMessage()
            })
        }).done(function (resp) {
            toastr.success(T('Message.ConversationCreated') || 'Konusma olusturuldu');
            newConvModal.hide();
            self.newConvCompanyId(null);
            self.newConvTourId(null);
            self.newConvSubject('');
            self.newConvMessage('');
            self.loadConversations();
            // Yeni konusmayi sec
            if (resp.data && resp.data.conversationId) {
                setTimeout(function () {
                    var conv = self.conversations().find(function (c) { return c.id === resp.data.conversationId; });
                    if (conv) self.selectConversation(conv);
                }, 500);
            }
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || T('Message.CreateError') || 'Konusma olusturulamadi');
        }).always(function () {
            self.isCreating(false);
        });
    };

    self.loadCompanies = function () {
        $.ajax({
            url: apiBaseUrl + '/api/companies?pageSize=100',
            method: 'GET'
        }).done(function (data) {
            self.companies(data.companies || data || []);
        });
    };

    // Init
    var newConvModal = null;
    $(document).ready(function () {
        newConvModal = new bootstrap.Modal(document.getElementById('newConversationModal'));
        self.loadConversations();
        self.loadCompanies();
    });
}

ko.applyBindings(new MessagesViewModel(), document.getElementById('messagesApp'));
