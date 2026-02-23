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
        0: T('Blog.Category.TravelTips'), 1: T('Blog.Category.Destinations'), 2: T('Blog.Category.News'),
        3: T('Blog.Category.Culture'), 4: T('Blog.Category.FoodAndDrink'), 5: T('Blog.Category.Adventure')
    };
    var categoryBadgeClasses = {
        0: 'bg-info', 1: 'bg-success', 2: 'bg-primary',
        3: 'bg-warning text-dark', 4: 'bg-danger', 5: 'bg-dark'
    };

    self.getCategoryName = function (id) {
        return categoryNames[id] || T('Blog.Category.Other');
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
            toastr.success(T('Success.CommentAdded'));
            self.newComment('');
            self.loadComments();
            self.isSaving(false);
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
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
            toastr.success(T('Success.ReplyAdded'));
            self.replyContent('');
            self.replyingTo(null);
            self.loadComments();
            self.isSaving(false);
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
            self.isSaving(false);
        });
    };

    // Delete comment
    self.deleteComment = function (commentId) {
        if (!confirm(T('Confirm.DeleteComment'))) return;
        $.ajax({
            url: apiBaseUrl + '/api/blogs/comments/' + commentId,
            method: 'DELETE'
        }).done(function () {
            toastr.success(T('Success.CommentDeleted'));
            self.loadComments();
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
        });
    };

    // Init
    $(document).ready(function () {
        self.loadPost();
    });
}

ko.applyBindings(new BlogDetailViewModel(), document.getElementById('blogDetailApp'));
