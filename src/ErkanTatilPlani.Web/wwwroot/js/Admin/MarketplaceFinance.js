function AdminMarketplaceFinanceViewModel() {
    var self = this;

    self.summary = ko.observable({});
    self.sellers = ko.observableArray([]);
    self.transactions = ko.observableArray([]);
    self.payouts = ko.observableArray([]);
    self.refunds = ko.observableArray([]);
    self.isSaving = ko.observable(false);
    self.selectedSeller = ko.observable(null);
    self.selectedTransaction = ko.observable(null);
    self.selectedPayout = ko.observable(null);

    self.sellerForm = {
        sellerLegalTypeId: ko.observable(2),
        marketplaceEnabled: ko.observable(false),
        platformCommissionRate: ko.observable(12),
        payoutDelayDays: ko.observable(7),
        legalCompanyTitle: ko.observable(''),
        taxOffice: ko.observable(''),
        taxNumber: ko.observable(''),
        iban: ko.observable(''),
        contactName: ko.observable(''),
        contactSurname: ko.observable('')
    };
    self.refundForm = { amount: ko.observable(0), reason: ko.observable('') };
    self.payoutForm = { periodStart: ko.observable(''), periodEnd: ko.observable(''), notes: ko.observable('') };
    self.paidForm = { bankReference: ko.observable(''), notes: ko.observable('') };

    var sellerModal, refundModal, payoutModal, paidModal;

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

    self.loadAll = function() {
        $.when(
            $.get(apiBaseUrl + '/api/marketplace/admin/overview'),
            $.get(apiBaseUrl + '/api/marketplace/admin/sellers'),
            $.get(apiBaseUrl + '/api/marketplace/admin/transactions'),
            $.get(apiBaseUrl + '/api/marketplace/admin/payouts'),
            $.get(apiBaseUrl + '/api/marketplace/admin/refunds')
        ).done(function(overview, sellers, transactions, payouts, refunds) {
            self.summary(overview[0].summary || {});
            self.sellers(sellers[0].sellers || []);
            self.transactions(transactions[0].transactions || []);
            self.payouts(payouts[0].payouts || []);
            self.refunds(refunds[0].refunds || []);
        }).fail(function(xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Error.DataLoadFailed') || 'Veri yuklenemedi');
        });
    };

    self.openSellerModal = function(seller) {
        self.selectedSeller(seller);
        self.sellerForm.sellerLegalTypeId(String(seller.sellerLegalTypeId || 2));
        self.sellerForm.marketplaceEnabled(!!seller.marketplaceEnabled);
        self.sellerForm.platformCommissionRate(seller.platformCommissionRate || 12);
        self.sellerForm.payoutDelayDays(seller.payoutDelayDays || 7);
        self.sellerForm.legalCompanyTitle(seller.legalCompanyTitle || '');
        self.sellerForm.taxOffice(seller.taxOffice || '');
        self.sellerForm.taxNumber(seller.taxNumber || '');
        self.sellerForm.iban(seller.iban || '');
        self.sellerForm.contactName(seller.contactName || '');
        self.sellerForm.contactSurname(seller.contactSurname || '');
        sellerModal.show();
    };

    self.saveSeller = function() {
        var seller = self.selectedSeller();
        if (!seller) return;
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/marketplace/admin/sellers/' + seller.id,
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(buildSellerPayload(seller))
        }).done(function(response) {
            toastr.success(T(response.message) || 'Kaydedildi');
            sellerModal.hide();
            self.loadAll();
        }).fail(function(xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        }).always(function() {
            self.isSaving(false);
        });
    };

    self.onboardSeller = function(seller) {
        self.isSaving(true);
        $.post(apiBaseUrl + '/api/marketplace/admin/sellers/' + seller.id + '/onboard')
            .done(function(response) {
                toastr.success(T(response.message) || 'Onboarding tamamlandi');
                self.loadAll();
            })
            .fail(function(xhr) {
                toastr.error(T(xhr.responseJSON?.message) || xhr.responseJSON?.errorMessage || T('Common.Error'));
            })
            .always(function() { self.isSaving(false); });
    };

    self.openRefundModal = function(transaction) {
        self.selectedTransaction(transaction);
        self.refundForm.amount(Math.max(Number(transaction.paidAmount || 0) - Number(transaction.refundedAmount || 0), 0).toFixed(2));
        self.refundForm.reason('');
        refundModal.show();
    };

    self.createRefund = function() {
        var transaction = self.selectedTransaction();
        if (!transaction) return;
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/marketplace/admin/transactions/' + transaction.id + '/refund',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ amount: Number(self.refundForm.amount()), reason: self.refundForm.reason() })
        }).done(function(response) {
            toastr.success(T(response.message) || 'Iade islendi');
            refundModal.hide();
            self.loadAll();
        }).fail(function(xhr) {
            toastr.error(T(xhr.responseJSON?.message) || xhr.responseJSON?.errorMessage || T('Common.Error'));
        }).always(function() {
            self.isSaving(false);
        });
    };

    self.openPayoutModal = function(seller) {
        self.selectedSeller(seller);
        var now = new Date();
        var start = new Date(now.getFullYear(), now.getMonth(), 1);
        self.payoutForm.periodStart(start.toISOString().slice(0, 10));
        self.payoutForm.periodEnd(now.toISOString().slice(0, 10));
        self.payoutForm.notes('');
        payoutModal.show();
    };

    self.createPayout = function() {
        var seller = self.selectedSeller();
        if (!seller) return;
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/marketplace/admin/sellers/' + seller.id + '/payouts',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                periodStart: self.payoutForm.periodStart(),
                periodEnd: self.payoutForm.periodEnd(),
                notes: self.payoutForm.notes()
            })
        }).done(function(response) {
            toastr.success(T(response.message) || 'Hak edis olusturuldu');
            payoutModal.hide();
            self.loadAll();
        }).fail(function(xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        }).always(function() {
            self.isSaving(false);
        });
    };

    self.openPaidModal = function(payout) {
        self.selectedPayout(payout);
        self.paidForm.bankReference(payout.bankReference || '');
        self.paidForm.notes(payout.notes || '');
        paidModal.show();
    };

    self.markPayoutPaid = function() {
        var payout = self.selectedPayout();
        if (!payout) return;
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/marketplace/admin/payouts/' + payout.id + '/mark-paid',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ bankReference: self.paidForm.bankReference(), notes: self.paidForm.notes() })
        }).done(function(response) {
            toastr.success(T(response.message) || 'Odendi isaretlendi');
            paidModal.hide();
            self.loadAll();
        }).fail(function(xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        }).always(function() {
            self.isSaving(false);
        });
    };

    function buildSellerPayload(seller) {
        return {
            sellerLegalTypeId: Number(self.sellerForm.sellerLegalTypeId()),
            marketplaceEnabled: self.sellerForm.marketplaceEnabled(),
            platformCommissionRate: Number(self.sellerForm.platformCommissionRate()),
            payoutDelayDays: Number(self.sellerForm.payoutDelayDays()),
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
        sellerModal = new bootstrap.Modal(document.getElementById('sellerModal'));
        refundModal = new bootstrap.Modal(document.getElementById('refundModal'));
        payoutModal = new bootstrap.Modal(document.getElementById('payoutModal'));
        paidModal = new bootstrap.Modal(document.getElementById('paidModal'));
        self.loadAll();
    });
}

var adminMarketplaceFinanceRoot = document.getElementById('adminMarketplaceFinanceApp');
if (adminMarketplaceFinanceRoot) {
    ko.applyBindings(new AdminMarketplaceFinanceViewModel(), adminMarketplaceFinanceRoot);
}
