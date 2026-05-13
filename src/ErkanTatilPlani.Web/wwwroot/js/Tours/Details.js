function TourDetailViewModel() {
    var self = this;
    // apiBaseUrl is defined globally in _Layout.cshtml
    var root = document.getElementById('tourDetailApp');
    var tourId = root ? parseInt(root.getAttribute('data-tour-id'), 10) : NaN;

    // Tur verisi
    self.tour = ko.observable(null);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);

    // Rezervasyon modal
    self.isLoadingDates = ko.observable(false);
    self.reservationDateFrom = ko.observable('');
    self.reservationDateTo = ko.observable('');
    self.reservationSessions = ko.observableArray([]);
    self.selectedReservationSession = ko.observable(null);
    self.reservationAvailabilityNotice = ko.observable('');
    self.showPaymentStep = ko.observable(false);
    self.paymentMethods = ko.observableArray([]);
    self.selectedPaymentMethod = ko.observable('');
    self.paymentType = ko.observable('deposit');
    self.numberOfPeople = ko.observable(1);
    self.couponCode = ko.observable('');
    self.couponDiscount = ko.observable(0);
    self.totalDiscount = ko.observable(0);
    self.appliedDiscounts = ko.observableArray([]);
    self.reservationData = ko.observable({
        fullName: '',
        email: '',
        phone: '',
        numberOfPeople: 1,
        notes: ''
    });
    self.participants = ko.observableArray([]);

    // Yorumlar
    self.reviews = ko.observableArray([]);
    self.isLoadingReviews = ko.observable(false);
    self.reviewSort = ko.observable('newest');
    self.reviewRatingFilter = ko.observable('');
    self.reviewPage = ko.observable(1);
    self.reviewTotalPages = ko.observable(1);
    self.reviewFormData = ko.observable({
        overallRating: 0, serviceRating: '', valueRating: '', locationRating: '',
        organizationRating: '', guideRating: '', title: '', pros: '', cons: '',
        comment: '', visitDate: '', travelTypeId: '0', wouldRecommend: true
    });

    // Yanit ve sikayet
    self.selectedReviewId = ko.observable(null);
    self.replyText = ko.observable('');
    self.reportReasonId = ko.observable('');
    self.reportDescription = ko.observable('');

    // Favoriler
    self.favoriteIds = ko.observableArray([]);
    self.isFavorite = function(tourId) {
        return self.favoriteIds().indexOf(tourId) !== -1;
    };

    // Takip
    self.watchedIds = ko.observableArray([]);
    self.watchTarget = ko.observable(null);
    self.watchDays = ko.observable(7);
    self.isWatching = function(tourId) {
        return self.watchedIds().indexOf(tourId) !== -1;
    };

    // Kategoriler
    self.categories = ko.observableArray([]);

    // Modal referanslari
    var writeReviewModal, replyModal, reportModal, reservationModal, watchModal;

    function createModal(id) {
        var element = document.getElementById(id);
        return element ? new bootstrap.Modal(element) : null;
    }

    self.destinationCoordinates = [
        { terms: ['selcuk', 'efes'], latitude: 37.9490, longitude: 27.3689 },
        { terms: ['cesme', 'alacati'], latitude: 38.3240, longitude: 26.3030 },
        { terms: ['pamukkale', 'denizli'], latitude: 37.9137, longitude: 29.1187 },
        { terms: ['uzungol'], latitude: 40.6200, longitude: 40.2900 },
        { terms: ['ayder', 'rize'], latitude: 40.9530, longitude: 41.0920 },
        { terms: ['sumela', 'macka'], latitude: 40.6890, longitude: 39.6580 },
        { terms: ['trabzon'], latitude: 41.0027, longitude: 39.7168 },
        { terms: ['kemer'], latitude: 36.6014, longitude: 30.5601 },
        { terms: ['kas', 'kekova'], latitude: 36.1999, longitude: 29.6409 },
        { terms: ['olimpos', 'yanaras'], latitude: 36.3965, longitude: 30.4730 },
        { terms: ['goreme', 'kapadokya', 'nevsehir'], latitude: 38.6431, longitude: 34.8289 },
        { terms: ['sultanahmet'], latitude: 41.0086, longitude: 28.9802 },
        { terms: ['bogaz'], latitude: 41.0830, longitude: 29.0430 },
        { terms: ['adalar', 'buyukada', 'heybeliada'], latitude: 40.8740, longitude: 29.1290 },
        { terms: ['istanbul'], latitude: 41.0082, longitude: 28.9784 },
        { terms: ['izmir'], latitude: 38.4237, longitude: 27.1428 },
        { terms: ['antalya'], latitude: 36.8969, longitude: 30.7133 }
    ];

    self.normalizeDestination = function(destination) {
        return (destination || '')
            .toString()
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/ı/g, 'i')
            .replace(/ğ/g, 'g')
            .replace(/ü/g, 'u')
            .replace(/ş/g, 's')
            .replace(/ö/g, 'o')
            .replace(/ç/g, 'c');
    };

    self.getTourCoordinates = function(tour) {
        var lat = parseFloat(tour.latitude);
        var lng = parseFloat(tour.longitude);
        if (!isNaN(lat) && !isNaN(lng)) {
            return { latitude: lat, longitude: lng };
        }

        var destination = self.normalizeDestination(tour.destination);
        var match = self.destinationCoordinates.find(function(item) {
            return item.terms.some(function(term) { return destination.indexOf(term) !== -1; });
        });
        return match ? { latitude: match.latitude, longitude: match.longitude } : null;
    };

    // ===== Tur Yukle =====
    self.loadTour = function(id) {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + id,
            method: 'GET',
            success: function(data) {
                var coordinates = self.getTourCoordinates(data);
                if (coordinates) {
                    data.latitude = coordinates.latitude;
                    data.longitude = coordinates.longitude;
                }
                data.meetingPoint = data.meetingPoint || data.meetingPointAddress || '';
                self.tour(data);
                self.isLoading(false);
                self.loadReviews(id, false);
                if (data.latitude && data.longitude) self.loadWeather(id);
                self.initShareButtons(data);
                self.initMap(data);
            },
            error: function() {
                self.tour(null);
                self.isLoading(false);
            },
            complete: function() {
                self.isLoading(false);
            }
        });
    };

    // ===== Rezervasyon Modal - Tarih Arama =====
    self.onReservationDateChange = function() {
        var tour = self.tour();
        var fromStr = self.reservationDateFrom();
        if (!tour || !fromStr) {
            self.reservationAvailabilityNotice('');
            self.setReservationSessions([]);
            return;
        }
        var toStr = self.reservationDateTo() || fromStr;
        self.isLoadingDates(true);
        self.reservationAvailabilityNotice('');
        var previousToken = self.getReservationSessionToken(self.selectedReservationSession());
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tour.id + '/dates',
            method: 'GET',
            data: { from: fromStr, to: toStr },
            success: function(data) {
                self.setReservationSessions(data, previousToken);
                self.isLoadingDates(false);
            },
            error: function() {
                self.setReservationSessions([]);
                self.isLoadingDates(false);
            }
        });
    };

    self.getReservationSessionToken = function(session) {
        return session ? (session.token || ('d:' + session.id)) : null;
    };

    self.sortReservationSessions = function(sessions) {
        return (sessions || []).slice().sort(function(left, right) {
            var leftDate = (left.date || '').toString();
            var rightDate = (right.date || '').toString();
            if (leftDate !== rightDate) return leftDate.localeCompare(rightDate);
            return (left.startTime || '').localeCompare(right.startTime || '');
        });
    };

    self.setReservationSessions = function(sessions, preferredToken) {
        var items = Array.isArray(sessions) ? sessions : [];
        var normalized = self.sortReservationSessions(items.filter(function(item) {
            return item && item.isAvailable;
        }));

        normalized.forEach(function(session) {
            session.token = self.getReservationSessionToken(session);
        });

        self.reservationSessions(normalized);

        var selected = null;
        if (preferredToken) {
            selected = normalized.find(function(session) {
                return session.token === preferredToken;
            }) || null;
        }

        if (!selected && normalized.length > 0) {
            selected = normalized[0];
        }

        self.selectedReservationSession(selected);
        if (normalized.length > 0) {
            self.reservationAvailabilityNotice('');
        }
    };

    self.selectReservationSession = function(session) {
        var tokenOrId = self.getReservationSessionToken(session);
        if (self.selectedReservationSession() && self.selectedReservationSession().token === tokenOrId) {
            self.selectedReservationSession(null);
        } else {
            session.token = tokenOrId;
            self.selectedReservationSession(session);
        }
    };

    // ===== Formatting =====
    self.formatDate = function(dateStr) {
        var d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR', {weekday:'short', day:'numeric', month:'short'});
    };

    self.formatSessionDuration = function(session) {
        var time = session.startTime || '00:00';
        var val = session.durationValue || 1;
        var unit = (session.durationUnit || 'Day').toLowerCase();
        if (unit === 'hour') {
            var parts = time.split(':');
            var startH = parseInt(parts[0]); var startM = parseInt(parts[1] || '0');
            var endH = startH + val; var endM = startM;
            return String(startH).padStart(2,'0') + ':' + String(startM).padStart(2,'0') + ' - ' + String(endH).padStart(2,'0') + ':' + String(endM).padStart(2,'0');
        }
        if (unit === 'day') return val + ' ' + (T('Common.Day') || 'gun');
        if (unit === 'week') return val + ' ' + (T('Common.Week') || 'hafta');
        if (unit === 'month') return val + ' ' + (T('Common.Month') || 'ay');
        return val + ' ' + unit;
    };

    self.toIsoDate = function(date) {
        var year = date.getFullYear();
        var month = String(date.getMonth() + 1).padStart(2, '0');
        var day = String(date.getDate()).padStart(2, '0');
        return year + '-' + month + '-' + day;
    };

    self.preloadReservationSessions = function(tour) {
        if (!tour) return;

        var startDate = new Date();
        startDate.setHours(0, 0, 0, 0);

        var endDate = new Date(startDate.getTime());
        endDate.setDate(endDate.getDate() + 45);

        self.isLoadingDates(true);
        self.reservationAvailabilityNotice('');
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tour.id + '/dates',
            method: 'GET',
            data: {
                from: self.toIsoDate(startDate),
                to: self.toIsoDate(endDate)
            },
            success: function(data) {
                var items = Array.isArray(data) ? data : [];
                var available = self.sortReservationSessions(items.filter(function(item) {
                    return item && item.isAvailable;
                }));

                if (available.length === 0) {
                    self.reservationDateFrom('');
                    self.reservationDateTo('');
                    self.setReservationSessions([]);
                    self.reservationAvailabilityNotice(T('TourDate.NoUpcomingSessions') || 'Bu tur icin yakin tarihte musait seans bulunmuyor');
                    self.isLoadingDates(false);
                    return;
                }

                var firstSession = available[0];
                var firstDate = firstSession.date;
                var sameDaySessions = available.filter(function(session) {
                    return session.date === firstDate;
                });

                self.reservationDateFrom(firstDate);
                self.reservationDateTo(firstDate);
                self.setReservationSessions(sameDaySessions, self.getReservationSessionToken(firstSession));
                self.isLoadingDates(false);
            },
            error: function() {
                self.setReservationSessions([]);
                self.isLoadingDates(false);
            }
        });
    };

    self.renderStars = function(rating) {
        var html = '';
        for (var i = 1; i <= 5; i++) {
            html += '<i class="bi ' + (i <= rating ? 'bi-star-fill text-warning' : 'bi-star text-muted') + '"></i>';
        }
        return html;
    };

    self.canWriteReview = ko.computed(function() {
        return window.currentUser && window.currentUser.id;
    });

    self.hasMoreReviews = ko.computed(function() {
        return self.reviewPage() < self.reviewTotalPages();
    });

    self.availablePaymentMethods = ko.computed(function() {
        return self.paymentMethods().filter(function(method) {
            return method && method.isAvailableForCheckout;
        });
    });

    self.hasSinglePaymentMethod = ko.computed(function() {
        return self.paymentMethods().length === 1;
    });

    self.hasMultiplePaymentMethods = ko.computed(function() {
        return self.paymentMethods().length > 1;
    });

    self.selectedPaymentMethodDetails = ko.computed(function() {
        var selectedSystemName = self.selectedPaymentMethod();
        var methods = self.paymentMethods();
        var selected = methods.find(function(method) {
            return method && method.systemName === selectedSystemName;
        });

        if (selected) return selected;

        var defaultMethod = methods.find(function(method) {
            return method && method.isDefault;
        });
        if (defaultMethod) return defaultMethod;

        var firstAvailable = methods.find(function(method) {
            return method && method.isAvailableForCheckout;
        });
        if (firstAvailable) return firstAvailable;

        return methods.length > 0 ? methods[0] : null;
    });

    self.getPaymentMethodMeta = function(method) {
        if (!method) return '';

        var provider = method.providerDisplayName || method.providerSystemName || '';
        var description = method.description || '';

        if (provider && description) return provider + ' - ' + description;
        return provider || description || '';
    };

    var travelTypeNames = { 0: 'Yalniz', 1: 'Cift', 2: 'Aile', 3: 'Arkadaslar', 4: 'Is Seyahati' };
    self.getTravelTypeName = function(id) { return travelTypeNames[id] || ''; };

    // ===== Difficulty / Category =====
    var difficultyMap = {
        0: { key: 'TourDifficulty.Easy', fallback: 'Kolay', css: 'bg-success' },
        1: { key: 'TourDifficulty.Moderate', fallback: 'Orta', css: 'bg-info' },
        2: { key: 'TourDifficulty.Challenging', fallback: 'Zor', css: 'bg-warning text-dark' },
        3: { key: 'TourDifficulty.Expert', fallback: 'Uzman', css: 'bg-danger' }
    };
    self.getDifficultyName = function(id) { return difficultyMap[id] ? TL(difficultyMap[id].key, difficultyMap[id].fallback) : ''; };
    self.getDifficultyBadgeClass = function(id) { return difficultyMap[id] ? difficultyMap[id].css : 'bg-secondary'; };
    self.getCategoryName = function(id) {
        var cat = self.categories().find(function(c) { return c.id === id; });
        return cat ? TL(cat.nameResourceKey, cat.systemName) : '';
    };

    // ===== Yorumlar =====
    self.loadReviews = function(tourId, append) {
        if (!append) { self.reviewPage(1); self.reviews([]); }
        self.isLoadingReviews(true);
        var params = { sort: self.reviewSort(), page: self.reviewPage(), pageSize: 5 };
        if (self.reviewRatingFilter()) params.rating = self.reviewRatingFilter();
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/reviews',
            method: 'GET',
            data: params,
            success: function(data) {
                if (append) { self.reviews(self.reviews().concat(data.reviews)); }
                else { self.reviews(data.reviews); }
                self.reviewTotalPages(data.pagination.totalPages);
                self.isLoadingReviews(false);
            },
            error: function() { toastr.error('Yorumlar yuklenirken hata olustu'); self.isLoadingReviews(false); }
        });
    };

    self.loadMoreReviews = function() {
        self.reviewPage(self.reviewPage() + 1);
        self.loadReviews(self.tour().id, true);
    };

    self.reviewSort.subscribe(function() { if (self.tour()) self.loadReviews(self.tour().id, false); });
    self.reviewRatingFilter.subscribe(function() { if (self.tour()) self.loadReviews(self.tour().id, false); });

    self.openWriteReviewModal = function() {
        if (!self.canWriteReview()) { toastr.warning('Yorum yapmak icin giris yapmaniz gerekiyor'); return; }
        self.reviewFormData({
            overallRating: 0, serviceRating: '', valueRating: '', locationRating: '',
            organizationRating: '', guideRating: '', title: '', pros: '', cons: '',
            comment: '', visitDate: '', travelTypeId: '0', wouldRecommend: true
        });
        writeReviewModal.show();
    };

    self.submitReview = function() {
        var data = self.reviewFormData();
        if (data.overallRating < 1) { toastr.warning('Lutfen bir puan secin'); return; }
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + self.tour().id + '/reviews',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                visitorId: window.currentUser.id,
                overallRating: data.overallRating,
                serviceRating: data.serviceRating ? parseInt(data.serviceRating) : null,
                valueRating: data.valueRating ? parseInt(data.valueRating) : null,
                locationRating: data.locationRating ? parseInt(data.locationRating) : null,
                organizationRating: data.organizationRating ? parseInt(data.organizationRating) : null,
                guideRating: data.guideRating ? parseInt(data.guideRating) : null,
                title: data.title, pros: data.pros, cons: data.cons, comment: data.comment,
                visitDate: data.visitDate || null, travelTypeId: parseInt(data.travelTypeId),
                wouldRecommend: data.wouldRecommend
            }),
            success: function(response) {
                writeReviewModal.hide();
                toastr.success(response.message || 'Yorumunuz eklendi');
                self.loadReviews(self.tour().id, false);
                self.loadTour(self.tour().id);
                self.isSaving(false);
            },
            error: function(xhr) { toastr.error(xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu'); self.isSaving(false); }
        });
    };

    self.voteHelpful = function(reviewId, isHelpful) {
        if (!self.canWriteReview()) { toastr.warning('Oy vermek icin giris yapmaniz gerekiyor'); return; }
        $.ajax({
            url: apiBaseUrl + '/api/reviews/' + reviewId + '/helpful',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ visitorId: window.currentUser.id, isHelpful: isHelpful }),
            success: function(response) {
                var review = self.reviews().find(function(r) { return r.id === reviewId; });
                if (review) { review.helpfulCount = response.helpfulCount; review.notHelpfulCount = response.notHelpfulCount; self.reviews.valueHasMutated(); }
                toastr.success(response.message);
            },
            error: function(xhr) { toastr.error(xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu'); }
        });
    };

    self.openReplyModal = function(reviewId) {
        if (!self.canWriteReview()) { toastr.warning('Yanit vermek icin giris yapmaniz gerekiyor'); return; }
        self.selectedReviewId(reviewId);
        self.replyText('');
        replyModal.show();
    };

    self.submitReply = function() {
        if (!self.replyText()) return;
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/reviews/' + self.selectedReviewId() + '/reply',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ visitorId: window.currentUser.id, comment: self.replyText() }),
            success: function(response) {
                replyModal.hide();
                toastr.success(response.message || 'Yanitiniz eklendi');
                self.loadReviews(self.tour().id, false);
                self.isSaving(false);
            },
            error: function(xhr) { toastr.error(xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu'); self.isSaving(false); }
        });
    };

    self.openReportModal = function(reviewId) {
        if (!self.canWriteReview()) { toastr.warning('Sikayet etmek icin giris yapmaniz gerekiyor'); return; }
        self.selectedReviewId(reviewId);
        self.reportReasonId('');
        self.reportDescription('');
        reportModal.show();
    };

    self.submitReport = function() {
        if (!self.reportReasonId()) return;
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/reviews/' + self.selectedReviewId() + '/report',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ visitorId: window.currentUser.id, reasonId: parseInt(self.reportReasonId()), description: self.reportDescription() }),
            success: function(response) { reportModal.hide(); toastr.success(response.message || 'Sikayetiniz alindi'); self.isSaving(false); },
            error: function(xhr) { toastr.error(xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu'); self.isSaving(false); }
        });
    };

    // ===== Favoriler =====
    self.loadFavorites = function() {
        if (!window.currentUser || !window.currentUser.id) return;
        $.ajax({
            url: apiBaseUrl + '/api/visitors/' + window.currentUser.id + '/favorites',
            method: 'GET',
            success: function(data) { self.favoriteIds(data.map(function(f) { return f.tourId; })); }
        });
    };

    self.toggleFavorite = function(tourId) {
        if (!window.currentUser || !window.currentUser.id) { toastr.warning('Giris yapmaniz gerekiyor'); return; }
        var isFav = self.isFavorite(tourId);
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/favorite',
            method: isFav ? 'DELETE' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ visitorId: window.currentUser.id }),
            success: function() {
                if (isFav) { self.favoriteIds.remove(tourId); }
                else { self.favoriteIds.push(tourId); }
                toastr.success(isFav ? 'Favorilerden cikarildi' : 'Favorilere eklendi');
            },
            error: function(xhr) { toastr.error(xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu'); }
        });
    };

    // ===== Takip =====
    self.loadWatches = function() {
        if (!window.currentUser || !window.currentUser.id) return;
        $.ajax({
            url: apiBaseUrl + '/api/visitors/' + window.currentUser.id + '/watches',
            method: 'GET',
            success: function(data) { self.watchedIds(data.map(function(w) { return w.tourId; })); }
        });
    };

    self.openWatchPopover = function(tour) {
        if (!window.currentUser || !window.currentUser.id) { toastr.warning('Giris yapmaniz gerekiyor'); return; }
        if (self.isWatching(tour.id)) {
            self.removeWatch(tour.id);
            return;
        }
        self.watchTarget(tour);
        self.watchDays(7);
        watchModal.show();
    };

    self.toggleWatch = function() {
        var tour = self.watchTarget();
        if (!tour) return;
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tour.id + '/watch',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ visitorId: window.currentUser.id, durationDays: self.watchDays() }),
            success: function() {
                self.watchedIds.push(tour.id);
                watchModal.hide();
                toastr.success('Tur takibe alindi');
            },
            error: function(xhr) { toastr.error(xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu'); }
        });
    };

    self.removeWatch = function(tourId) {
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/watch',
            method: 'DELETE',
            contentType: 'application/json',
            data: JSON.stringify({ visitorId: window.currentUser.id }),
            success: function() {
                self.watchedIds.remove(tourId);
                toastr.success('Takip kaldirildi');
            },
            error: function(xhr) { toastr.error(xhr.responseJSON ? xhr.responseJSON.message : 'Hata olustu'); }
        });
    };

    // ===== Hava Durumu =====
    self.loadWeather = function(tourId) {
        $('#weatherWidget').hide();
        $.ajax({
            url: apiBaseUrl + '/api/weather/tour/' + tourId,
            method: 'GET'
        }).done(function(data) {
            if (data) {
                var iconUrl = 'https://openweathermap.org/img/wn/' + (data.icon || '01d') + '@2x.png';
                $('#weatherIcon').attr('src', iconUrl);
                $('#weatherTemp').text(data.temperature + '\u00B0C');
                $('#weatherCondition').text(data.condition);
                $('#weatherHumidity').html('<i class="bi bi-droplet"></i> ' + data.humidity + '%');
                $('#weatherWind').html('<i class="bi bi-wind"></i> ' + data.windSpeed + ' m/s');
                var recText = T(data.recommendation) || '';
                if (recText) {
                    var alertClass = data.isRainy ? 'text-warning' : 'text-success';
                    $('#weatherRecommendation').html('<i class="bi bi-info-circle ' + alertClass + '"></i> <span class="' + alertClass + '">' + recText + '</span>');
                } else {
                    $('#weatherRecommendation').empty();
                }
                $('#weatherWidget').show();
            }
        });
    };

    // ===== Paylas Butonlari =====
    self.initShareButtons = function(tour) {
        var shareUrl = window.location.origin + '/Tours/Details/' + tour.id;
        var shareText = tour.name + ' - ' + tour.destination + ' | Erkan Tatil Plani';
        $('#tourShareButtons').html(SocialShare.renderButtons({
            url: shareUrl,
            text: shareText,
            size: 'sm',
            platforms: ['facebook', 'twitter', 'whatsapp', 'telegram', 'copy']
        }));
    };

    // ===== Harita =====
    self.initMap = function(tour) {
        if (!tour.latitude || !tour.longitude) return;
        if (!window.L) return;
        setTimeout(function() {
            var mapEl = document.getElementById('tourMap');
            if (!mapEl || !window.L) return;
            var map = L.map('tourMap').setView([tour.latitude, tour.longitude], 14);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap'
            }).addTo(map);
            L.marker([tour.latitude, tour.longitude]).addTo(map)
                .bindPopup('<strong>' + tour.name + '</strong>' + (tour.meetingPoint ? '<br>' + tour.meetingPoint : ''))
                .openPopup();
        }, 300);
    };

    self.loadPaymentMethods = function() {
        $.ajax({
            url: apiBaseUrl + '/api/paymentmethods/public',
            method: 'GET',
            success: function(data) {
                var methods = Array.isArray(data) ? data : [];
                self.paymentMethods(methods);

                var defaultMethod = methods.find(function(method) {
                    return method.isDefault && method.isAvailableForCheckout;
                });
                var firstAvailable = methods.find(function(method) {
                    return method.isAvailableForCheckout;
                });
                var selected = defaultMethod || firstAvailable;
                self.selectedPaymentMethod(selected ? selected.systemName : '');
            },
            error: function() {
                self.paymentMethods([
                    {
                        systemName: 'iyzico-card',
                        displayName: T('Payment.CreditCard') || 'Kredi/Banka Karti',
                        description: T('Payment.IyzicoSecure') || 'iyzico ile guvenli odeme',
                        iconClass: 'bi bi-credit-card-2-front',
                        isDefault: true,
                        isOnline: true,
                        isAvailableForCheckout: true,
                        comingSoon: false
                    }
                ]);
                self.selectedPaymentMethod('iyzico-card');
            }
        });
    };

    // ===== Rezervasyon Modal =====
    self.openReservationModal = function(tour) {
        var user = getUser();
        if (!user) {
            toastr.warning(T('Error.LoginRequired') || 'Rezervasyon yapmak icin giris yapmaniz gerekiyor');
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent('/Tours/Details/' + tour.id);
            return;
        }
        if (user.userTypeId >= 1) {
            toastr.warning(T('Error.CompanyCannotReserve'));
            return;
        }

        self.numberOfPeople(1);
        self.showPaymentStep(false);
        self.paymentType('deposit');
        var defaultMethod = self.paymentMethods().find(function(method) {
            return method.isDefault && method.isAvailableForCheckout;
        });
        var firstAvailable = self.paymentMethods().find(function(method) {
            return method.isAvailableForCheckout;
        });
        var selected = defaultMethod || firstAvailable;
        self.selectedPaymentMethod(selected ? selected.systemName : '');
        self.selectedReservationSession(null);
        self.couponCode('');
        self.couponDiscount(0);
        self.totalDiscount(0);
        self.appliedDiscounts([]);
        self.participants([]);

        self.reservationDateFrom('');
        self.reservationDateTo('');
        self.reservationSessions([]);

        if (user) {
            self.reservationData({
                fullName: (user.firstName || '') + ' ' + (user.lastName || ''),
                email: user.email || '',
                phone: user.phone || '',
                numberOfPeople: 1,
                notes: ''
            });
        } else {
            self.reservationData({ fullName: '', email: '', phone: '', numberOfPeople: 1, notes: '' });
        }
        reservationModal.show();
        self.preloadReservationSessions(tour);
    };

    // ===== Fiyat Hesaplama =====
    self.calculateTotal = ko.computed(function() {
        var tour = self.tour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        return (tour.price * people).toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    self.getDepositPercentage = function() {
        var tour = self.tour();
        if (tour && tour.company && tour.company.depositPercentage) return tour.company.depositPercentage;
        return 30;
    };

    self.calculateDeposit = ko.computed(function() {
        var tour = self.tour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        return (tour.price * people * (self.getDepositPercentage() / 100)).toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    self.calculateRemaining = ko.computed(function() {
        var tour = self.tour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        return (tour.price * people * ((100 - self.getDepositPercentage()) / 100)).toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    self.calculateFinalTotal = ko.computed(function() {
        var tour = self.tour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        var total = tour.price * people;
        var discount = self.totalDiscount() || 0;
        return (total - discount).toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    self.calculatePayNow = ko.computed(function() {
        var tour = self.tour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        var total = tour.price * people;
        var discount = self.totalDiscount() || 0;
        var finalTotal = total - discount;
        if (self.paymentType() === 'full') return finalTotal.toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
        return (finalTotal * (self.getDepositPercentage() / 100)).toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    self.selectPaymentType = function(type) { self.paymentType(type); };
    self.selectPaymentMethod = function(method) {
        if (!method || !method.isAvailableForCheckout) return;
        self.selectedPaymentMethod(method.systemName);
    };

    self.applyCoupon = function() {
        var tour = self.tour();
        var code = self.couponCode();
        if (!tour || !code) { toastr.warning('Lutfen bir kupon kodu girin'); return; }
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tour.id + '/calculate-price',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ numberOfPeople: self.numberOfPeople() || 1, couponCode: code })
        }).done(function(result) {
            if (result.totalDiscount > 0) {
                self.totalDiscount(result.totalDiscount);
                self.appliedDiscounts(result.appliedDiscounts || []);
                var couponDisc = (result.appliedDiscounts || []).filter(function(d) { return d.promotionType === 'Coupon'; });
                self.couponDiscount(couponDisc.length > 0 ? couponDisc[0].discountAmount : 0);
                toastr.success('Kupon uyguland\u0131');
            } else {
                self.couponDiscount(0); self.totalDiscount(0); self.appliedDiscounts([]);
                toastr.warning('Bu kupon gecerli degil veya indirim uygulanamadi');
            }
        }).fail(function() { toastr.error('Kupon dogrulanamadi'); });
    };

    // ===== Katilimci =====
    self.addParticipant = function() {
        self.participants.push({ name: ko.observable(''), age: ko.observable(''), diet: ko.observable(''), health: ko.observable('') });
    };
    self.removeParticipant = function(p) { self.participants.remove(p); };

    // ===== Odeme Adimlari =====
    self.goToPaymentStep = function() {
        if (!self.selectedReservationSession()) {
            toastr.warning(T('TourDate.NoSessionSelected') || 'Lutfen bir tarih ve saat secin');
            return;
        }
        if (!self.selectedPaymentMethod()) {
            toastr.warning('Lutfen bir odeme yontemi secin');
            return;
        }
        var form = document.getElementById('reservationForm');
        if (!form.checkValidity()) { form.reportValidity(); return; }
        var data = self.reservationData();
        data.numberOfPeople = self.numberOfPeople();
        self.showPaymentStep(true);
    };

    self.goBackToForm = function() { self.showPaymentStep(false); };

    self.submitPayment = function() {
        var tour = self.tour();
        var data = self.reservationData();
        if (!tour) { toastr.error('Tur bilgisi bulunamadi'); return; }
        if (!self.selectedPaymentMethod()) { toastr.warning('Lutfen bir odeme yontemi secin'); return; }
        self.isSaving(true);

        var participantInfo = null;
        if (self.participants().length > 0) {
            participantInfo = JSON.stringify(self.participants().map(function(p) {
                return { name: p.name(), age: p.age(), diet: p.diet(), health: p.health() };
            }));
        }

        var session = self.selectedReservationSession();
        $.ajax({
            url: apiBaseUrl + '/api/reservations/public/create',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                tourId: tour.id,
                dateToken: session ? session.token : null,
                fullName: data.fullName,
                email: data.email,
                phone: data.phone,
                numberOfPeople: self.numberOfPeople(),
                notes: data.notes || '',
                couponCode: self.couponCode() || null,
                participantInfo: participantInfo,
                payFullAmount: self.paymentType() === 'full',
                paymentMethodSystemName: self.selectedPaymentMethod()
            }),
            success: function(response) {
                if (response.success && response.paymentPageUrl) {
                    reservationModal.hide();
                    self.showPaymentStep(false);
                    toastr.info('Odeme sayfasina yonlendiriliyorsunuz...');
                    setTimeout(function() { window.location.href = response.paymentPageUrl; }, 500);
                } else {
                    toastr.error(response.message || 'Rezervasyon olusturulamadi');
                    self.isSaving(false);
                }
            },
            error: function(xhr) {
                var msg = 'Bir hata olustu';
                if (xhr.responseJSON) msg = xhr.responseJSON.message || xhr.responseJSON.error || msg;
                toastr.error(msg);
                self.isSaving(false);
            }
        });
    };

    // ===== Kategorileri yukle =====
    self.loadCategories = function() {
        $.ajax({
            url: apiBaseUrl + '/api/tours/categories',
            method: 'GET',
            success: function(data) { self.categories(data); }
        });
    };

    // ===== Init =====
    self.init = function() {
        writeReviewModal = createModal('writeReviewModal');
        replyModal = createModal('replyModal');
        reportModal = createModal('reportModal');
        reservationModal = createModal('reservationModal');
        watchModal = createModal('watchModal');
        self.loadPaymentMethods();
        self.loadCategories();
        self.loadFavorites();
        self.loadWatches();
        if (isNaN(tourId)) {
            self.isLoading(false);
            return;
        }
        self.loadTour(tourId);
    };
}

var detailRoot = document.getElementById('tourDetailApp');
if (detailRoot) {
    var detailVM = new TourDetailViewModel();
    ko.applyBindings(detailVM, detailRoot);
    detailVM.init();
}
