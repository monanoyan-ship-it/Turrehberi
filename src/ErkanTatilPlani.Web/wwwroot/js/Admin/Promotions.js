function AdminPromotionsViewModel() {
    var self = this;

    // Data
    self.promotions = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.companyFilter = ko.observable('');
    self.typeFilter = ko.observable('');
    self.statusFilter = ko.observable('');

    // Helpers
    self.getTypeBadgeClass = function (typeId) {
        var classes = { 0: 'bg-primary', 1: 'bg-info', 2: 'bg-warning text-dark', 3: 'bg-success', 4: 'bg-danger', 5: 'bg-dark', 6: 'bg-secondary' };
        return classes[typeId] || 'bg-secondary';
    };

    self.getStatusBadgeClass = function (statusId) {
        var classes = { 0: 'bg-success', 1: 'bg-secondary', 2: 'bg-warning text-dark' };
        return classes[statusId] || 'bg-secondary';
    };

    self.formatDiscount = function (discountTypeId, value) {
        if (discountTypeId === 0) return '%' + value;
        if (discountTypeId === 1) return value + ' TL';
        return 'x' + value;
    };

    self.formatDateRange = function (start, end) {
        var s = new Date(start).toLocaleDateString('tr-TR');
        var e = new Date(end).toLocaleDateString('tr-TR');
        return s + ' - ' + e;
    };

    // Load
    self.loadData = function () {
        self.isLoading(true);
        var url = apiBaseUrl + '/api/admin/promotions';
        var params = [];
        if (self.companyFilter()) params.push('companyId=' + self.companyFilter());
        if (self.typeFilter()) params.push('type=' + self.typeFilter());
        if (self.statusFilter()) params.push('status=' + self.statusFilter());
        if (params.length) url += '?' + params.join('&');

        $.ajax({
            url: url,
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('authToken') }
        }).done(function (data) {
            self.promotions(data);
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Promosyonlar yuklenemedi');
        }).always(function () {
            self.isLoading(false);
        });
    };

    // Filter subscriptions
    self.companyFilter.subscribe(function () { self.loadData(); });
    self.typeFilter.subscribe(function () { self.loadData(); });
    self.statusFilter.subscribe(function () { self.loadData(); });

    // Init
    self.loadData();
}

$(document).ready(function () {
    ko.applyBindings(new AdminPromotionsViewModel(), document.getElementById('adminPromotionsApp'));
});
