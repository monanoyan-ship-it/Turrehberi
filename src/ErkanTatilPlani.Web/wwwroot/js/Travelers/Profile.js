function TravelerProfileViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isPrivate = ko.observable(false);
    self.notFound = ko.observable(false);
    self.isFollowLoading = ko.observable(false);

    // Profile Data
    self.profileId = ko.observable(0);
    self.fullName = ko.observable('');
    self.avatarUrl = ko.observable('');
    self.bio = ko.observable('');
    self.tierNameKey = ko.observable('');
    self.tierIcon = ko.observable('bi-compass');
    self.tierCssClass = ko.observable('bg-secondary');
    self.followerCount = ko.observable(0);
    self.followingCount = ko.observable(0);
    self.tripStoryCount = ko.observable(0);
    self.completedTours = ko.observable(0);
    self.reviewCount = ko.observable(0);
    self.isFollowing = ko.observable(false);
    self.recentStories = ko.observableArray([]);

    // Computed
    self.canFollow = ko.computed(function () {
        var token = localStorage.getItem('token');
        if (!token) return false;
        try {
            var payload = JSON.parse(atob(token.split('.')[1]));
            var myId = parseInt(payload.nameid || payload.sub);
            return myId !== self.profileId();
        } catch (e) { return false; }
    });

    // Helper
    self.T = function (key) {
        return typeof T === 'function' ? T(key) : key;
    };

    // Load profile
    self.loadProfile = function () {
        var token = localStorage.getItem('token');
        var headers = token ? { 'Authorization': 'Bearer ' + token } : {};

        $.ajax({
            url: apiBaseUrl + '/api/travelers/' + travelerId,
            method: 'GET',
            headers: headers
        }).done(function (data) {
            if (data.error) {
                if (data.isPrivate) {
                    self.isPrivate(true);
                } else {
                    self.notFound(true);
                }
                self.isLoading(false);
                return;
            }

            self.profileId(data.id);
            self.fullName(data.firstName + ' ' + data.lastName);
            self.avatarUrl(data.avatarUrl || '');
            self.bio(data.bio || '');
            self.tierNameKey(data.tierNameKey || '');
            self.tierIcon(data.tierIcon || 'bi-compass');
            self.tierCssClass(data.tierCssClass || 'bg-secondary');
            self.followerCount(data.followerCount || 0);
            self.followingCount(data.followingCount || 0);
            self.tripStoryCount(data.tripStoryCount || 0);
            self.completedTours(data.completedTours || 0);
            self.reviewCount(data.reviewCount || 0);
            self.isFollowing(data.isFollowing || false);
            self.recentStories(data.recentStories || []);
            self.isLoading(false);
        }).fail(function () {
            self.notFound(true);
            self.isLoading(false);
        });
    };

    // Toggle follow
    self.toggleFollow = function () {
        var token = localStorage.getItem('token');
        if (!token) {
            toastr.warning(self.T('Profile.LoginRequired'));
            return;
        }

        self.isFollowLoading(true);
        var method = self.isFollowing() ? 'DELETE' : 'POST';

        $.ajax({
            url: apiBaseUrl + '/api/travelers/' + travelerId + '/follow',
            method: method,
            headers: { 'Authorization': 'Bearer ' + token }
        }).done(function () {
            if (self.isFollowing()) {
                self.isFollowing(false);
                self.followerCount(Math.max(0, self.followerCount() - 1));
            } else {
                self.isFollowing(true);
                self.followerCount(self.followerCount() + 1);
            }
            self.isFollowLoading(false);
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.error || self.T('Common.Error'));
            self.isFollowLoading(false);
        });
    };

    // Init
    if (typeof travelerId !== 'undefined' && travelerId > 0) {
        self.loadProfile();
    } else {
        self.notFound(true);
        self.isLoading(false);
    }
}

$(document).ready(function () {
    var vm = new TravelerProfileViewModel();
    ko.applyBindings(vm, document.getElementById('travelerProfileApp'));
});
