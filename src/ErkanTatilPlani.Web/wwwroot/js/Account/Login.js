function LoginViewModel() {
    var self = this;
    self.email = ko.observable('');
    self.password = ko.observable('');
    self.isLoading = ko.observable(false);

    self.syncFromDom = function() {
        var root = $('#loginApp');
        self.email((root.find('[data-field="email"]').val() || '').trim());
        self.password(root.find('[data-field="password"]').val() || '');
    };

    self.getSafeReturnUrl = function(userTypeId) {
        var urlParams = new URLSearchParams(window.location.search);
        var returnUrl = urlParams.get('returnUrl');
        if (!returnUrl || returnUrl.indexOf('/') !== 0 || returnUrl.indexOf('//') === 0) return null;
        if (userTypeId >= 2) return returnUrl;
        if (userTypeId === 1) return returnUrl.indexOf('/MyCompany') === 0 ? returnUrl : null;
        return returnUrl.indexOf('/Admin') === 0 || returnUrl.indexOf('/MyCompany') === 0 ? null : returnUrl;
    };

    self.redirectAfterLogin = function(user) {
        var userTypeId = user.userTypeId;
        var returnUrl = self.getSafeReturnUrl(userTypeId);
        if (returnUrl) {
            window.location.href = returnUrl;
        } else if (userTypeId >= 2) {
            window.location.href = '/Admin';
        } else if (userTypeId === 1) {
            window.location.href = '/MyCompany';
        } else {
            window.location.href = '/';
        }
    };

    self.setWebAuthToken = function(response) {
        $.ajax({
            url: '/Account/SetAuthToken',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ token: response.token })
        }).done(function() {
            toastr.success(TL('Login.Success', 'Giris basarili!'));
            self.redirectAfterLogin(response.user);
        }).fail(function(xhr) {
            if (xhr.status === 404 || xhr.status === 405) {
                toastr.success(TL('Login.Success', 'Giris basarili!'));
                self.redirectAfterLogin(response.user);
                return;
            }
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            toastr.error(TL('Login.Error.Invalid', 'Giris basarisiz'));
            self.isLoading(false);
        });
    };

    self.tryExistingSession = function() {
        var token = localStorage.getItem('token');
        var userStr = localStorage.getItem('user');
        if (!token || !userStr) return;

        try {
            var user = JSON.parse(userStr);
            self.isLoading(true);
            self.setWebAuthToken({ token: token, user: user });
        } catch (e) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
        }
    };

    self.login = function() {
        self.syncFromDom();

        var email = self.email();
        var password = self.password();

        if (!email || !password) {
            toastr.error(TL('Login.Error.Required', 'Email ve sifre zorunludur'));
            return;
        }

        self.isLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/auth/login',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                email: email,
                password: password
            }),
            success: function(response) {
                localStorage.setItem('token', response.token);
                localStorage.setItem('user', JSON.stringify(response.user));
                self.setWebAuthToken(response);
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || xhr.responseJSON?.message || TL('Login.Error.Invalid', 'Giris basarisiz');
                toastr.error(msg);
                self.isLoading(false);
            }
        });
    };

    // Enter key support
    $(document).keypress(function(e) {
        if (e.which === 13) self.login();
    });

    self.tryExistingSession();
}

ko.applyBindings(new LoginViewModel(), document.getElementById('loginApp'));
