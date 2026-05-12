function RegisterViewModel() {
    var self = this;

    // Hesap tipi (0 = Ziyaretci, 1 = Tur Sirketi)
    self.userType = ko.observable('0');

    // Kisisel bilgiler
    self.firstName = ko.observable('');
    self.lastName = ko.observable('');
    self.email = ko.observable('');
    self.phone = ko.observable('');
    self.password = ko.observable('');
    self.passwordConfirm = ko.observable('');

    // Firma bilgileri
    self.companyName = ko.observable('');
    self.companyTaxNumber = ko.observable('');
    self.companyEmail = ko.observable('');
    self.companyPhone = ko.observable('');
    self.companyAddress = ko.observable('');
    self.companyWebsite = ko.observable('');

    // Form durumu
    self.acceptTerms = ko.observable(false);
    self.isLoading = ko.observable(false);

    self.syncFromDom = function() {
        var root = $('#registerApp');
        function field(name, trim) {
            var value = root.find('[data-field="' + name + '"]').val() || '';
            return trim === false ? value : value.trim();
        }

        self.userType(root.find('input[name="userType"]:checked').val() || self.userType());
        self.firstName(field('firstName'));
        self.lastName(field('lastName'));
        self.email(field('email'));
        self.phone(field('phone'));
        self.password(field('password', false));
        self.passwordConfirm(field('passwordConfirm', false));
        self.companyName(field('companyName'));
        self.companyTaxNumber(field('companyTaxNumber'));
        self.companyEmail(field('companyEmail'));
        self.companyPhone(field('companyPhone'));
        self.companyAddress(field('companyAddress'));
        self.companyWebsite(field('companyWebsite'));
        self.acceptTerms($('#termsCheck').is(':checked'));
    };

    self.redirectAfterRegister = function(user) {
        if (user.userTypeId === 1) {
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
            toastr.success(T('Register.Success', response.user.firstName));
            self.redirectAfterRegister(response.user);
        }).fail(function(xhr) {
            if (xhr.status === 404 || xhr.status === 405) {
                toastr.success(T('Register.Success', response.user.firstName));
                self.redirectAfterRegister(response.user);
                return;
            }
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            toastr.error(T('Register.Error.Failed'));
            self.isLoading(false);
        });
    };

    self.register = function() {
        self.syncFromDom();

        // Genel validasyonlar
        if (!self.firstName() || !self.lastName()) {
            toastr.error(T('Register.Error.NameRequired'));
            return;
        }
        if (!self.email()) {
            toastr.error(T('Register.Error.EmailRequired'));
            return;
        }
        if (!self.password() || self.password().length < 6) {
            toastr.error(T('Register.Error.PasswordMin'));
            return;
        }
        if (self.password() !== self.passwordConfirm()) {
            toastr.error(T('Register.Error.PasswordMismatch'));
            return;
        }
        if (!self.acceptTerms()) {
            toastr.error(T('Register.Error.TermsRequired'));
            return;
        }

        // Tur Sirketi icin ek validasyonlar
        if (self.userType() === '1') {
            if (!self.companyName()) {
                toastr.error(T('Register.Error.CompanyNameRequired'));
                return;
            }
            if (!self.companyTaxNumber()) {
                toastr.error(T('Register.Error.TaxNumberRequired'));
                return;
            }
        }

        self.isLoading(true);

        // Hesap tipine gore farkli endpoint
        var url, data;

        if (self.userType() === '1') {
            // Tur Sirketi kaydi
            url = apiBaseUrl + '/api/auth/register-company';
            data = {
                firstName: self.firstName(),
                lastName: self.lastName(),
                email: self.email(),
                phone: self.phone(),
                password: self.password(),
                company: {
                    name: self.companyName(),
                    taxNumber: self.companyTaxNumber(),
                    email: self.companyEmail(),
                    phone: self.companyPhone(),
                    address: self.companyAddress(),
                    website: self.companyWebsite()
                }
            };
        } else {
            // Ziyaretci kaydi
            url = apiBaseUrl + '/api/auth/register';
            data = {
                firstName: self.firstName(),
                lastName: self.lastName(),
                email: self.email(),
                phone: self.phone(),
                password: self.password()
            };
        }

        $.ajax({
            url: url,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(response) {
                localStorage.setItem('token', response.token);
                localStorage.setItem('user', JSON.stringify(response.user));
                self.setWebAuthToken(response);
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || T('Register.Error.Failed');
                toastr.error(msg);
                self.isLoading(false);
            }
        });
    };
}

// Wait for locale to load before applying bindings
$(document).ready(function() {
    // Check if locale is loaded, if not wait for it
    function initRegister() {
        if (Object.keys(locale).length > 0) {
            ko.applyBindings(new RegisterViewModel(), document.getElementById('registerApp'));
        } else {
            setTimeout(initRegister, 100);
        }
    }
    initRegister();
});

// Re-apply translations when language changes
$(document).on('languageChanged', function() {
    applyTranslations();
});
