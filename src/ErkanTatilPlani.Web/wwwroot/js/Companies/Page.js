function CompanyPageViewModel() {
    var self = this;

    self.companySlug = '';
    self.pageSlug = '';

    self.isLoading = ko.observable(true);
    self.page = ko.observable(null);
    self.pages = ko.observableArray([]);

    self.loadPage = function () {
        self.isLoading(true);

        // Sayfa icerigini yukle
        $.ajax({
            url: apiBaseUrl + '/api/companies/pages/' + self.companySlug + '/' + self.pageSlug,
            method: 'GET'
        }).done(function (data) {
            self.page(data);
            document.title = (data.metaTitle || data.title) + ' - ' + data.companyName;
            self.isLoading(false);
        }).fail(function () {
            self.page(null);
            self.isLoading(false);
        });

        // Diger sayfalari yukle
        $.ajax({
            url: apiBaseUrl + '/api/companies/pages/' + self.companySlug,
            method: 'GET'
        }).done(function (data) {
            self.pages(data);
        });
    };

    $(document).ready(function () {
        // companySlug ve pageSlug'u DOM'dan al
        var pathParts = window.location.pathname.split('/');
        // /Companies/{slug}/page/{pageSlug}
        if (pathParts.length >= 5) {
            self.companySlug = decodeURIComponent(pathParts[2]);
            self.pageSlug = decodeURIComponent(pathParts[4]);
        }

        if (self.companySlug && self.pageSlug) {
            self.loadPage();
        } else {
            self.isLoading(false);
        }
    });
}

ko.applyBindings(new CompanyPageViewModel(), document.getElementById('companyPageApp'));
