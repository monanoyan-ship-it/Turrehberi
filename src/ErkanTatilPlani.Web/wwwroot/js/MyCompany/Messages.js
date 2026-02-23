function CompanyMessagesViewModel() {
    var self = this;

    // Observables
    self.conversations = ko.observableArray([]);
    self.messages = ko.observableArray([]);
    self.selectedConversationId = ko.observable(null);
    self.selectedConversation = ko.observable(null);
    self.isLoading = ko.observable(false);
    self.isLoadingMessages = ko.observable(false);
    self.isSending = ko.observable(false);
    self.newMessage = ko.observable('');
    self.totalUnread = ko.observable(0);

    // Sablonlar
    self.templates = ko.observableArray([]);
    self.tplTitle = ko.observable('');
    self.tplContent = ko.observable('');
    self.tplOrder = ko.observable(0);
    self.editingTemplateId = ko.observable(null);

    // Computed
    self.isConversationClosed = ko.computed(function () {
        var conv = self.selectedConversation();
        return conv && conv.isClosedByVisitor;
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
            url: apiBaseUrl + '/api/messages/conversations/company',
            method: 'GET'
        }).done(function (data) {
            self.conversations(data.conversations || []);
            var total = 0;
            (data.conversations || []).forEach(function (c) { total += c.unreadCount || 0; });
            self.totalUnread(total);
        }).fail(function () {
            toastr.error('Konusmalar yuklenemedi');
        }).always(function () {
            self.isLoading(false);
        });
    };

    self.selectConversation = function (conv) {
        self.selectedConversationId(conv.id);
        self.selectedConversation(conv);
        self.loadMessages(conv.id);
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
            var msgs = (data.messages || []).reverse();
            self.messages(msgs);
            setTimeout(function () {
                var container = document.getElementById('messageContainer');
                if (container) container.scrollTop = container.scrollHeight;
            }, 100);
        }).fail(function () {
            toastr.error('Mesajlar yuklenemedi');
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
            toastr.error(xhr.responseJSON?.message || 'Mesaj gonderilemedi');
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

    // Sablon fonksiyonlari
    self.loadTemplates = function () {
        $.ajax({
            url: apiBaseUrl + '/api/messagetemplates',
            method: 'GET'
        }).done(function (data) {
            self.templates(data.templates || []);
        });
    };

    self.useTemplate = function (tpl) {
        self.newMessage(tpl.content);
        // Kullanim sayisi artir
        $.ajax({ url: apiBaseUrl + '/api/messagetemplates/' + tpl.id + '/use', method: 'POST' });
    };

    self.saveTemplate = function () {
        if (!self.tplTitle() || !self.tplContent()) {
            toastr.warning('Baslik ve icerik zorunlu');
            return;
        }
        var data = { title: self.tplTitle(), content: self.tplContent(), displayOrder: self.tplOrder() || 0 };

        if (self.editingTemplateId()) {
            $.ajax({
                url: apiBaseUrl + '/api/messagetemplates/' + self.editingTemplateId(),
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(data)
            }).done(function () {
                toastr.success('Sablon guncellendi');
                self.resetTemplateForm();
                self.loadTemplates();
            }).fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Guncelleme basarisiz');
            });
        } else {
            $.ajax({
                url: apiBaseUrl + '/api/messagetemplates',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data)
            }).done(function () {
                toastr.success('Sablon eklendi');
                self.resetTemplateForm();
                self.loadTemplates();
            }).fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Ekleme basarisiz');
            });
        }
    };

    self.editTemplate = function (tpl) {
        self.editingTemplateId(tpl.id);
        self.tplTitle(tpl.title);
        self.tplContent(tpl.content);
        self.tplOrder(tpl.displayOrder);
    };

    self.deleteTemplate = function (tpl) {
        if (!confirm('Bu sablonu silmek istediginize emin misiniz?')) return;
        $.ajax({
            url: apiBaseUrl + '/api/messagetemplates/' + tpl.id,
            method: 'DELETE'
        }).done(function () {
            toastr.success('Sablon silindi');
            self.loadTemplates();
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Silme basarisiz');
        });
    };

    self.resetTemplateForm = function () {
        self.editingTemplateId(null);
        self.tplTitle('');
        self.tplContent('');
        self.tplOrder(0);
    };

    // Init
    $(document).ready(function () {
        self.loadConversations();
        self.loadTemplates();
    });
}

ko.applyBindings(new CompanyMessagesViewModel(), document.getElementById('companyMessagesApp'));
