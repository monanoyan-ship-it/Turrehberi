function LanguagesViewModel() {
    var self = this;
    self.languages = ko.observableArray([]);

    self.loadData = function() {
        $.ajax({ url: apiBaseUrl + '/api/languages', method: 'GET' })
            .done(function(data) { self.languages(data); })
            .fail(function() { toastr.error(T('Error.LanguagesLoadFailed')); });
    };

    self.openAddModal = function() { toastr.info(T('Info.FeatureComingSoon')); };

    self.setDefault = function(lang) {
        $.ajax({ url: apiBaseUrl + '/api/languages/' + lang.id + '/set-default', method: 'PUT' })
            .done(function() { toastr.success(T('Success.DefaultLanguageSet')); self.loadData(); })
            .fail(function() { toastr.error(T('Common.Error')); });
    };

    $(document).ready(function() { self.loadData(); });
}
ko.applyBindings(new LanguagesViewModel(), document.getElementById('languagesApp'));
