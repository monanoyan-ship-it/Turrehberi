function BlogIndexViewModel() {
    var self = this;

    // Data
    self.posts = ko.observableArray([]);
    self.categories = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.searchQuery = ko.observable('');
    self.selectedCategory = ko.observable('');
    self.currentPage = ko.observable(1);
    self.totalPages = ko.observable(1);
    self.totalCount = ko.observable(0);

    // Computed
    self.pageNumbers = ko.computed(function () {
        var pages = [];
        var total = self.totalPages();
        var current = self.currentPage();
        var start = Math.max(1, current - 2);
        var end = Math.min(total, current + 2);
        for (var i = start; i <= end; i++) {
            pages.push(i);
        }
        return pages;
    });

    // Category helpers
    var categoryKeys = {
        0: 'Blog.Category.TravelTips',
        1: 'Blog.Category.Destinations',
        2: 'Blog.Category.News',
        3: 'Blog.Category.Culture',
        4: 'Blog.Category.FoodAndDrink',
        5: 'Blog.Category.Adventure'
    };

    var categoryBadgeClasses = {
        0: 'bg-info',
        1: 'bg-success',
        2: 'bg-primary',
        3: 'bg-warning text-dark',
        4: 'bg-danger',
        5: 'bg-dark'
    };

    self.getCategoryName = function (id) {
        return categoryKeys[id] ? T(categoryKeys[id]) : T('Blog.Category.Other');
    };

    self.getCategoryBadgeClass = function (id) {
        return categoryBadgeClasses[id] || 'bg-secondary';
    };

    self.formatDate = function (dateStr) {
        if (!dateStr) return '-';
        var d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR', { year: 'numeric', month: 'long', day: 'numeric' });
    };

    // Load data
    self.loadPosts = function () {
        self.isLoading(true);
        var params = {
            page: self.currentPage(),
            pageSize: 9
        };
        if (self.searchQuery()) params.search = self.searchQuery();
        if (self.selectedCategory()) params.categoryId = self.selectedCategory();

        $.ajax({
            url: apiBaseUrl + '/api/blogs',
            method: 'GET',
            data: params
        }).done(function (data) {
            self.posts(data.posts);
            self.totalPages(data.totalPages);
            self.totalCount(data.totalCount);
            self.isLoading(false);
        }).fail(function () {
            toastr.error(T('Error.PostsLoadFailed'));
            self.isLoading(false);
        });
    };

    self.loadCategories = function () {
        $.ajax({
            url: apiBaseUrl + '/api/blogs/categories',
            method: 'GET'
        }).done(function (data) {
            self.categories(data);
        });
    };

    // Actions
    self.clearSearch = function () {
        self.searchQuery('');
    };

    self.changePage = function (page) {
        if (page < 1 || page > self.totalPages()) return;
        self.currentPage(page);
        self.loadPosts();
        window.scrollTo(0, 0);
    };

    // Watchers
    var searchTimeout;
    self.searchQuery.subscribe(function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            self.currentPage(1);
            self.loadPosts();
        }, 300);
    });

    self.selectedCategory.subscribe(function () {
        self.currentPage(1);
        self.loadPosts();
    });

    // Init
    $(document).ready(function () {
        self.loadCategories();
        self.loadPosts();

        // categoryKeys zaten lazy T() kullaniyor, languageChanged gereksiz
    });
}

ko.applyBindings(new BlogIndexViewModel(), document.getElementById('blogApp'));
