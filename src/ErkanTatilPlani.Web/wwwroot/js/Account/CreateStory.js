function CreateStoryViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isLoggedIn = ko.observable(false);
    self.isCreating = ko.observable(false);

    // Form Data
    self.title = ko.observable('');
    self.content = ko.observable('');
    self.selectedTourId = ko.observable(null);
    self.destination = ko.observable('');
    self.travelDate = ko.observable('');
    self.rating = ko.observable(null);
    self.isPublic = ko.observable(true);
    self.charCount = ko.observable(0);

    // Tours dropdown
    self.completedTours = ko.observableArray([]);

    // Helper
    self.T = function (key) {
        return typeof T === 'function' ? T(key) : key;
    };

    // Update char count
    self.updateCharCount = function () {
        self.charCount(self.content().length);
    };

    // Set rating
    self.setRating = function (val) {
        self.rating(val);
    };

    // Load completed tours for dropdown
    self.loadCompletedTours = function () {
        var token = localStorage.getItem('token');
        $.ajax({
            url: apiBaseUrl + '/api/reservations/my?status=completed',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + token }
        }).done(function (data) {
            var tours = [];
            var seen = {};
            var items = data.items || data || [];
            items.forEach(function (r) {
                if (r.tourId && !seen[r.tourId]) {
                    seen[r.tourId] = true;
                    tours.push({ id: r.tourId, name: r.tourName || ('Tur #' + r.tourId) });
                }
            });
            self.completedTours(tours);
        });
    };

    // Create story
    self.createStory = function () {
        if (!self.title().trim()) {
            toastr.error(self.T('Error.TitleRequired') || 'Baslik zorunludur');
            return;
        }
        if (!self.content().trim()) {
            toastr.error(self.T('Error.ContentRequired') || 'Icerik zorunludur');
            return;
        }

        self.isCreating(true);
        var token = localStorage.getItem('token');

        var data = {
            title: self.title().trim(),
            content: self.content().trim(),
            tourId: self.selectedTourId() ? parseInt(self.selectedTourId()) : null,
            destination: self.destination().trim() || null,
            travelDate: self.travelDate() || null,
            rating: self.rating(),
            isPublic: self.isPublic()
        };

        $.ajax({
            url: apiBaseUrl + '/api/trip-stories',
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token },
            contentType: 'application/json',
            data: JSON.stringify(data)
        }).done(function (result) {
            var storyId = result.id;

            // Upload photos if any
            var fileInput = document.getElementById('photoInput');
            var files = fileInput ? fileInput.files : [];

            if (files.length > 0) {
                self.uploadPhotos(storyId, files, 0, function () {
                    toastr.success(self.T('Success.StoryCreated') || 'Hikaye paylasıldı!');
                    window.location.href = '/Travelers/Story/' + storyId;
                });
            } else {
                toastr.success(self.T('Success.StoryCreated') || 'Hikaye paylasıldı!');
                window.location.href = '/Travelers/Story/' + storyId;
            }
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.error || self.T('Common.Error'));
            self.isCreating(false);
        });
    };

    // Upload photos sequentially
    self.uploadPhotos = function (storyId, files, index, callback) {
        if (index >= files.length || index >= 10) {
            callback();
            return;
        }

        var file = files[index];
        if (!file.type.startsWith('image/') || file.size > 5 * 1024 * 1024) {
            self.uploadPhotos(storyId, files, index + 1, callback);
            return;
        }

        var formData = new FormData();
        formData.append('file', file);

        var token = localStorage.getItem('token');
        $.ajax({
            url: apiBaseUrl + '/api/trip-stories/' + storyId + '/photos',
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token },
            data: formData,
            processData: false,
            contentType: false
        }).always(function () {
            self.uploadPhotos(storyId, files, index + 1, callback);
        });
    };

    // Init
    self.init = function () {
        var token = localStorage.getItem('token');
        if (token) {
            self.isLoggedIn(true);
            self.loadCompletedTours();
        }
        self.isLoading(false);
    };
}

$(document).ready(function () {
    var vm = new CreateStoryViewModel();
    ko.applyBindings(vm, document.getElementById('createStoryApp'));
    vm.init();
});
