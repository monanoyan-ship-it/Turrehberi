function LoginViewModel() {
    var self = this;
    self.email = ko.observable('');
    self.password = ko.observable('');
    self.isLoading = ko.observable(false);

    self.login = function() {
        if (!self.email() || !self.password()) {
            toastr.error('Email ve sifre zorunludur');
            return;
        }

        self.isLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/auth/login',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                email: self.email(),
                password: self.password()
            }),
            success: function(response) {
                localStorage.setItem('token', response.token);
                localStorage.setItem('user', JSON.stringify(response.user));
                toastr.success('Giris basarili!');

                // Redirect based on user type
                var userTypeId = response.user.userTypeId;
                if (userTypeId >= 2) { // Staff or Admin
                    window.location.href = '/Admin';
                } else if (userTypeId === 1) { // CompanyOwner
                    window.location.href = '/MyCompany';
                } else {
                    var urlParams = new URLSearchParams(window.location.search);
                    var returnUrl = urlParams.get('returnUrl');
                    window.location.href = returnUrl || '/';
                }
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || 'Giris basarisiz';
                toastr.error(msg);
                self.isLoading(false);
            }
        });
    };

    // Enter key support
    $(document).keypress(function(e) {
        if (e.which === 13) self.login();
    });
}

ko.applyBindings(new LoginViewModel(), document.getElementById('loginApp'));
