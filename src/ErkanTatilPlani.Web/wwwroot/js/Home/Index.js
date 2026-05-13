function HomeViewModel() {
    var self = this;

    self.featuredTours = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.stats = ko.observable({ tours: 0, companies: 0, visitors: 0 });
    self.userTypeId = ko.observable(null);

    self.isTravelerUser = ko.computed(function() {
        return self.userTypeId() === 0;
    });

    self.loadCurrentUser = function() {
        var user = typeof getUser === 'function' ? getUser() : null;
        var userTypeId = user && user.userTypeId !== undefined
            ? parseInt(user.userTypeId, 10)
            : NaN;
        self.userTypeId(isNaN(userTypeId) ? null : userTypeId);
    };

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
        self.loadCurrentUser();
        self.loadData();
    });
}

ko.applyBindings(new HomeViewModel(), document.getElementById('homeApp'));
