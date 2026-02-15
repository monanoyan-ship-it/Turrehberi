function BlogDetailViewModel() {
    var self = this;

    // Data
    self.post = ko.observable(null);
    self.comments = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.newComment = ko.observable('');
    self.replyingTo = ko.observable(null);
    self.replyContent = ko.observable('');

    // Auth
    self.isLoggedIn = ko.observable(!!getToken());

    // Category helpers
    var categoryNames = {
        0: 'Seyahat Ipuclari', 1: 'Gezi Rehberleri', 2: 'Haberler',
        3: 'Kultur & Yasam', 4: 'Yeme & Icme', 5: 'Macera & Doga'
    };
    var categoryBadgeClasses = {
        0: 'bg-info', 1: 'bg-success', 2: 'bg-primary',
        3: 'bg-warning text-dark', 4: 'bg-danger', 5: 'bg-dark'
    };

    self.getCategoryName = function (id) {
        return categoryNames[id] || 'Diger';
    };

    self.getCategoryBadgeClass = function (id) {
        return categoryBadgeClasses[id] || 'bg-secondary';
    };

    self.formatDate = function (dateStr) {
        if (!dateStr) return '-';
        var d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR', { year: 'numeric', month: 'long', day: 'numeric' });
    };

    self.canDeleteComment = function (visitorId) {
        var user = getUser();
        if (!user) return false;
        return user.id === visitorId || user.userTypeId >= 2;
    };

    // Load post
    self.loadPost = function () {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + blogSlug,
            method: 'GET'
        }).done(function (data) {
            self.post(data);
            self.isLoading(false);
            self.loadComments();
            // Render share buttons
            if (typeof SocialShare !== 'undefined') {
                $('#shareButtons').html(SocialShare.renderButtons({
                    url: window.location.href,
                    text: data.title
                }));
            }
        }).fail(function () {
            self.isLoading(false);
        });
    };

    // Load comments
    self.loadComments = function () {
        var p = self.post();
        if (!p) return;
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + p.id + '/comments',
            method: 'GET'
        }).done(function (data) {
            self.comments(data);
        });
    };

    // Add comment
    self.addComment = function () {
        if (!self.newComment()) return;
        self.isSaving(true);
        var p = self.post();
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + p.id + '/comments',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ content: self.newComment(), parentCommentId: null })
        }).done(function () {
            toastr.success('Yorum eklendi');
            self.newComment('');
            self.loadComments();
            self.isSaving(false);
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Yorum eklenemedi');
            self.isSaving(false);
        });
    };

    // Reply
    self.toggleReply = function (commentId) {
        if (self.replyingTo() === commentId) {
            self.replyingTo(null);
        } else {
            self.replyingTo(commentId);
            self.replyContent('');
        }
    };

    self.addReply = function (parentCommentId) {
        if (!self.replyContent()) return;
        self.isSaving(true);
        var p = self.post();
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + p.id + '/comments',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ content: self.replyContent(), parentCommentId: parentCommentId })
        }).done(function () {
            toastr.success('Yanit eklendi');
            self.replyContent('');
            self.replyingTo(null);
            self.loadComments();
            self.isSaving(false);
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Yanit eklenemedi');
            self.isSaving(false);
        });
    };

    // Delete comment
    self.deleteComment = function (commentId) {
        if (!confirm('Bu yorumu silmek istediginizden emin misiniz?')) return;
        $.ajax({
            url: apiBaseUrl + '/api/blogs/comments/' + commentId,
            method: 'DELETE'
        }).done(function () {
            toastr.success('Yorum silindi');
            self.loadComments();
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Yorum silinemedi');
        });
    };

    // Init
    $(document).ready(function () {
        self.loadPost();
    });
}

ko.applyBindings(new BlogDetailViewModel(), document.getElementById('blogDetailApp'));
