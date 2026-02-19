function CompanyBlogDetailViewModel() {
    var self = this;

    // URL'den slug'lari al
    var pathParts = window.location.pathname.split('/');
    // /Companies/{slug}/blog/{postSlug}
    self.companySlug = pathParts.length >= 5 ? decodeURIComponent(pathParts[2]) : '';
    var postSlug = pathParts.length >= 5 ? decodeURIComponent(pathParts[4]) : '';

    self.isLoading = ko.observable(true);
    self.post = ko.observable(null);
    self.comments = ko.observableArray([]);

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

    // Load post
    self.loadPost = function () {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + postSlug,
            method: 'GET'
        }).done(function (data) {
            self.post(data);
            document.title = (data.metaTitle || data.title) + ' - ' + (data.companyName || '');
            self.loadComments(data.id);
            self.isLoading(false);
        }).fail(function () {
            self.post(null);
            self.isLoading(false);
        });
    };

    // Load comments
    self.loadComments = function (postId) {
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + postId + '/comments',
            method: 'GET'
        }).done(function (data) {
            self.comments(data);
        });
    };

    $(document).ready(function () {
        if (postSlug) {
            self.loadPost();
        } else {
            self.isLoading(false);
        }
    });
}

ko.applyBindings(new CompanyBlogDetailViewModel(), document.getElementById('companyBlogDetailApp'));
