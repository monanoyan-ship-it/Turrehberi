function PromotionsViewModel() {
    var self = this;

    // Data
    self.promotions = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isEditing = ko.observable(false);
    self.editingId = ko.observable(null);
    self.typeFilter = ko.observable('');
    self.statusFilter = ko.observable('');

    // Helpers
    self.getEmptyForm = function () {
        return {
            name: ko.observable(''),
            description: ko.observable(''),
            promotionTypeId: ko.observable('0'),
            discountTypeId: ko.observable('0'),
            discountValue: ko.observable(0),
            startDate: ko.observable(''),
            endDate: ko.observable(''),
            code: ko.observable(''),
            usageLimit: ko.observable(null),
            usageLimitPerUser: ko.observable(null),
            minOrderAmount: ko.observable(null),
            maxDiscountAmount: ko.observable(null),
            minDaysAhead: ko.observable(null),
            maxHoursBeforeStart: ko.observable(null),
            minAvailableCapacityPercent: ko.observable(null),
            minGroupSize: ko.observable(null),
            maxGroupSize: ko.observable(null),
            flashSaleStock: ko.observable(null),
            applicableTourIds: ko.observable('')
        };
    };

    // Form
    self.formData = ko.observable(self.getEmptyForm());

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

    self.onTypeChange = function () { };

    // CRUD
    self.loadData = function () {
        self.isLoading(true);
        var url = apiBaseUrl + '/api/promotions';
        var params = [];
        if (self.typeFilter()) params.push('type=' + self.typeFilter());
        if (self.statusFilter()) params.push('status=' + self.statusFilter());
        if (params.length) url += '?' + params.join('&');

        $.ajax({
            url: url,
            method: 'GET',
        }).done(function (data) {
            self.promotions(data);
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        }).always(function () {
            self.isLoading(false);
        });
    };

    self.openAddModal = function () {
        self.isEditing(false);
        self.editingId(null);
        self.formData(self.getEmptyForm());
        formModal.show();
    };

    self.openEditModal = function (promo) {
        self.isEditing(true);
        self.editingId(promo.id);
        var form = self.getEmptyForm();
        form.name(promo.name || '');
        form.description(promo.description || '');
        form.promotionTypeId(String(promo.promotionTypeId));
        form.discountTypeId(String(promo.discountTypeId));
        form.discountValue(promo.discountValue);
        form.startDate(promo.startDate ? promo.startDate.substring(0, 16) : '');
        form.endDate(promo.endDate ? promo.endDate.substring(0, 16) : '');
        form.code(promo.code || '');
        form.usageLimit(promo.usageLimit);
        form.usageLimitPerUser(promo.usageLimitPerUser);
        form.minOrderAmount(promo.minOrderAmount);
        form.maxDiscountAmount(promo.maxDiscountAmount);
        form.minDaysAhead(promo.minDaysAhead);
        form.maxHoursBeforeStart(promo.maxHoursBeforeStart);
        form.minAvailableCapacityPercent(promo.minAvailableCapacityPercent);
        form.minGroupSize(promo.minGroupSize);
        form.maxGroupSize(promo.maxGroupSize);
        form.flashSaleStock(promo.flashSaleStock);
        form.applicableTourIds(promo.applicableTourIds || '');
        self.formData(form);
        formModal.show();
    };

    self.savePromotion = function () {
        var f = self.formData();
        var data = {
            name: f.name(),
            description: f.description(),
            promotionTypeId: parseInt(f.promotionTypeId()),
            discountTypeId: parseInt(f.discountTypeId()),
            discountValue: parseFloat(f.discountValue()) || 0,
            startDate: f.startDate(),
            endDate: f.endDate(),
            code: f.code() || null,
            usageLimit: f.usageLimit() ? parseInt(f.usageLimit()) : null,
            usageLimitPerUser: f.usageLimitPerUser() ? parseInt(f.usageLimitPerUser()) : null,
            minOrderAmount: f.minOrderAmount() ? parseFloat(f.minOrderAmount()) : null,
            maxDiscountAmount: f.maxDiscountAmount() ? parseFloat(f.maxDiscountAmount()) : null,
            minDaysAhead: f.minDaysAhead() ? parseInt(f.minDaysAhead()) : null,
            maxHoursBeforeStart: f.maxHoursBeforeStart() ? parseInt(f.maxHoursBeforeStart()) : null,
            minAvailableCapacityPercent: f.minAvailableCapacityPercent() ? parseInt(f.minAvailableCapacityPercent()) : null,
            minGroupSize: f.minGroupSize() ? parseInt(f.minGroupSize()) : null,
            maxGroupSize: f.maxGroupSize() ? parseInt(f.maxGroupSize()) : null,
            flashSaleStock: f.flashSaleStock() ? parseInt(f.flashSaleStock()) : null,
            applicableTourIds: f.applicableTourIds() || null
        };

        var url = apiBaseUrl + '/api/promotions';
        var method = 'POST';
        if (self.isEditing()) {
            url += '/' + self.editingId();
            method = 'PUT';
        }

        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(data),
        }).done(function () {
            toastr.success(self.isEditing() ? T('Success.PromotionUpdated') : T('Success.PromotionCreated'));
            formModal.hide();
            self.loadData();
            self.loadDynamicRules();
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        });
    };

    self.toggleStatus = function (promo) {
        $.ajax({
            url: apiBaseUrl + '/api/promotions/' + promo.id + '/toggle',
            method: 'PUT',
        }).done(function () {
            toastr.success(T('Success.PromotionStatusChanged'));
            self.loadData();
            self.loadDynamicRules();
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        });
    };

    self.deletePromotion = function (promo) {
        if (!confirm(T('Confirm.DeletePromotion'))) return;
        $.ajax({
            url: apiBaseUrl + '/api/promotions/' + promo.id,
            method: 'DELETE',
        }).done(function () {
            toastr.success(T('Success.PromotionDeleted'));
            self.loadData();
            self.loadDynamicRules();
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        });
    };

    // Dynamic pricing rules (promotionTypeId === 6)
    self.dynamicRules = ko.observableArray([]);

    self.loadDynamicRules = function () {
        $.ajax({
            url: apiBaseUrl + '/api/promotions?type=6',
            method: 'GET',
        }).done(function (data) {
            self.dynamicRules(data);
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        });
    };

    self.openDynamicRuleModal = function () {
        self.isEditing(false);
        self.editingId(null);
        var form = self.getEmptyForm();
        form.promotionTypeId('6');
        form.discountTypeId('0');
        self.formData(form);
        formModal.show();
    };

    // Filter subscriptions
    self.typeFilter.subscribe(function () { self.loadData(); });
    self.statusFilter.subscribe(function () { self.loadData(); });

    // Modals
    var formModal = null;

    // Init
    $(document).ready(function () {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        self.loadData();
        self.loadDynamicRules();
    });
}

$(document).ready(function () {
    ko.applyBindings(new PromotionsViewModel(), document.getElementById('promotionsApp'));
});
