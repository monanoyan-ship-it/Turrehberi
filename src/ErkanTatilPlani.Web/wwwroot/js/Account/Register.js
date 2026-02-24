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

    self.register = function() {
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
                toastr.success(T('Register.Success', response.user.firstName));

                // Kullanici tipine gore yonlendir
                if (response.user.userTypeId === 1) {
                    window.location.href = '/MyCompany';
                } else {
                    window.location.href = '/';
                }
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
