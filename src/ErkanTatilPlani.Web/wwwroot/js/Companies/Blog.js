function CompanyBlogListViewModel() {
    var self = this;

    // URL'den slug al
    var pathParts = window.location.pathname.split('/');
    self.companySlug = pathParts.length >= 4 ? decodeURIComponent(pathParts[2]) : '';

    self.isLoading = ko.observable(true);
    self.posts = ko.observableArray([]);
    self.companyInfo = ko.observable(null);
    self.currentPage = ko.observable(1);
    self.totalPages = ko.observable(1);

    // Computed
    self.pageNumbers = ko.computed(function () {
        var pages = [];
        var total = self.totalPages();
        var current = self.currentPage();
        var start = Math.max(1, current - 2);
        var end = Math.min(total, current + 2);
        for (var i = start; i <= end; i++) pages.push(i);
        return pages;
    });

    // Helpers
    var categoryNames = {
        0: 'Seyahat Ipuclari', 1: 'Gezi Rehberleri', 2: 'Haberler',
        3: 'Kultur & Yasam', 4: 'Yeme & Icme', 5: 'Macera & Doga'
    };
    var categoryBadgeClasses = {
        0: 'bg-info', 1: 'bg-success', 2: 'bg-primary',
        3: 'bg-warning text-dark', 4: 'bg-danger', 5: 'bg-dark'
    };

    self.getCategoryName = function (id) { return categoryNames[id] || 'Diger'; };
    self.getCategoryBadgeClass = function (id) { return categoryBadgeClasses[id] || 'bg-secondary'; };

    self.formatDate = function (dateStr) {
        if (!dateStr) return '';
        return new Date(dateStr).toLocaleDateString('tr-TR', { year: 'numeric', month: 'long', day: 'numeric' });
    };

    // Load company info
    self.loadCompanyInfo = function () {
        $.ajax({
            url: apiBaseUrl + '/api/companies/profile/' + self.companySlug,
            method: 'GET'
        }).done(function (data) {
            self.companyInfo(data.company);
            // companyId ile blog yazilari yukle
            self.loadPosts(data.company.id);
        }).fail(function () {
            self.isLoading(false);
        });
    };

    // Load posts
    self.loadPosts = function (companyId) {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/blogs/company/' + companyId,
            method: 'GET',
            data: { page: self.currentPage(), pageSize: 9 }
        }).done(function (data) {
            self.posts(data.posts);
            self.totalPages(data.totalPages);
            self.isLoading(false);
        }).fail(function () {
            self.isLoading(false);
        });
    };

    self.changePage = function (page) {
        if (page < 1 || page > self.totalPages()) return;
        self.currentPage(page);
        if (self.companyInfo()) {
            self.loadPosts(self.companyInfo().id);
        }
    };

    $(document).ready(function () {
        if (self.companySlug) {
            self.loadCompanyInfo();
        } else {
            self.isLoading(false);
        }
    });
}

ko.applyBindings(new CompanyBlogListViewModel(), document.getElementById('companyBlogListApp'));
