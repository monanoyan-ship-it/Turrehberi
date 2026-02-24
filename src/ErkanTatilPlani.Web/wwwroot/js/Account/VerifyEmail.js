function VerifyEmailViewModel() {
    var self = this;

    // URL'den parametreleri al, fallback olarak data attribute'lardan oku
    var urlParams = new URLSearchParams(window.location.search);
    var container = document.getElementById('verifyEmailApp');
    self.token = urlParams.get('token') || container.getAttribute('data-token');
    self.email = urlParams.get('email') || container.getAttribute('data-email');

    self.isChecking = ko.observable(true);
    self.isSuccess = ko.observable(false);
    self.hasError = ko.observable(false);
    self.hasToken = ko.observable(true);
    self.errorMessage = ko.observable('');

    self.verifyEmail = function() {
        // Token veya email yoksa
        if (!self.token || !self.email) {
            self.isChecking(false);
            self.hasToken(false);
            return;
        }

        $.ajax({
            url: apiBaseUrl + '/api/auth/verify-email',
            method: 'GET',
            data: {
                email: self.email,
                token: self.token
            },
            success: function(response) {
                self.isChecking(false);
                if (response.success) {
                    self.isSuccess(true);
                    toastr.success(T('VerifyEmail.Success'));

                    // LocalStorage'daki user bilgisini guncelle
                    var user = JSON.parse(localStorage.getItem('user') || '{}');
                    if (user.email === self.email) {
                        user.emailVerified = true;
                        localStorage.setItem('user', JSON.stringify(user));
                    }
                } else {
                    self.hasError(true);
                    self.errorMessage(response.error || T('VerifyEmail.InvalidToken'));
                }
            },
            error: function(xhr) {
                self.isChecking(false);
                self.hasError(true);
                self.errorMessage(xhr.responseJSON?.error || T('VerifyEmail.InvalidToken'));
            }
        });
    };

    // Sayfa yuklendiginde dogrulama yap
    self.verifyEmail();
}

ko.applyBindings(new VerifyEmailViewModel(), document.getElementById('verifyEmailApp'));
