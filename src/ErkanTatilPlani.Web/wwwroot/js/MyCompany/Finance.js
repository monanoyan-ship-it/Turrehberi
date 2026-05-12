function CompanyFinanceViewModel() {
    var self = this;

    function createEmptySummary() {
        return {
            grossVolume: 0,
            sellerReceivable: 0,
            availableForPayout: 0,
            refundedAmount: 0
        };
    }

    self.seller = ko.observable({});
    self.summary = ko.observable(createEmptySummary());
    self.transactions = ko.observableArray([]);
    self.payouts = ko.observableArray([]);
    self.refunds = ko.observableArray([]);
    self.isSaving = ko.observable(false);

    self.sellerForm = {
        sellerLegalTypeId: ko.observable(2),
        marketplaceEnabled: ko.observable(false),
        legalCompanyTitle: ko.observable(''),
        taxOffice: ko.observable(''),
        taxNumber: ko.observable(''),
        iban: ko.observable(''),
        contactName: ko.observable(''),
        contactSurname: ko.observable('')
    };

    self.formatMoney = function(value) {
        return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(Number(value || 0));
    };

    self.formatDate = function(value) {
        if (!value) return '-';
        return new Date(value).toLocaleDateString('tr-TR');
    };

    self.onboardingBadge = function(statusId) {
        return {
            0: 'bg-warning text-dark',
            1: 'bg-info',
            2: 'bg-primary',
            3: 'bg-success',
            4: 'bg-danger',
            5: 'bg-secondary'
        }[statusId] || 'bg-secondary';
    };

    self.transactionBadge = function(statusId) {
        return {
            0: 'bg-secondary',
            1: 'bg-success',
            2: 'bg-danger',
            3: 'bg-warning text-dark',
            4: 'bg-info'
        }[statusId] || 'bg-secondary';
    };

    self.payoutBadge = function(statusId) {
        return {
            0: 'bg-secondary',
            1: 'bg-primary',
            2: 'bg-success',
            3: 'bg-danger'
        }[statusId] || 'bg-secondary';
    };

    self.refundBadge = function(statusId) {
        return {
            0: 'bg-warning text-dark',
            1: 'bg-primary',
            2: 'bg-success',
            3: 'bg-danger',
            4: 'bg-secondary'
        }[statusId] || 'bg-secondary';
    };

    self.localizeOnboardingStatus = function(status) {
        if (!status) return '-';

        var key = 'SellerOnboardingStatus.' + status;
        var translated = T(key);
        if (translated && translated !== key) return translated;

        return {
            MissingInfo: 'Eksik bilgi',
            ReadyForSubmission: 'Gonderime hazir',
            Submitted: 'Gonderildi',
            Active: 'Aktif',
            Failed: 'Basarisiz',
            Suspended: 'Askida'
        }[status] || status;
    };

    function enrichSeller(seller) {
        seller = seller || {};
        seller.localizedOnboardingStatus = self.localizeOnboardingStatus(seller.onboardingStatus);
        return seller;
    }

    function refreshSellerLocalization() {
        var seller = self.seller();
        if (!seller) return;

        self.seller(enrichSeller(seller));
    }

    self.load = function() {
        $.get(apiBaseUrl + '/api/marketplace/my')
            .done(function(data) {
                var seller = enrichSeller(data.seller || {});
                self.seller(seller);
                self.summary(data.summary || createEmptySummary());
                self.transactions(data.transactions || []);
                self.payouts(data.payouts || []);
                self.refunds(data.refunds || []);
                fillSellerForm(seller);
            })
            .fail(function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Error.DataLoadFailed') || 'Veri yuklenemedi');
            });
    };

    self.saveSettings = function() {
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/marketplace/my/settings',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(buildPayload())
        }).done(function(response) {
            toastr.success(T(response.message) || 'Kaydedildi');
            self.load();
        }).fail(function(xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        }).always(function() {
            self.isSaving(false);
        });
    };

    self.onboard = function() {
        self.isSaving(true);
        $.post(apiBaseUrl + '/api/marketplace/my/onboard')
            .done(function(response) {
                toastr.success(T(response.message) || 'Onboarding tamamlandi');
                self.load();
            })
            .fail(function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || xhr.responseJSON?.errorMessage || T('Common.Error'));
            })
            .always(function() {
                self.isSaving(false);
            });
    };

    function fillSellerForm(seller) {
        self.sellerForm.sellerLegalTypeId(String(seller.sellerLegalTypeId || 2));
        self.sellerForm.marketplaceEnabled(!!seller.marketplaceEnabled);
        self.sellerForm.legalCompanyTitle(seller.legalCompanyTitle || '');
        self.sellerForm.taxOffice(seller.taxOffice || '');
        self.sellerForm.taxNumber(seller.taxNumber || '');
        self.sellerForm.iban(seller.iban || '');
        self.sellerForm.contactName(seller.contactName || '');
        self.sellerForm.contactSurname(seller.contactSurname || '');
    }

    function buildPayload() {
        var seller = self.seller();
        return {
            sellerLegalTypeId: Number(self.sellerForm.sellerLegalTypeId()),
            marketplaceEnabled: self.sellerForm.marketplaceEnabled(),
            legalCompanyTitle: self.sellerForm.legalCompanyTitle(),
            taxOffice: self.sellerForm.taxOffice(),
            taxNumber: self.sellerForm.taxNumber(),
            iban: self.sellerForm.iban(),
            contactName: self.sellerForm.contactName(),
            contactSurname: self.sellerForm.contactSurname(),
            email: seller.email,
            phone: seller.phone,
            address: seller.address
        };
    }

    $(document).ready(function() {
        self.load();
    });

    onLocaleReady.push(function() {
        refreshSellerLocalization();
    });
}

var companyFinanceRoot = document.getElementById('companyFinanceApp');
if (companyFinanceRoot) {
    ko.applyBindings(new CompanyFinanceViewModel(), companyFinanceRoot);
}
