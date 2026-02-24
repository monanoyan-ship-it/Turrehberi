function ResetPasswordViewModel() {
    var self = this;

    // URL'den parametreleri al, fallback olarak data attribute'lardan oku
    var urlParams = new URLSearchParams(window.location.search);
    var container = document.getElementById('resetPasswordApp');
    self.token = urlParams.get('token') || container.getAttribute('data-token');
    self.email = urlParams.get('email') || container.getAttribute('data-email');

    self.newPassword = ko.observable('');
    self.confirmPassword = ko.observable('');
    self.isLoading = ko.observable(false);
    self.isChecking = ko.observable(true);
    self.isValidToken = ko.observable(false);
    self.resetSuccess = ko.observable(false);

    // Token gecerliligi kontrol et
    self.verifyToken = function() {
        if (!self.token || !self.email) {
            self.isChecking(false);
            self.isValidToken(false);
            return;
        }

        $.ajax({
            url: apiBaseUrl + '/api/auth/verify-reset-token',
            method: 'GET',
            data: {
                email: self.email,
                token: self.token
            },
            success: function(response) {
                self.isValidToken(response.valid);
                self.isChecking(false);
            },
            error: function() {
                self.isValidToken(false);
                self.isChecking(false);
            }
        });
    };

    self.resetPassword = function() {
        if (!self.newPassword()) {
            toastr.error(T('Profile.NewPasswordRequired'));
            return;
        }
        if (self.newPassword().length < 6) {
            toastr.error(T('Register.Error.PasswordMin'));
            return;
        }
        if (self.newPassword() !== self.confirmPassword()) {
            toastr.error(T('Profile.PasswordMismatch'));
            return;
        }

        self.isLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/auth/reset-password',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                email: self.email,
                token: self.token,
                newPassword: self.newPassword()
            }),
            success: function(response) {
                self.resetSuccess(true);
                self.isLoading(false);
                toastr.success(T('ResetPassword.Success'));
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || T('Common.Error');
                toastr.error(msg);
                self.isLoading(false);
            }
        });
    };

    // Enter key support
    $(document).keypress(function(e) {
        if (e.which === 13 && self.isValidToken() && !self.resetSuccess()) self.resetPassword();
    });

    // Sayfa yuklendiginde token kontrol et
    self.verifyToken();
}

ko.applyBindings(new ResetPasswordViewModel(), document.getElementById('resetPasswordApp'));
