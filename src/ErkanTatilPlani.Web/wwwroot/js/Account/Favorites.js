function FavoritesViewModel() {
    var self = this;

    self.favorites = ko.observableArray([]);
    self.isLoading = ko.observable(true);

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

    self.loadFavorites = function() {
        self.isLoading(true);

        $.ajax({
            url: apiBaseUrl + '/api/favorites',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            success: function(response) {
                self.favorites(response.favorites || []);
                self.isLoading(false);
            },
            error: function(xhr) {
                if (xhr.status === 401) {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                } else {
                    toastr.error(T('Common.Error'));
                }
                self.isLoading(false);
            }
        });
    };

    self.removeFavorite = function(favorite) {
        if (!confirm(T('Favorites.RemoveConfirm'))) return;

        $.ajax({
            url: apiBaseUrl + '/api/favorites/' + favorite.tourId,
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            success: function() {
                self.favorites.remove(favorite);
                toastr.success(T('Favorites.Removed'));
            },
            error: function() {
                toastr.error(T('Common.Error'));
            }
        });
    };

    // Initialize
    self.loadFavorites();
}

ko.applyBindings(new FavoritesViewModel(), document.getElementById('favoritesApp'));
