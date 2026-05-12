function HomeViewModel() {
    var self = this;

    self.featuredTours = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.stats = ko.observable({ tours: 0, companies: 0, visitors: 0 });

    self.loadData = function() {
        self.isLoading(true);
        $.when(
            $.ajax({ url: apiBaseUrl + '/api/tours/featured', method: 'GET' }),
            $.ajax({ url: apiBaseUrl + '/api/tours', method: 'GET' }),
            $.ajax({ url: apiBaseUrl + '/api/companies/public', method: 'GET' })
        ).done(function(featuredRes, toursRes, companiesRes) {
            var tours = toursRes[0].tours || toursRes[0] || [];
            var companies = companiesRes[0].companies || companiesRes[0] || [];
            self.featuredTours(featuredRes[0] || []);
            self.stats({
                tours: tours.length,
                companies: companies.length,
                visitors: 0
            });
            self.isLoading(false);
        }).fail(function() {
            toastr.error('Veriler yuklenirken hata olustu');
            self.isLoading(false);
        });
    };

    $(document).ready(function() {
        self.loadData();
    });
}

ko.applyBindings(new HomeViewModel(), document.getElementById('homeApp'));
