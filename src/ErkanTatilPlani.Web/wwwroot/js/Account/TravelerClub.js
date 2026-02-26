function TravelerClubViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isLoggedIn = ko.observable(false);
    self.isLoadingStories = ko.observable(false);
    self.activeTab = ko.observable('feed');
    self.stories = ko.observableArray([]);
    self.currentPage = ko.observable(1);
    self.hasMore = ko.observable(false);

    // My Stats
    self.myFollowerCount = ko.observable(0);
    self.myFollowingCount = ko.observable(0);
    self.myStoryCount = ko.observable(0);

    // Top Travelers
    self.topTravelers = ko.observableArray([]);

    // Helper
    self.T = function (key) {
        return typeof T === 'function' ? T(key) : key;
    };

    // Switch tab
    self.switchTab = function (tab) {
        self.activeTab(tab);
        self.stories([]);
        self.currentPage(1);
        self.hasMore(false);
        self.loadStories();
    };

    // Load stories
    self.loadStories = function () {
        self.isLoadingStories(true);
        var token = localStorage.getItem('token');
        var url, headers = {};

        if (self.activeTab() === 'feed') {
            url = apiBaseUrl + '/api/trip-stories/feed?page=' + self.currentPage() + '&pageSize=10';
            headers = { 'Authorization': 'Bearer ' + token };
        } else {
            url = apiBaseUrl + '/api/trip-stories?page=' + self.currentPage() + '&pageSize=10';
        }

        $.ajax({
            url: url,
            method: 'GET',
            headers: headers
        }).done(function (data) {
            var items = data.items || [];
            if (self.currentPage() === 1) {
                self.stories(items);
            } else {
                self.stories(self.stories().concat(items));
            }
            self.hasMore(data.page < data.totalPages);
            self.isLoadingStories(false);
        }).fail(function () {
            self.isLoadingStories(false);
        });
    };

    // Load more
    self.loadMore = function () {
        self.currentPage(self.currentPage() + 1);
        self.loadStories();
    };

    // Toggle like
    self.toggleLike = function (storyId) {
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
            // Reload stories to update like count
            var story = self.stories().find(function (s) { return s.id === storyId; });
            if (story) {
                if (data.message === 'Success.Liked') {
                    story.likeCount++;
                } else {
                    story.likeCount = Math.max(0, story.likeCount - 1);
                }
                self.stories.valueHasMutated();
            }
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.error || self.T('Common.Error'));
        });
    };

    // Load my stats
    self.loadMyStats = function () {
        var token = localStorage.getItem('token');
        $.ajax({
            url: apiBaseUrl + '/api/auth/me',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + token }
        }).done(function (data) {
            self.myFollowerCount(data.followerCount || 0);
            self.myFollowingCount(data.followingCount || 0);
            self.myStoryCount(data.tripStoryCount || 0);
        });
    };

    // Load top travelers
    self.loadTopTravelers = function () {
        $.ajax({
            url: apiBaseUrl + '/api/travelers/top?limit=5',
            method: 'GET'
        }).done(function (data) {
            self.topTravelers(data || []);
        });
    };

    // Init
    self.init = function () {
        var token = localStorage.getItem('token');
        if (token) {
            self.isLoggedIn(true);
            self.loadStories();
            self.loadMyStats();
        }
        self.loadTopTravelers();
        self.isLoading(false);
    };
}

$(document).ready(function () {
    var vm = new TravelerClubViewModel();
    ko.applyBindings(vm, document.getElementById('travelerClubApp'));
    vm.init();
});
