function EmailTemplatesViewModel() {
    var self = this;

    self.templates = ko.observableArray([]);
    self.accounts = ko.observableArray([]);
    self.loading = ko.observable(true);
    self.loadingModal = ko.observable(false);
    self.saving = ko.observable(false);
    self.savingTranslation = ko.observable(false);
    self.editingTemplate = ko.observable(null);
    self.summernoteInitialized = false;

    self.supportedLanguages = [
        { code: 'tr', name: 'Turkce' },
        { code: 'en', name: 'English' },
        { code: 'ru', name: 'Русский' },
        { code: 'de', name: 'Deutsch' },
        { code: 'es', name: 'Espanol' },
        { code: 'fr', name: 'Francais' },
        { code: 'ar', name: 'العربية' },
        { code: 'fa', name: 'فارسی' },
        { code: 'pt', name: 'Portugues' }
    ];

    self.selectedLanguage = ko.observable('tr');
    self.translations = ko.observableArray([]);

    self.formData = {
        key: ko.observable(''),
        name: ko.observable(''),
        description: ko.observable(''),
        emailAccountId: ko.observable(null)
    };

    self.translationData = {
        subject: ko.observable(''),
        body: ko.observable('')
    };

    // Preview
    self.previewTemplateId = ko.observable(null);
    self.previewLanguage = ko.observable('tr');
    self.previewSubject = ko.observable('');
    self.previewBody = ko.observable('');
    self.loadingPreview = ko.observable(false);

    self.initSummernote = function () {
        if (self.summernoteInitialized) {
            $('#emailBodyEditor').summernote('destroy');
        }
        $('#emailBodyEditor').summernote({
            height: 350,
            placeholder: 'Email HTML icerigi...',
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'italic', 'underline', 'clear']],
                ['fontsize', ['fontsize']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['table', ['table']],
                ['insert', ['link', 'picture']],
                ['view', ['codeview', 'fullscreen', 'help']]
            ],
            callbacks: {
                onChange: function (contents) {
                    self.translationData.body(contents);
                }
            }
        });
        self.summernoteInitialized = true;
    };

    self.loadTemplates = function () {
        self.loading(true);
        $.get(apiBaseUrl + '/api/admin/email-templates')
            .done(function (data) {
                self.templates(data);
            })
            .fail(function () {
                toastr.error('Sablonlar yuklenemedi');
            })
            .always(function () {
                self.loading(false);
            });
    };

    self.loadAccounts = function () {
        $.get(apiBaseUrl + '/api/admin/email-accounts')
            .done(function (data) {
                self.accounts(data);
            });
    };

    self.hasTranslation = function (langCode) {
        return self.translations().some(function (t) { return t.languageCode === langCode; });
    };

    self.selectLanguage = function (lang) {
        if (!lang || !lang.code) return;

        self.selectedLanguage(lang.code);

        var translation = self.translations().find(function (t) { return t.languageCode === lang.code; });
        if (translation) {
            self.translationData.subject(translation.subject);
            self.translationData.body(translation.body);
            if (self.summernoteInitialized) {
                $('#emailBodyEditor').summernote('code', translation.body);
            }
        } else {
            self.translationData.subject('');
            self.translationData.body('');
            if (self.summernoteInitialized) {
                $('#emailBodyEditor').summernote('code', '');
            }
        }
    };

    self.openCreateModal = function () {
        self.editingTemplate(null);
        self.resetForm();
        self.translations([]);
        self.selectedLanguage('tr');
        self.translationData.subject('');
        self.translationData.body('');
        self.loadingModal(false);

        setTimeout(function () {
            self.initSummernote();
            $('#emailBodyEditor').summernote('code', '');
        }, 300);
    };

    self.openEditModal = function (template) {
        self.loadingModal(true);
        $('#templateModal').modal('show');

        $.get(apiBaseUrl + '/api/admin/email-templates/' + template.id)
            .done(function (data) {
                self.editingTemplate(data);
                self.formData.key(data.key);
                self.formData.name(data.name);
                self.formData.description(data.description || '');
                self.formData.emailAccountId(data.emailAccountId);
                self.translations(data.translations || []);

                var initialBody = '';
                var trTranslation = data.translations.find(function (t) { return t.languageCode === 'tr'; });
                if (trTranslation) {
                    self.selectedLanguage('tr');
                    self.translationData.subject(trTranslation.subject);
                    self.translationData.body(trTranslation.body);
                    initialBody = trTranslation.body;
                } else if (data.translations.length > 0) {
                    var first = data.translations[0];
                    self.selectedLanguage(first.languageCode);
                    self.translationData.subject(first.subject);
                    self.translationData.body(first.body);
                    initialBody = first.body;
                } else {
                    self.selectedLanguage('tr');
                    self.translationData.subject('');
                    self.translationData.body('');
                }

                self.loadingModal(false);

                setTimeout(function () {
                    self.initSummernote();
                    $('#emailBodyEditor').summernote('code', initialBody);
                }, 300);
            })
            .fail(function () {
                toastr.error('Sablon detaylari yuklenemedi');
                $('#templateModal').modal('hide');
                self.loadingModal(false);
            });
    };

    self.resetForm = function () {
        self.formData.key('');
        self.formData.name('');
        self.formData.description('');
        self.formData.emailAccountId(null);
    };

    self.insertPlaceholder = function (placeholder) {
        if (self.summernoteInitialized) {
            $('#emailBodyEditor').summernote('insertText', placeholder);
            self.translationData.body($('#emailBodyEditor').summernote('code'));
        }
        toastr.info(placeholder + ' eklendi', '', { timeOut: 1000 });
        return false;
    };

    self.saveTemplate = function () {
        if (!self.formData.key() || !self.formData.name()) {
            toastr.warning('Key ve Sablon Adi zorunludur');
            return;
        }

        self.saving(true);
        $.ajax({
            url: apiBaseUrl + '/api/admin/email-templates',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                key: self.formData.key(),
                name: self.formData.name(),
                description: self.formData.description(),
                emailAccountId: self.formData.emailAccountId() || null,
                placeholders: []
            })
        })
            .done(function (newTemplate) {
                toastr.success('Sablon olusturuldu');
                self.editingTemplate(newTemplate);
                self.translations([]);
                self.loadTemplates();
            })
            .fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Islem basarisiz');
            })
            .always(function () {
                self.saving(false);
            });
    };

    self.updateTemplate = function () {
        if (!self.formData.name()) {
            toastr.warning('Sablon adi zorunludur');
            return;
        }

        self.saving(true);
        $.ajax({
            url: apiBaseUrl + '/api/admin/email-templates/' + self.editingTemplate().id,
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({
                name: self.formData.name(),
                description: self.formData.description(),
                emailAccountId: self.formData.emailAccountId() || null,
                placeholders: []
            })
        })
            .done(function () {
                toastr.success('Sablon guncellendi');
                self.loadTemplates();
            })
            .fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Islem basarisiz');
            })
            .always(function () {
                self.saving(false);
            });
    };

    self.saveTranslation = function () {
        if (!self.editingTemplate()) {
            toastr.warning('Once sablonu kaydedin');
            return;
        }

        var subject = self.translationData.subject();
        var body = self.summernoteInitialized ? $('#emailBodyEditor').summernote('code') : self.translationData.body();

        if (!subject || !body) {
            toastr.warning('Konu ve icerik zorunludur');
            return;
        }

        self.savingTranslation(true);
        $.ajax({
            url: apiBaseUrl + '/api/admin/email-templates/' + self.editingTemplate().id + '/translations/' + self.selectedLanguage(),
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ subject: subject, body: body })
        })
            .done(function () {
                toastr.success(self.selectedLanguage().toUpperCase() + ' cevirisi kaydedildi');

                var existing = self.translations().find(function (t) { return t.languageCode === self.selectedLanguage(); });
                if (existing) {
                    existing.subject = subject;
                    existing.body = body;
                } else {
                    self.translations.push({
                        languageCode: self.selectedLanguage(),
                        subject: subject,
                        body: body
                    });
                }
                self.translations.valueHasMutated();
                self.loadTemplates();
            })
            .fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Kaydetme basarisiz');
            })
            .always(function () {
                self.savingTranslation(false);
            });
    };

    self.deleteTranslation = function () {
        if (!confirm(self.selectedLanguage().toUpperCase() + ' cevirisini silmek istediginizden emin misiniz?')) return;

        $.ajax({
            url: apiBaseUrl + '/api/admin/email-templates/' + self.editingTemplate().id + '/translations/' + self.selectedLanguage(),
            method: 'DELETE'
        })
            .done(function () {
                toastr.success('Ceviri silindi');
                self.translations.remove(function (t) { return t.languageCode === self.selectedLanguage(); });
                self.translationData.subject('');
                self.translationData.body('');
                if (self.summernoteInitialized) {
                    $('#emailBodyEditor').summernote('code', '');
                }
                self.loadTemplates();
            })
            .fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Silme basarisiz');
            });
    };

    self.deleteTemplate = function (template) {
        if (!confirm('Bu sablonu silmek istediginizden emin misiniz?')) return;

        $.ajax({
            url: apiBaseUrl + '/api/admin/email-templates/' + template.id,
            method: 'DELETE'
        })
            .done(function () {
                toastr.success('Sablon silindi');
                self.loadTemplates();
            })
            .fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Silme basarisiz');
            });
    };

    self.openPreviewModal = function (template) {
        self.previewTemplateId(template.id);
        self.previewLanguage('tr');
        self.previewSubject('');
        self.previewBody('');
        $('#previewModal').modal('show');
        self.loadPreview();
    };

    self.loadPreview = function () {
        if (!self.previewTemplateId()) return;

        self.loadingPreview(true);
        $.ajax({
            url: apiBaseUrl + '/api/admin/email-templates/' + self.previewTemplateId() + '/preview',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ languageCode: self.previewLanguage() })
        })
            .done(function (data) {
                self.previewSubject(data.subject);
                self.previewBody(data.body);

                var iframe = document.getElementById('previewFrame');
                var doc = iframe.contentDocument || iframe.contentWindow.document;
                doc.open();
                doc.write(data.body);
                doc.close();
            })
            .fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Onizleme yuklenemedi');
            })
            .always(function () {
                self.loadingPreview(false);
            });
    };

    // Init
    self.loadTemplates();
    self.loadAccounts();
}

$(document).ready(function () {
    ko.applyBindings(new EmailTemplatesViewModel(), document.getElementById('emailTemplatesApp'));
});
