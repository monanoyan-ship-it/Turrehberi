function NotificationsViewModel() {
    var self = this;

    self.notifications = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.unreadCount = ko.observable(0);
    self.page = ko.observable(1);
    self.totalPages = ko.observable(1);
    self.pageSize = 20;

    self.pageNumbers = ko.computed(function() {
        var pages = [];
        for (var i = 1; i <= self.totalPages(); i++) {
            pages.push(i);
        }
        return pages;
    });

    self.getTitle = function(n) {
        return T(n.titleKey) || n.titleKey;
    };

    self.getMessage = function(n) {
        var msg = T(n.messageKey) || n.messageKey;
        if (n.messageParams) {
            try {
                var params = JSON.parse(n.messageParams);
                Object.keys(params).forEach(function(k) {
                    msg = msg.replace('{' + k + '}', params[k]);
                });
            } catch(e) {}
        }
        return msg;
    };

    self.formatDate = function(dateStr) {
        if (!dateStr) return '';
        var d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR') + ' ' + d.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
    };

    self.loadData = function() {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/notifications?page=' + self.page() + '&pageSize=' + self.pageSize,
            method: 'GET'
        }).done(function(data) {
            self.notifications(data.notifications || []);
            self.totalPages(data.totalPages || 1);
            self.isLoading(false);
        }).fail(function(xhr) {
            if (xhr.status === 401) {
                window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            }
            self.isLoading(false);
        });
    };

    self.loadUnreadCount = function() {
        $.ajax({
            url: apiBaseUrl + '/api/notifications/unread-count',
            method: 'GET'
        }).done(function(data) {
            self.unreadCount(data.count || 0);
        });
    };

    self.markRead = function(notification) {
        $.ajax({
            url: apiBaseUrl + '/api/notifications/' + notification.id + '/read',
            method: 'PUT'
        }).done(function() {
            notification.isRead = true;
            self.notifications.valueHasMutated();
            self.loadUnreadCount();
            if (typeof loadNotificationCount === 'function') loadNotificationCount();
        });
    };

    self.markAllRead = function() {
        $.ajax({
            url: apiBaseUrl + '/api/notifications/read-all',
            method: 'PUT'
        }).done(function() {
            self.loadData();
            self.loadUnreadCount();
            if (typeof loadNotificationCount === 'function') loadNotificationCount();
            toastr.success(T('Notification.MarkAllRead'));
        });
    };

    self.changePage = function(newPage) {
        if (newPage < 1 || newPage > self.totalPages()) return;
        self.page(newPage);
        self.loadData();
    };

    $(document).ready(function() {
        self.loadData();
        self.loadUnreadCount();
    });
}

ko.applyBindings(new NotificationsViewModel(), document.getElementById('notificationsApp'));
