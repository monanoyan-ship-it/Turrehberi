function SettingsViewModel() {
    var self = this;

    self.languages = ko.observableArray([]);
    self.defaultLanguage = ko.observable(1);
    self.selectedLanguageForImport = ko.observable(1);
    self.selectedLanguageForExport = ko.observable(1);
    self.isImporting = ko.observable(false);

    self.loadLanguages = function() {
        $.ajax({
            url: apiBaseUrl + '/api/languages',
            method: 'GET',
            success: function(data) {
                self.languages(data);
                var defaultLang = data.find(function(l) { return l.isDefault; });
                if (defaultLang) {
                    self.defaultLanguage(defaultLang.id);
                }
            },
            error: function() {
                // API henuz hazir degil, simdilik statik data
                self.languages([
                    { id: 1, name: 'Turkce', languageCulture: 'tr-TR', uniqueSeoCode: 'tr', flagIcon: 'fi fi-tr', isDefault: true, isActive: true, resourceCount: 0 },
                    { id: 2, name: 'English', languageCulture: 'en-US', uniqueSeoCode: 'en', flagIcon: 'fi fi-us', isDefault: false, isActive: true, resourceCount: 0 },
                    { id: 3, name: 'Deutsch', languageCulture: 'de-DE', uniqueSeoCode: 'de', flagIcon: 'fi fi-de', isDefault: false, isActive: true, resourceCount: 0 },
                    { id: 4, name: 'Русский', languageCulture: 'ru-RU', uniqueSeoCode: 'ru', flagIcon: 'fi fi-ru', isDefault: false, isActive: true, resourceCount: 0 },
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
            success: function(result) {
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

    self.editLanguage = function(language) {
        toastr.info(T('Info.FeatureComingSoon'));
    };

    self.loadLanguages();
}

ko.applyBindings(new SettingsViewModel(), document.getElementById('settingsApp'));
