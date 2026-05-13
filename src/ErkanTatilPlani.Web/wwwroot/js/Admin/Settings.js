function SettingsViewModel() {
    var self = this;

    function createEmptyPaymentMethod() {
        return {
            id: 0,
            systemName: '',
            displayName: '',
            description: '',
            providerSystemName: 'iyzico',
            providerDisplayName: 'Iyzico',
            isEnabled: true,
            isDefault: false,
            isOnline: true,
            supportsMarketplaceSplit: true,
            displayOrder: 1,
            iconClass: 'bi bi-credit-card-2-front',
            apiKey: '',
            secretKey: '',
            baseUrl: 'https://sandbox-api.iyzipay.com',
            isSandbox: true,
            extraSettingsJson: ''
        };
    }

    self.languages = ko.observableArray([]);
    self.defaultLanguage = ko.observable(1);
    self.selectedLanguageForImport = ko.observable(1);
    self.selectedLanguageForExport = ko.observable(1);
    self.isImporting = ko.observable(false);

    self.paymentMethods = ko.observableArray([]);
    self.paymentMethodFormData = ko.observable(createEmptyPaymentMethod());
    self.selectedPaymentMethod = ko.observable(null);
    self.isEditingPaymentMethod = ko.observable(false);
    self.isSavingPaymentMethod = ko.observable(false);

    var paymentMethodModal;

    function errorTextFromXhr(xhr, fallbackKey) {
        var key = xhr && xhr.responseJSON && typeof xhr.responseJSON.message === 'string'
            ? xhr.responseJSON.message
            : '';
        return key ? T(key) : T(fallbackKey);
    }

    self.getMethodStatusClass = function(method) {
        if (!method.isEnabled) return 'bg-secondary';
        if (!method.isOnline) return 'bg-warning text-dark';
        return 'bg-success';
    };

    self.getMethodStatusText = function(method) {
        if (!method.isEnabled) return 'Pasif';
        if (!method.isOnline) return 'Manuel';
        return 'Aktif';
    };

    self.maskSecret = function(value) {
        if (!value) return '-';
        if (value.length <= 8) return '********';
        return value.substring(0, 4) + '****' + value.substring(value.length - 2);
    };

    self.loadLanguages = function() {
        $.ajax({
            url: apiBaseUrl + '/api/languages',
            method: 'GET',
            success: function(data) {
                self.languages(data);
                var defaultLang = data.find(function(language) { return language.isDefault; });
                if (defaultLang) {
                    self.defaultLanguage(defaultLang.id);
                }
            },
            error: function() {
                self.languages([
                    { id: 1, name: 'Turkce', languageCulture: 'tr-TR', uniqueSeoCode: 'tr', flagIcon: 'fi fi-tr', isDefault: true, isActive: true, resourceCount: 0 },
                    { id: 2, name: 'English', languageCulture: 'en-US', uniqueSeoCode: 'en', flagIcon: 'fi fi-us', isDefault: false, isActive: true, resourceCount: 0 },
                    { id: 3, name: 'Deutsch', languageCulture: 'de-DE', uniqueSeoCode: 'de', flagIcon: 'fi fi-de', isDefault: false, isActive: true, resourceCount: 0 },
                    { id: 4, name: 'Russian', languageCulture: 'ru-RU', uniqueSeoCode: 'ru', flagIcon: 'fi fi-ru', isDefault: false, isActive: true, resourceCount: 0 },
                    { id: 5, name: 'Espanol', languageCulture: 'es-ES', uniqueSeoCode: 'es', flagIcon: 'fi fi-es', isDefault: false, isActive: true, resourceCount: 0 }
                ]);
            }
        });
    };

    self.importLanguage = function() {
        var fileInput = document.getElementById('languageFile');
        if (!fileInput.files || fileInput.files.length === 0) {
            toastr.warning(T('Validation.PleaseSelectFile'));
            return;
        }

        var file = fileInput.files[0];
        var languageId = self.selectedLanguageForImport();
        self.isImporting(true);

        var formData = new FormData();
        formData.append('file', file);
        formData.append('languageId', languageId);

        $.ajax({
            url: apiBaseUrl + '/api/languages/' + languageId + '/import',
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function() {
                toastr.success(T('Success.LanguageFileUploaded'));
                self.loadLanguages();
                fileInput.value = '';
                self.isImporting(false);
            },
            error: function() {
                toastr.error(T('Error.LanguageFileUploadFailed'));
                self.isImporting(false);
            }
        });
    };

    self.exportLanguage = function() {
        var languageId = self.selectedLanguageForExport();
        window.location.href = apiBaseUrl + '/api/languages/' + languageId + '/export';
    };

    self.openAddLanguageModal = function() {
        toastr.info(T('Info.FeatureComingSoon'));
    };

    self.editLanguage = function() {
        toastr.info(T('Info.FeatureComingSoon'));
    };

    self.loadPaymentMethods = function() {
        $.ajax({
            url: apiBaseUrl + '/api/paymentmethods/admin',
            method: 'GET'
        }).done(function(data) {
            self.paymentMethods(Array.isArray(data) ? data : []);
        }).fail(function(xhr) {
            toastr.error(errorTextFromXhr(xhr, 'Error.DataLoadFailed'));
        });
    };

    self.openCreatePaymentMethodModal = function() {
        self.isEditingPaymentMethod(false);
        self.selectedPaymentMethod(null);
        self.paymentMethodFormData(createEmptyPaymentMethod());
        paymentMethodModal.show();
    };

    self.openEditPaymentMethodModal = function(method) {
        self.isEditingPaymentMethod(true);
        self.selectedPaymentMethod(method);
        self.paymentMethodFormData({
            id: method.id,
            systemName: method.systemName || '',
            displayName: method.displayName || '',
            description: method.description || '',
            providerSystemName: method.providerSystemName || '',
            providerDisplayName: method.providerDisplayName || '',
            isEnabled: !!method.isEnabled,
            isDefault: !!method.isDefault,
            isOnline: !!method.isOnline,
            supportsMarketplaceSplit: !!method.supportsMarketplaceSplit,
            displayOrder: method.displayOrder || 1,
            iconClass: method.iconClass || 'bi bi-credit-card-2-front',
            apiKey: method.apiKey || '',
            secretKey: method.secretKey || '',
            baseUrl: method.baseUrl || '',
            isSandbox: !!method.isSandbox,
            extraSettingsJson: method.extraSettingsJson || ''
        });
        paymentMethodModal.show();
    };

    self.setDefaultPaymentMethod = function(method) {
        $.ajax({
            url: apiBaseUrl + '/api/paymentmethods/' + method.id + '/set-default',
            method: 'POST'
        }).done(function(response) {
            toastr.success(T(response.message) || T('Success.Updated'));
            self.loadPaymentMethods();
        }).fail(function(xhr) {
            toastr.error(errorTextFromXhr(xhr, 'Common.Error'));
        });
    };

    self.savePaymentMethod = function() {
        var formData = self.paymentMethodFormData();
        if (!formData.systemName || !formData.displayName || !formData.providerSystemName) {
            toastr.warning('Lutfen zorunlu alanlari doldurun');
            return;
        }

        var isEdit = self.isEditingPaymentMethod();
        var method = isEdit ? 'PUT' : 'POST';
        var url = isEdit
            ? apiBaseUrl + '/api/paymentmethods/' + formData.id
            : apiBaseUrl + '/api/paymentmethods';

        self.isSavingPaymentMethod(true);
        $.ajax({
            url: url,
            method: method,
            contentType: 'application/json',
            data: JSON.stringify(formData)
        }).done(function(response) {
            toastr.success(T(response.message) || T('Success.Updated'));
            paymentMethodModal.hide();
            self.loadPaymentMethods();
            self.isSavingPaymentMethod(false);
        }).fail(function(xhr) {
            toastr.error(errorTextFromXhr(xhr, 'Common.Error'));
            self.isSavingPaymentMethod(false);
        });
    };

    self.loadLanguages();
    self.loadPaymentMethods();

    $(document).ready(function() {
        var modalElement = document.getElementById('paymentMethodModal');
        paymentMethodModal = modalElement ? new bootstrap.Modal(modalElement) : null;
    });
}

ko.applyBindings(new SettingsViewModel(), document.getElementById('settingsApp'));
