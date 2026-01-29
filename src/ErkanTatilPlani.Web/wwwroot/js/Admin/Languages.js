function LanguagesViewModel() {
    var self = this;
    self.languages = ko.observableArray([]);
    self.isImporting = ko.observable(false);

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

    self.editResources = function(lang) { toastr.info('Ceviri duzenleme ozelligi yakin zamanda eklenecek'); };

    self.importFromFolder = function(lang) {
        self.isImporting(true);
        $.ajax({
            url: apiBaseUrl + '/api/languages/' + lang.id + '/import-from-folder',
            method: 'POST',
            success: function(result) {
                toastr.success(lang.name + ': ' + result.importedCount + ' yeni, ' + result.updatedCount + ' guncellendi');
                self.loadData();
                self.isImporting(false);
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || 'Yukleme hatasi';
                toastr.error(lang.name + ': ' + msg);
                self.isImporting(false);
            }
        });
    };

    self.importAll = function() {
        self.isImporting(true);
        $.ajax({
            url: apiBaseUrl + '/api/languages/import-all',
            method: 'POST',
            success: function(data) {
                var success = data.results.filter(function(r) { return r.success; }).length;
                var failed = data.results.filter(function(r) { return !r.success; }).length;
                toastr.success(success + ' dil yuklendi' + (failed > 0 ? ', ' + failed + ' basarisiz' : ''));
                self.loadData();
                self.isImporting(false);
            },
            error: function() {
                toastr.error('Toplu yukleme hatasi');
                self.isImporting(false);
            }
        });
    };

    self.exportXml = function(lang) {
        window.location.href = apiBaseUrl + '/api/languages/' + lang.id + '/export';
    };

    $(document).ready(function() { self.loadData(); });
}
ko.applyBindings(new LanguagesViewModel(), document.getElementById('languagesApp'));
