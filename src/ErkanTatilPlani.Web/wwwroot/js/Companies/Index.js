function CompaniesViewModel() {
    var self = this;

    // State
    self.companies = ko.observableArray([]);
    self.cities = ko.observableArray([]);
    self.isLoading = ko.observable(true);

    // Filters
    self.searchQuery = ko.observable('');
    self.selectedCity = ko.observable('');
    self.sortBy = ko.observable('');

    // Debounce timer
    var searchTimer = null;

    // Computed: Has filters
    self.hasFilters = ko.computed(function() {
        return self.searchQuery() || self.selectedCity() || self.sortBy();
    });

    // Firma paylas
    self.shareCompany = function(company, platform) {
        var shareUrl = window.location.origin + '/' + company.slug;
        var shareText = company.name + ' - ' + (company.tagline || 'Tur Firmasi') + ' | Erkan Tatil Plani';

        if (platform === 'copy') {
            SocialShare.copyLink(shareUrl);
        } else {
            SocialShare.share(platform, shareUrl, shareText);
        }
    };

    // Get star icons
    self.getStars = function(rating) {
        var stars = [];
        var fullStars = Math.floor(rating);
        var hasHalf = rating - fullStars >= 0.5;

        for (var i = 0; i < fullStars; i++) {
            stars.push('bi bi-star-fill');
        }
        if (hasHalf) {
            stars.push('bi bi-star-half');
        }
        while (stars.length < 5) {
            stars.push('bi bi-star');
        }
        return stars;
    };

    // Load companies
    self.loadCompanies = function() {
        self.isLoading(true);

        var params = {};
        if (self.searchQuery()) params.search = self.searchQuery();
        if (self.selectedCity()) params.city = self.selectedCity();
        if (self.sortBy()) params.sort = self.sortBy();

        $.ajax({
            url: apiBaseUrl + '/api/companies/public',
            method: 'GET',
            data: params,
            success: function(response) {
                self.companies(response.companies || []);
                self.cities(response.cities || []);
                self.isLoading(false);
            },
            error: function() {
                toastr.error(T('Common.Error'));
                self.isLoading(false);
            }
        });
    };

    // Search keyup with debounce
    self.onSearchKeyup = function() {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(function() {
            self.applyFilters();
        }, 300);
        return true;
    };

    // Apply filters
    self.applyFilters = function() {
        self.loadCompanies();
    };

    // Clear search
    self.clearSearch = function() {
        self.searchQuery('');
        self.applyFilters();
    };

    // Clear all filters
    self.clearFilters = function() {
        self.searchQuery('');
        self.selectedCity('');
        self.sortBy('');
        self.applyFilters();
    };

    // Initialize
    self.loadCompanies();
}

ko.applyBindings(new CompaniesViewModel(), document.getElementById('companiesApp'));
