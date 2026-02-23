function MyReviewsViewModel() {
    var self = this;

    // Observables
    self.isLoading = ko.observable(true);
    self.reviews = ko.observableArray([]);
    self.stats = ko.observable({ total: 0, withReply: 0, withoutReply: 0, averageRating: 0 });
    self.currentFilter = ko.observable(null); // null=all, true=replied, false=awaiting
    self.companyId = ko.observable(null);

    // Reply state
    self.isReplying = ko.observable(false);
    self.replyingReviewId = ko.observable(null);
    self.replyText = ko.observable('');
    self.isSending = ko.observable(false);

    // Helper functions
    self.formatDate = function(dateString) {
        if (!dateString) return '';
        var date = new Date(dateString);
        return date.toLocaleDateString('tr-TR');
    };

    self.getStars = function(rating) {
        var html = '';
        for (var i = 0; i < 5; i++) {
            if (i < rating) {
                html += '<i class="bi bi-star-fill"></i>';
            } else {
                html += '<i class="bi bi-star"></i>';
            }
        }
        return html;
    };

    // Yorumlari yukle
    self.loadReviews = function(hasReply) {
        self.isLoading(true);

        // Company ID'yi al
        var userStr = localStorage.getItem('user');
        if (userStr) {
            try {
                var user = JSON.parse(userStr);
                if (user.companyId) {
                    self.companyId(user.companyId);
                }
            } catch (e) {}
        }

        if (!self.companyId()) {
            self.companyId(1); // Test icin
        }

        var url = apiBaseUrl + '/api/reviews/my?companyId=' + self.companyId();
        if (hasReply !== null && hasReply !== undefined) {
            url += '&hasReply=' + hasReply;
        }

        $.ajax({
            url: url,
            method: 'GET',
            success: function(data) {
                self.reviews(data.reviews || []);
                self.stats(data.stats || { total: 0, withReply: 0, withoutReply: 0, averageRating: 0 });
                self.isLoading(false);
            },
            error: function(xhr) {
                console.error('Yorumlar yuklenemedi:', xhr);
                toastr.error(T('Common.Error') || 'Bir hata olustu');
                self.isLoading(false);
            }
        });
    };

    // Filtrele
    self.filterByReply = function(hasReply) {
        self.currentFilter(hasReply);
        self.loadReviews(hasReply);
    };

    // Yanit baslat
    self.startReply = function(review) {
        self.isReplying(true);
        self.replyingReviewId(review.id);
        self.replyText('');
    };

    // Yanit iptal
    self.cancelReply = function() {
        self.isReplying(false);
        self.replyingReviewId(null);
        self.replyText('');
    };

    // Yanit gonder
    self.submitReply = function() {
        if (!self.replyText() || !self.replyText().trim()) {
            toastr.warning(T('Common.Required') || 'Yanit yazin');
            return;
        }

        self.isSending(true);

        // Visitor ID'yi al
        var visitorId = null;
        var userStr = localStorage.getItem('user');
        if (userStr) {
            try {
                var user = JSON.parse(userStr);
                visitorId = user.id;
            } catch (e) {}
        }

        $.ajax({
            url: apiBaseUrl + '/api/reviews/' + self.replyingReviewId() + '/reply',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                visitorId: visitorId,
                comment: self.replyText()
            }),
            success: function() {
                toastr.success(T('MyReviews.ReplySuccess') || 'Yanit gonderildi');
                self.cancelReply();
                self.loadReviews(self.currentFilter());
                self.isSending(false);
            },
            error: function(xhr) {
                console.error('Yanit gonderilemedi:', xhr);
                toastr.error(xhr.responseJSON?.message || T('Common.Error') || 'Bir hata olustu');
                self.isSending(false);
            }
        });
    };

    // Init
    $(document).ready(function() {
        self.loadReviews(null);
    });
}

ko.applyBindings(new MyReviewsViewModel(), document.getElementById('myReviewsApp'));
