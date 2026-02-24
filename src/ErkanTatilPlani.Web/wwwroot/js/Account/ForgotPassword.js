function ForgotPasswordViewModel() {
    var self = this;
    self.email = ko.observable('');
    self.isLoading = ko.observable(false);
    self.emailSent = ko.observable(false);
    self.debugResetUrl = ko.observable('');

    self.sendResetLink = function() {
        if (!self.email()) {
            toastr.error(T('ForgotPassword.EmailRequired'));
            return;
        }

        // Email format kontrolu
        var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(self.email())) {
            toastr.error(T('ForgotPassword.InvalidEmail'));
            return;
        }

        self.isLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/auth/forgot-password',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                email: self.email(),
                baseUrl: window.location.origin
            }),
            success: function(response) {
                self.emailSent(true);
                self.isLoading(false);
                toastr.success(T('ForgotPassword.EmailSent'));

                // Development mode - reset URL'i goster
                if (response.debug && response.debug.resetUrl) {
                    self.debugResetUrl(response.debug.resetUrl);
                }
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
        if (e.which === 13 && !self.emailSent()) self.sendResetLink();
    });
}

ko.applyBindings(new ForgotPasswordViewModel(), document.getElementById('forgotPasswordApp'));
