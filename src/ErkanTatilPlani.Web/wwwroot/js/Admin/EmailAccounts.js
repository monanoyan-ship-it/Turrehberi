function EmailAccountsViewModel() {
    var self = this;

    self.accounts = ko.observableArray([]);
    self.loading = ko.observable(true);
    self.saving = ko.observable(false);
    self.editingAccount = ko.observable(null);
    self.showPassword = ko.observable(false);

    self.formData = {
        name: ko.observable(''),
        description: ko.observable(''),
        smtpHost: ko.observable(''),
        smtpPort: ko.observable(587),
        smtpUsername: ko.observable(''),
        smtpPassword: ko.observable(''),
        fromEmail: ko.observable(''),
        fromName: ko.observable(''),
        enableSsl: ko.observable(true),
        displayOrder: ko.observable(0)
    };

    // Test modal
    self.testAccountId = ko.observable(null);
    self.testAccountName = ko.observable('');
    self.testEmail = ko.observable('');
    self.sendingTest = ko.observable(false);

    self.loadAccounts = function() {
        self.loading(true);
        $.get(apiBaseUrl + '/api/admin/email-accounts')
            .done(function(data) {
                self.accounts(data);
            })
            .fail(function(xhr) {
                toastr.error('Hesaplar yuklenemedi');
            })
            .always(function() {
                self.loading(false);
            });
    };

    self.togglePassword = function() {
        self.showPassword(!self.showPassword());
    };

    self.openCreateModal = function() {
        self.editingAccount(null);
        self.resetForm();
        self.showPassword(false);
    };

    self.openEditModal = function(account) {
        self.editingAccount(account);
        self.formData.name(account.name);
        self.formData.description(account.description || '');
        self.formData.smtpHost(account.smtpHost);
        self.formData.smtpPort(account.smtpPort);
        self.formData.smtpUsername(account.smtpUsername);
        self.formData.smtpPassword('');
        self.formData.fromEmail(account.fromEmail);
        self.formData.fromName(account.fromName);
        self.formData.enableSsl(account.enableSsl);
        self.formData.displayOrder(account.displayOrder);
        self.showPassword(false);
        $('#accountModal').modal('show');
    };

    self.resetForm = function() {
        self.formData.name('');
        self.formData.description('');
        self.formData.smtpHost('smtp.gmail.com');
        self.formData.smtpPort(587);
        self.formData.smtpUsername('');
        self.formData.smtpPassword('');
        self.formData.fromEmail('');
        self.formData.fromName('');
        self.formData.enableSsl(true);
        self.formData.displayOrder(self.accounts().length);
    };

    self.saveAccount = function() {
        if (!self.formData.name() || !self.formData.smtpHost() || !self.formData.fromEmail()) {
            toastr.warning('Lutfen zorunlu alanlari doldurun');
            return;
        }

        self.saving(true);
        var data = {
            name: self.formData.name(),
            description: self.formData.description(),
            smtpHost: self.formData.smtpHost(),
            smtpPort: parseInt(self.formData.smtpPort()) || 587,
            smtpUsername: self.formData.smtpUsername(),
            smtpPassword: self.formData.smtpPassword(),
            fromEmail: self.formData.fromEmail(),
            fromName: self.formData.fromName(),
            enableSsl: self.formData.enableSsl(),
            displayOrder: parseInt(self.formData.displayOrder()) || 0
        };

        var url = apiBaseUrl + '/api/admin/email-accounts';
        var method = 'POST';

        if (self.editingAccount()) {
            url += '/' + self.editingAccount().id;
            method = 'PUT';
        }

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(data)
        })
        .done(function() {
            toastr.success(self.editingAccount() ? 'Hesap guncellendi' : 'Hesap olusturuldu');
            $('#accountModal').modal('hide');
            self.loadAccounts();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Islem basarisiz';
            toastr.error(msg);
        })
        .always(function() {
            self.saving(false);
        });
    };

    self.copyAccount = function(account) {
        if (!confirm('Bu hesabi kopyalamak istediginizden emin misiniz?')) return;

        $.post(apiBaseUrl + '/api/admin/email-accounts/' + account.id + '/copy')
            .done(function() {
                toastr.success('Hesap kopyalandi');
                self.loadAccounts();
            })
            .fail(function(xhr) {
                var msg = xhr.responseJSON?.message || 'Kopyalama basarisiz';
                toastr.error(msg);
            });
    };

    self.setDefault = function(account) {
        if (!confirm('Bu hesabi varsayilan yapmak istediginizden emin misiniz?')) return;

        $.ajax({
            url: apiBaseUrl + '/api/admin/email-accounts/' + account.id + '/set-default',
            method: 'PUT'
        })
        .done(function() {
            toastr.success('Varsayilan hesap ayarlandi');
            self.loadAccounts();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Islem basarisiz';
            toastr.error(msg);
        });
    };

    self.deleteAccount = function(account) {
        if (!confirm('Bu hesabi silmek istediginizden emin misiniz?')) return;

        $.ajax({
            url: apiBaseUrl + '/api/admin/email-accounts/' + account.id,
            method: 'DELETE'
        })
        .done(function() {
            toastr.success('Hesap silindi');
            self.loadAccounts();
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Silme basarisiz';
            toastr.error(msg);
        });
    };

    self.openTestModal = function(account) {
        self.testAccountId(account.id);
        self.testAccountName(account.name);
        self.testEmail('');
        $('#testModal').modal('show');
    };

    self.sendTestEmail = function() {
        if (!self.testEmail()) {
            toastr.warning('Lutfen alici email adresini girin');
            return;
        }

        self.sendingTest(true);
        $.ajax({
            url: apiBaseUrl + '/api/admin/email-accounts/' + self.testAccountId() + '/test',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ toEmail: self.testEmail() })
        })
        .done(function(response) {
            toastr.success(response.message || 'Test emaili gonderildi');
            $('#testModal').modal('hide');
        })
        .fail(function(xhr) {
            var msg = xhr.responseJSON?.message || 'Email gonderilemedi';
            toastr.error(msg);
        })
        .always(function() {
            self.sendingTest(false);
        });
    };

    // Init
    self.loadAccounts();
}

$(document).ready(function() {
    ko.applyBindings(new EmailAccountsViewModel(), document.getElementById('emailAccountsApp'));
});
