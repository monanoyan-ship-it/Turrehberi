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
            $.ajax({ url: apiBaseUrl + '/api/companies', method: 'GET' }),
            $.ajax({ url: apiBaseUrl + '/api/visitors', method: 'GET' })
        ).done(function(featuredRes, toursRes, companiesRes, visitorsRes) {
            self.featuredTours(featuredRes[0]);
            self.stats({
                tours: toursRes[0].length,
                companies: companiesRes[0].length,
                visitors: visitorsRes[0].length
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
