function LoyaltyViewModel() {
    var self = this;

    // Observables
    self.isLoading = ko.observable(true);
    self.isLoggedIn = ko.observable(false);

    // Loyalty Dashboard
    self.tierIcon = ko.observable('bi-compass');
    self.tierNameKey = ko.observable('');
    self.tierCssClass = ko.observable('bg-secondary');
    self.discountPercent = ko.observable(0);
    self.creditEarnPercent = ko.observable(0);
    self.creditBalance = ko.observable(0);
    self.completedTours = ko.observable(0);
    self.nextTierNameKey = ko.observable('');
    self.nextTierMinTours = ko.observable(0);
    self.progressPercent = ko.observable(0);

    // Referral
    self.referralCode = ko.observable('');
    self.totalReferrals = ko.observable(0);
    self.completedReferrals = ko.observable(0);
    self.totalEarned = ko.observable(0);
    self.referrals = ko.observableArray([]);

    // Credit History
    self.creditHistory = ko.observableArray([]);
    self.currentPage = ko.observable(1);
    self.totalPages = ko.observable(1);

    // Helper
    self.T = function (key) {
        return typeof T === 'function' ? T(key) : key;
    };

    // Load loyalty dashboard
    self.loadDashboard = function () {
        $.ajax({
            url: apiBaseUrl + '/api/loyalty/dashboard',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
        }).done(function (data) {
            self.tierIcon(data.tierIcon || 'bi-compass');
            self.tierNameKey(data.tierNameKey || '');
            self.tierCssClass(data.tierCssClass || 'bg-secondary');
            self.discountPercent(data.discountPercent || 0);
            self.creditEarnPercent(data.creditEarnPercent || 0);
            self.creditBalance(data.creditBalance || 0);
            self.completedTours(data.completedTours || 0);
            self.nextTierNameKey(data.nextTierNameKey || '');
            self.nextTierMinTours(data.nextTierMinTours || 0);
            self.progressPercent(data.progressPercent || 0);
        }).fail(function () {
            toastr.error(self.T('Error.LoadFailed'));
        });
    };

    // Load referral dashboard
    self.loadReferrals = function () {
        $.ajax({
            url: apiBaseUrl + '/api/referrals/dashboard',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
        }).done(function (data) {
            self.referralCode(data.referralCode || '');
            self.totalReferrals(data.totalReferrals || 0);
            self.completedReferrals(data.completedReferrals || 0);
            self.totalEarned(data.totalEarned || 0);
            self.referrals(data.referrals || []);
        }).fail(function () {
            toastr.error(self.T('Error.LoadFailed'));
        });
    };

    // Load credit history
    self.loadCredits = function (page) {
        page = page || 1;
        $.ajax({
            url: apiBaseUrl + '/api/loyalty/credits?page=' + page + '&pageSize=20',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
        }).done(function (data) {
            self.creditHistory(data.items || []);
            self.currentPage(data.page || 1);
            self.totalPages(data.totalPages || 1);
        }).fail(function () {
            toastr.error(self.T('Error.LoadFailed'));
        });
    };

    // Copy referral code
    self.copyReferralCode = function () {
        if (navigator.clipboard && self.referralCode()) {
            navigator.clipboard.writeText(self.referralCode()).then(function () {
                toastr.success(self.T('Referral.CodeCopied'));
            });
        }
    };

    // Init
    self.init = function () {
        var token = localStorage.getItem('token');
        if (token) {
            self.isLoggedIn(true);
            self.loadDashboard();
            self.loadReferrals();
            self.loadCredits(1);
        }
        self.isLoading(false);
    };
}

$(document).ready(function () {
    var vm = new LoyaltyViewModel();
    ko.applyBindings(vm, document.getElementById('loyaltyApp'));
    vm.init();
});
