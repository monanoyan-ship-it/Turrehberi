function LanguagesViewModel() {
    var self = this;
    self.languages = ko.observableArray([]);

    self.loadData = function() {
        $.ajax({ url: apiBaseUrl + '/api/languages', method: 'GET' })
            .done(function(data) { self.languages(data); })
            .fail(function() { toastr.error('Diller yuklenemedi'); });
    };

    self.openAddModal = function() { toastr.info('Dil ekleme ozelligi yakin zamanda eklenecek'); };

    self.setDefault = function(lang) {
        $.ajax({ url: apiBaseUrl + '/api/languages/' + lang.id + '/set-default', method: 'PUT' })
            .done(function() { toastr.success(lang.name + ' varsayilan dil yapildi'); self.loadData(); })
            .fail(function() { toastr.error('Hata olustu'); });
    };

    $(document).ready(function() { self.loadData(); });
}
ko.applyBindings(new LanguagesViewModel(), document.getElementById('languagesApp'));
