function StoryViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.notFound = ko.observable(false);
    self.isLoggedIn = ko.observable(!!localStorage.getItem('token'));
    self.isAddingComment = ko.observable(false);

    // Story Data
    self.title = ko.observable('');
    self.content = ko.observable('');
    self.rating = ko.observable(null);
    self.travelDate = ko.observable(null);
    self.destination = ko.observable('');
    self.tourName = ko.observable('');
    self.likeCount = ko.observable(0);
    self.commentCount = ko.observable(0);
    self.isLiked = ko.observable(false);
    self.createdAtFormatted = ko.observable('');
    self.photos = ko.observableArray([]);

    // Author
    self.authorId = ko.observable(0);
    self.authorName = ko.observable('');
    self.authorAvatarUrl = ko.observable('');
    self.authorTierNameKey = ko.observable('');
    self.authorTierCssClass = ko.observable('bg-secondary');

    // Comments
    self.comments = ko.observableArray([]);
    self.newComment = ko.observable('');

    // Helper
    self.T = function (key) {
        return typeof T === 'function' ? T(key) : key;
    };

    // Load story
    self.loadStory = function () {
        var token = localStorage.getItem('token');
        var headers = token ? { 'Authorization': 'Bearer ' + token } : {};

        $.ajax({
            url: apiBaseUrl + '/api/trip-stories/' + storyId,
            method: 'GET',
            headers: headers
        }).done(function (data) {
            if (data.error) {
                self.notFound(true);
                self.isLoading(false);
                return;
            }

            self.title(data.title);
            self.content(data.content);
            self.rating(data.rating);
            self.travelDate(data.travelDate);
            self.destination(data.destination || '');
            self.tourName(data.tourName || '');
            self.likeCount(data.likeCount || 0);
            self.commentCount(data.commentCount || 0);
            self.isLiked(data.isLiked || false);
            self.createdAtFormatted(new Date(data.createdAt).toLocaleDateString());
            self.photos(data.photos || []);

            if (data.visitor) {
                self.authorId(data.visitor.id);
                self.authorName(data.visitor.firstName + ' ' + data.visitor.lastName);
                self.authorAvatarUrl(data.visitor.avatarUrl || '');
                self.authorTierNameKey(data.visitor.tierNameKey || '');
                self.authorTierCssClass(data.visitor.tierCssClass || 'bg-secondary');
            }

            self.isLoading(false);
            self.loadComments();
        }).fail(function () {
            self.notFound(true);
            self.isLoading(false);
        });
    };

    // Toggle like
    self.toggleLike = function () {
        var token = localStorage.getItem('token');
        if (!token) {
            toastr.warning(self.T('Profile.LoginRequired'));
            return;
        }

        $.ajax({
            url: apiBaseUrl + '/api/trip-stories/' + storyId + '/like',
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token }
        }).done(function (data) {
            if (data.message === 'Success.Liked') {
                self.isLiked(true);
                self.likeCount(self.likeCount() + 1);
            } else {
                self.isLiked(false);
                self.likeCount(Math.max(0, self.likeCount() - 1));
            }
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.error || self.T('Common.Error'));
        });
    };

    // Load comments
    self.loadComments = function () {
        $.ajax({
            url: apiBaseUrl + '/api/trip-stories/' + storyId + '/comments?page=1&pageSize=50',
            method: 'GET'
        }).done(function (data) {
            self.comments(data.items || []);
        });
    };

    // Add comment
    self.addComment = function () {
        var token = localStorage.getItem('token');
        if (!token) {
            toastr.warning(self.T('Profile.LoginRequired'));
            return;
        }
        if (!self.newComment().trim()) return;

        self.isAddingComment(true);

        $.ajax({
            url: apiBaseUrl + '/api/trip-stories/' + storyId + '/comments',
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token },
            contentType: 'application/json',
            data: JSON.stringify({ content: self.newComment().trim() })
        }).done(function () {
            self.newComment('');
            self.commentCount(self.commentCount() + 1);
            self.loadComments();
            self.isAddingComment(false);
            toastr.success(self.T('Success.CommentAdded') || 'Yorum eklendi');
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.error || self.T('Common.Error'));
            self.isAddingComment(false);
        });
    };

    // Init
    if (typeof storyId !== 'undefined' && storyId > 0) {
        self.loadStory();
    } else {
        self.notFound(true);
        self.isLoading(false);
    }
}

$(document).ready(function () {
    var vm = new StoryViewModel();
    ko.applyBindings(vm, document.getElementById('storyApp'));
});
