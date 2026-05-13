function ToursViewModel() {
    var self = this;

    // Tur verileri
    self.tours = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.totalCount = ko.observable(0);

    // Harita gorunumu
    self.viewMode = ko.observable('list');
    self.map = null;
    self.markers = [];
    self.userMarker = null;
    self.userLocation = ko.observable(null);
    self.userLocationLabel = ko.observable('');
    self.addressQuery = ko.observable('');
    self.locationError = ko.observable('');
    self.isLocating = ko.observable(false);
    self.isSearchingAddress = ko.observable(false);
    self.isNearbyPanelOpen = ko.observable(false);
    self.defaultNearbyRadiusKm = 50;
    self.maximumNearbyRadiusKm = 120;
    self.nearbyRadiusBufferKm = 15;

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

    // Koordinatli tur sayisi
    self.toursWithCoordinates = ko.computed(function() {
        return self.tours().filter(function(t) { return self.getTourCoordinates(t); }).length;
    });

    self.hasUserLocation = ko.pureComputed(function() {
        return !!self.userLocation();
    });

    self.nearbyTours = ko.pureComputed(function() {
        var location = self.userLocation();
        if (!location) {
            return [];
        }

        var candidates = self.tours()
            .map(function(tour) {
                var coordinates = self.getTourCoordinates(tour);
                if (!coordinates) {
                    return null;
                }

                var distanceKm = self.calculateDistanceKm(
                    location.latitude,
                    location.longitude,
                    coordinates.latitude,
                    coordinates.longitude
                );

                return {
                    tour: tour,
                    distanceKm: distanceKm,
                    distanceLabel: self.formatDistanceLabel(distanceKm)
                };
            })
            .filter(function(item) { return item !== null; })
            .sort(function(left, right) { return left.distanceKm - right.distanceKm; });

        if (candidates.length === 0) {
            return [];
        }

        var closestDistanceKm = candidates[0].distanceKm;
        var nearbyRadiusKm = Math.min(
            self.maximumNearbyRadiusKm,
            Math.max(self.defaultNearbyRadiusKm, closestDistanceKm + self.nearbyRadiusBufferKm)
        );

        return candidates
            .filter(function(item) { return item.distanceKm <= nearbyRadiusKm; })
            .slice(0, 5);
    });

    self.hasNearbyTours = ko.pureComputed(function() {
        return self.nearbyTours().length > 0;
    });

    // Filtreleme verileri
    self.searchQuery = ko.observable('');
    self.filterDestination = ko.observable('');
    self.filterMinPrice = ko.observable('');
    self.filterMaxPrice = ko.observable('');
    self.filterMinDays = ko.observable('');
    self.filterMaxDays = ko.observable('');
    self.filterCompanyId = ko.observable('');
    self.filterFeatured = ko.observable(false);
    self.filterDifficulty = ko.observable('');
    self.filterCategory = ko.observable('');
    self.filterGuideLanguage = ko.observable('');
    self.sortBy = ko.observable('');

    // Filtre secenekleri (API'den yuklenecek)
    self.destinations = ko.observableArray([]);
    self.companies = ko.observableArray([]);
    self.categories = ko.observableArray([]);
    self.priceRange = ko.observable({ min: 0, max: 100000 });
    self.durationRange = ko.observable({ min: 1, max: 30 });

    // Favoriler
    self.favoriteIds = ko.observableArray([]);

    // Favori mi kontrol et
    self.isFavorite = function(tourId) {
        return self.favoriteIds().indexOf(tourId) !== -1;
    };

    // Favorileri yukle
    self.loadFavorites = function() {
        var token = localStorage.getItem('token');
        if (!token) return;

        var tourIds = self.tours().map(function(t) { return t.id; });
        if (tourIds.length === 0) return;

        $.ajax({
            url: apiBaseUrl + '/api/favorites/check-multiple',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(tourIds),
            headers: { 'Authorization': 'Bearer ' + token },
            success: function(response) {
                self.favoriteIds(response.favoriteIds || []);
            }
        });
    };

    // ===============================================
    // TUR TAKIP (WATCH) SISTEMI
    // ===============================================
    self.watchedIds = ko.observableArray([]);
    self.watchTarget = ko.observable(null);
    self.watchDays = ko.observable(7);

    self.isWatching = function(tourId) {
        return self.watchedIds().indexOf(tourId) !== -1;
    };

    self.loadWatches = function() {
        var token = localStorage.getItem('token');
        if (!token) return;

        var tourIds = self.tours().map(function(t) { return t.id; });
        if (tourIds.length === 0) return;

        $.ajax({
            url: apiBaseUrl + '/api/watches/check-multiple',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(tourIds),
            headers: { 'Authorization': 'Bearer ' + token },
            success: function(response) {
                self.watchedIds(response.watchedIds || []);
            }
        });
    };

    self.openWatchPopover = function(tour) {
        if (!isLoggedIn()) {
            toastr.warning(T('Common.LoginRequired') || 'Giris yapmaniz gerekiyor');
            window.location.href = '/Account/Login';
            return;
        }
        // Zaten takip ediyorsa direkt kaldir
        if (self.isWatching(tour.id)) {
            self.removeWatch(tour);
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
            url: apiBaseUrl + '/api/watches/' + tour.id + '/toggle',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ watchDays: parseInt(self.watchDays()) })
        }).done(function(response) {
            if (response.isWatching) {
                if (self.watchedIds().indexOf(tour.id) === -1) {
                    self.watchedIds.push(tour.id);
                }
                toastr.success(T('Watch.Added'));
            } else {
                self.watchedIds.remove(tour.id);
                toastr.success(T('Watch.Removed'));
            }
            watchModal.hide();
        }).fail(function(xhr) {
            toastr.error(xhr.responseJSON?.message || T('Common.Error'));
        });
    };

    self.removeWatch = function(tour) {
        $.ajax({
            url: apiBaseUrl + '/api/watches/' + tour.id,
            method: 'DELETE'
        }).done(function() {
            self.watchedIds.remove(tour.id);
            toastr.success(T('Watch.Removed'));
        }).fail(function(xhr) {
            toastr.error(xhr.responseJSON?.message || T('Common.Error'));
        });
    };

    // ===============================================
    // KITLIK (SCARCITY)
    // ===============================================
    self.getRemainingSlots = function(tour) {
        if (!tour.maxCapacity) return 999;
        var activeReservations = tour.activeReservationCount || 0;
        return tour.maxCapacity - activeReservations;
    };

    // Favori toggle
    // Tur paylas
    self.shareTour = function(tour, platform) {
        var shareUrl = window.location.origin + '/Tours/Details/' + tour.id;
        var shareText = tour.name + ' - ' + tour.destination + ' | Erkan Tatil Plani';

        if (platform === 'copy') {
            SocialShare.copyLink(shareUrl);
        } else {
            SocialShare.share(platform, shareUrl, shareText);
        }
    };

    self.toggleFavorite = function(tour) {
        var token = localStorage.getItem('token');
        if (!token) {
            toastr.warning(T('Common.LoginRequired'));
            return;
        }

        $.ajax({
            url: apiBaseUrl + '/api/favorites/' + tour.id + '/toggle',
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token },
            success: function(response) {
                if (response.isFavorite) {
                    if (self.favoriteIds().indexOf(tour.id) === -1) {
                        self.favoriteIds.push(tour.id);
                    }
                } else {
                    self.favoriteIds.remove(tour.id);
                }
                toastr.success(response.message);
            },
            error: function(xhr) {
                if (xhr.status === 401) {
                    toastr.warning(T('Common.LoginRequired'));
                } else {
                    toastr.error(T('Common.Error'));
                }
            }
        });
    };

    // Aktif filtre sayisi
    self.activeFilterCount = ko.computed(function() {
        var count = 0;
        if (self.searchQuery()) count++;
        if (self.filterDestination()) count++;
        if (self.filterMinPrice()) count++;
        if (self.filterMaxPrice()) count++;
        if (self.filterMinDays()) count++;
        if (self.filterMaxDays()) count++;
        if (self.filterCompanyId()) count++;
        if (self.filterFeatured()) count++;
        if (self.filterDifficulty()) count++;
        if (self.filterCategory()) count++;
        if (self.filterGuideLanguage()) count++;
        return count;
    });

    // Promosyon badge'leri
    self.promoBadges = ko.observable({});
    self.getPromoBadges = function(tourId) {
        return self.promoBadges()[tourId] || [];
    };
    self.loadPromoBadges = function(tourIds) {
        tourIds.forEach(function(tourId) {
            $.ajax({
                url: apiBaseUrl + '/api/tours/' + tourId + '/promotion-badges',
                method: 'GET'
            }).done(function(badges) {
                if (badges && badges.length > 0) {
                    var current = self.promoBadges();
                    current[tourId] = badges;
                    self.promoBadges(Object.assign({}, current));
                }
            });
        });
    };

    // Modal referanslari
    var watchModal;

    // Tarih formatlama
    self.formatDate = function(dateStr) {
        if (!dateStr) return '';
        var date = new Date(dateStr);
        return date.toLocaleDateString('tr-TR');
    };

    self.escapeHtml = function(value) {
        return (value || '')
            .toString()
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    };

    self.calculateDistanceKm = function(fromLatitude, fromLongitude, toLatitude, toLongitude) {
        var earthRadiusKm = 6371;
        var dLat = (toLatitude - fromLatitude) * (Math.PI / 180);
        var dLon = (toLongitude - fromLongitude) * (Math.PI / 180);
        var originLat = fromLatitude * (Math.PI / 180);
        var destinationLat = toLatitude * (Math.PI / 180);

        var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
            Math.sin(dLon / 2) * Math.sin(dLon / 2) * Math.cos(originLat) * Math.cos(destinationLat);
        var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

        return earthRadiusKm * c;
    };

    self.formatDistanceLabel = function(distanceKm) {
        var formattedDistance = distanceKm < 10
            ? distanceKm.toFixed(1)
            : Math.round(distanceKm).toString();

        return T('Tours.DistanceAway').replace('{0}', formattedDistance);
    };

    self.getDistanceText = function(tour) {
        var location = self.userLocation();
        var coordinates = self.getTourCoordinates(tour);
        if (!location || !coordinates) {
            return '';
        }

        return self.formatDistanceLabel(self.calculateDistanceKm(
            location.latitude,
            location.longitude,
            coordinates.latitude,
            coordinates.longitude
        ));
    };

    self.buildApproximateLocationLabel = function(location) {
        return T('Tours.LocationApproximate')
            .replace('{0}', location.latitude.toFixed(4))
            .replace('{1}', location.longitude.toFixed(4));
    };

    self.formatResolvedLocationLabel = function(label, fallbackLabel) {
        if (!label) {
            return fallbackLabel || '';
        }

        var parts = label
            .split(',')
            .map(function(part) { return part.trim(); })
            .filter(function(part) { return part.length > 0; });

        if (parts.length === 0) {
            return fallbackLabel || label;
        }

        var uniqueParts = [];
        parts.forEach(function(part) {
            if (uniqueParts.indexOf(part) === -1) {
                uniqueParts.push(part);
            }
        });

        return uniqueParts.slice(0, 4).join(', ');
    };

    self.setResolvedUserLocation = function(location, label) {
        self.userLocation(location);
        self.userLocationLabel(label || self.buildApproximateLocationLabel(location));
        self.locationError('');
        self.isNearbyPanelOpen(true);
        self.focusMapOnUserLocation();
    };

    self.resolveLocationErrorMessage = function(error) {
        if (error && error.code === 1) {
            return T('Tours.LocationPermissionDenied');
        }

        return T('Tours.LocationUnavailable');
    };

    self.reverseGeocodeUserLocation = function(location) {
        $.ajax({
            url: 'https://nominatim.openstreetmap.org/reverse',
            method: 'GET',
            dataType: 'json',
            timeout: 5000,
            data: {
                format: 'jsonv2',
                lat: location.latitude,
                lon: location.longitude,
                zoom: 16,
                'accept-language': currentLang || 'tr'
            }
        }).done(function(data) {
            if (!self.userLocation() ||
                self.userLocation().latitude !== location.latitude ||
                self.userLocation().longitude !== location.longitude) {
                return;
            }

            var label = data && (data.display_name || data.name);
            if (!label) {
                return;
            }

            self.userLocationLabel(self.formatResolvedLocationLabel(label, self.userLocationLabel()));
        });
    };

    self.focusMapOnUserLocation = function() {
        if (self.viewMode() !== 'map') {
            self.setViewMode('map');
            return;
        }

        setTimeout(function() {
            self.initMap();
        }, 100);
    };

    self.handleAddressKeydown = function(_, event) {
        if (event.key === 'Enter') {
            self.searchAddress();
            return false;
        }

        return true;
    };

    self.locateUser = function() {
        if (!navigator.geolocation) {
            var unsupportedMessage = T('Tours.LocationNotSupported');
            self.locationError(unsupportedMessage);
            toastr.error(unsupportedMessage);
            return;
        }

        self.isLocating(true);
        self.locationError('');

        navigator.geolocation.getCurrentPosition(function(position) {
            var location = {
                latitude: position.coords.latitude,
                longitude: position.coords.longitude
            };

            self.isLocating(false);
            self.setResolvedUserLocation(location, self.buildApproximateLocationLabel(location));
            self.reverseGeocodeUserLocation(location);
        }, function(error) {
            var errorMessage = self.resolveLocationErrorMessage(error);
            self.isLocating(false);
            self.locationError(errorMessage);
            toastr.warning(errorMessage);
        }, {
            enableHighAccuracy: true,
            timeout: 12000,
            maximumAge: 300000
        });
    };

    self.searchAddress = function() {
        var query = (self.addressQuery() || '').trim();
        if (!query) {
            var requiredMessage = T('Tours.AddressRequired');
            self.locationError(requiredMessage);
            toastr.warning(requiredMessage);
            return;
        }

        self.isSearchingAddress(true);
        self.locationError('');

        $.ajax({
            url: 'https://nominatim.openstreetmap.org/search',
            method: 'GET',
            dataType: 'json',
            timeout: 8000,
            data: {
                q: query,
                format: 'jsonv2',
                limit: 1,
                addressdetails: 1,
                'accept-language': currentLang || 'tr'
            }
        }).done(function(results) {
            var match = Array.isArray(results) && results.length > 0 ? results[0] : null;
            if (!match) {
                var notFoundMessage = T('Tours.AddressNotFound');
                self.locationError(notFoundMessage);
                toastr.warning(notFoundMessage);
                return;
            }

            var latitude = parseFloat(match.lat);
            var longitude = parseFloat(match.lon);
            if (isNaN(latitude) || isNaN(longitude)) {
                var invalidMessage = T('Tours.AddressLookupFailed');
                self.locationError(invalidMessage);
                toastr.error(invalidMessage);
                return;
            }

            self.setResolvedUserLocation({
                latitude: latitude,
                longitude: longitude
            }, self.formatResolvedLocationLabel(match.display_name || query, query));
        }).fail(function() {
            var lookupFailedMessage = T('Tours.AddressLookupFailed');
            self.locationError(lookupFailedMessage);
            toastr.error(lookupFailedMessage);
        }).always(function() {
            self.isSearchingAddress(false);
        });
    };

    self.clearUserLocation = function() {
        self.userLocation(null);
        self.userLocationLabel('');
        self.locationError('');
        self.isNearbyPanelOpen(false);

        if (self.map && self.userMarker) {
            self.map.removeLayer(self.userMarker);
        }

        self.userMarker = null;

        if (self.viewMode() === 'map' && self.map) {
            self.updateMarkers();
        }
    };

    self.openNearbyPanel = function() {
        if (!self.hasUserLocation()) {
            return;
        }

        self.isNearbyPanelOpen(true);
    };

    self.closeNearbyPanel = function() {
        self.isNearbyPanelOpen(false);
    };

    // Turlari yukle
    self.loadData = function() {
        self.isLoading(true);

        var params = {};
        if (self.searchQuery()) params.search = self.searchQuery();
        if (self.filterDestination()) params.destination = self.filterDestination();
        if (self.filterMinPrice()) params.minPrice = self.filterMinPrice();
        if (self.filterMaxPrice()) params.maxPrice = self.filterMaxPrice();
        if (self.filterMinDays()) params.minDays = self.filterMinDays();
        if (self.filterMaxDays()) params.maxDays = self.filterMaxDays();
        if (self.filterCompanyId()) params.companyId = self.filterCompanyId();
        if (self.filterFeatured()) params.featured = true;
        if (self.filterDifficulty()) params.difficulty = self.filterDifficulty();
        if (self.filterCategory()) params.category = self.filterCategory();
        if (self.filterGuideLanguage()) params.guideLanguage = self.filterGuideLanguage();
        if (self.sortBy()) params.sort = self.sortBy();

        $.ajax({
            url: apiBaseUrl + '/api/tours',
            method: 'GET',
            data: params,
            success: function(response) {
                var tours = (response.tours || []).map(function(tour) {
                    var coordinates = self.getTourCoordinates(tour);
                    if (coordinates) {
                        tour.latitude = coordinates.latitude;
                        tour.longitude = coordinates.longitude;
                    }
                    return tour;
                });
                self.tours(tours);
                self.totalCount(response.totalCount);

                // Promosyon badge'lerini yukle
                var tourIds = tours.map(function(t) { return t.id; });
                self.loadPromoBadges(tourIds);

                // Filtre seceneklerini guncelle (ilk yuklemede)
                if (response.filters) {
                    if (self.destinations().length === 0) {
                        self.destinations(response.filters.destinations);
                    }
                    self.priceRange(response.filters.priceRange);
                    self.durationRange(response.filters.durationRange);
                }

                self.isLoading(false);

                // URL'den ?tour=ID parametresi varsa detay sayfasina yonlendir
                if (!self._tourParamHandled) {
                    self._tourParamHandled = true;
                    var urlParams = new URLSearchParams(window.location.search);
                    var tourId = parseInt(urlParams.get('tour'));
                    if (tourId) {
                        window.location.href = '/Tours/Details/' + tourId;
                    }
                }
            },
            error: function() {
                toastr.error('Turlar yuklenirken hata olustu');
                self.isLoading(false);
            }
        });
    };

    // Firmalari yukle
    self.loadCompanies = function() {
        $.ajax({
            url: apiBaseUrl + '/api/tours/companies',
            method: 'GET',
            success: function(data) {
                self.companies(data);
            }
        });
    };

    // Filtreleri uygula
    self.applyFilters = function() {
        self.loadData();
    };

    // Filtreleri temizle
    self.clearFilters = function() {
        self.searchQuery('');
        self.filterDestination('');
        self.filterMinPrice('');
        self.filterMaxPrice('');
        self.filterMinDays('');
        self.filterMaxDays('');
        self.filterCompanyId('');
        self.filterFeatured(false);
        self.filterDifficulty('');
        self.filterCategory('');
        self.filterGuideLanguage('');
        self.sortBy('');
        self.loadData();
    };

    // Aramayi temizle
    self.clearSearch = function() {
        self.searchQuery('');
        self.loadData();
    };

    // Gorunum modu degistir
    self.setViewMode = function(mode) {
        if (mode === 'map' && !window.L) {
            toastr.warning(T('Error.DataLoadFailed') || 'Veriler yuklenemedi');
            self.viewMode('list');
            return;
        }

        self.viewMode(mode);
        if (mode === 'map') {
            // Haritayi baslatmak icin kucuk bir gecikme gerekli (DOM render)
            setTimeout(function() {
                self.initMap();
            }, 100);
        }
    };

    // Haritayi baslat
    self.initMap = function() {
        var mapContainer = document.getElementById('toursMap');
        if (!mapContainer || !window.L) return;

        // Harita zaten baslatilmissa sadece marker'lari guncelle
        if (self.map) {
            self.updateMarkers();
            return;
        }

        // Turkiye merkezi koordinatlari
        self.map = L.map('toursMap').setView([39.0, 35.0], 6);

        // OpenStreetMap tile layer
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        }).addTo(self.map);

        self.updateMarkers();
    };

    // Marker'lari guncelle
    self.updateMarkers = function() {
        if (!self.map) return;

        // Mevcut marker'lari temizle
        self.markers.forEach(function(marker) {
            self.map.removeLayer(marker);
        });
        self.markers = [];

        if (self.userMarker) {
            self.map.removeLayer(self.userMarker);
            self.userMarker = null;
        }

        // Koordinatli turlari filtrele
        var toursWithCoords = self.hasUserLocation()
            ? self.nearbyTours().map(function(item) { return item.tour; })
            : self.tours().filter(function(t) { return self.getTourCoordinates(t); });

        var bounds = [];

        if (self.userLocation()) {
            self.userMarker = L.marker([self.userLocation().latitude, self.userLocation().longitude]).addTo(self.map);
            self.userMarker.bindPopup(
                '<div class="tour-popup">' +
                '<strong>' + self.escapeHtml(T('Tours.LocationReady')) + '</strong><br/>' +
                self.escapeHtml(self.userLocationLabel()) +
                '</div>'
            );
            bounds.push([self.userLocation().latitude, self.userLocation().longitude]);
        }

        if (toursWithCoords.length === 0) {
            if (bounds.length === 1) {
                self.map.setView(bounds[0], 12);
            }
            return;
        }

        toursWithCoords.forEach(function(tour) {
            var coordinates = self.getTourCoordinates(tour);
            var marker = L.marker([coordinates.latitude, coordinates.longitude]).addTo(self.map);
            var distanceText = self.getDistanceText(tour);

            // Popup icerigi
            var popupContent = '<div class="tour-popup">' +
                '<strong>' + tour.name + '</strong><br/>' +
                '<i class="bi bi-geo-alt"></i> ' + tour.destination + '<br/>' +
                '<span class="badge bg-primary">' + tour.price.toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'}) + '</span> ' +
                '<small class="text-muted">' + tour.durationDays + ' ' + T('Tour.Days') + '</small><br/>' +
                (tour.reviewCount > 0 ? '<i class="bi bi-star-fill text-warning"></i> ' + tour.averageRating.toFixed(1) + ' (' + tour.reviewCount + ')' : '') +
                (distanceText ? '<br/><small class="text-primary"><i class="bi bi-signpost"></i> ' + distanceText + '</small>' : '') +
                '<br/><a class="btn btn-sm btn-outline-primary mt-2" href="/Tours/Details/' + tour.id + '">' +
                '<i class="bi bi-info-circle"></i> ' + T('Tours.Details') + '</a>' +
                '</div>';

            marker.bindPopup(popupContent);
            self.markers.push(marker);
            bounds.push([coordinates.latitude, coordinates.longitude]);
        });

        // Haritayi marker'lara sigdir
        if (bounds.length > 0) {
            if (bounds.length === 1) {
                self.map.setView(bounds[0], 12);
            } else {
                self.map.fitBounds(bounds, {
                    padding: [20, 20],
                    maxZoom: self.hasUserLocation() ? 12 : 10
                });
            }
        }
    };

    // Turlar degistiginde marker'lari guncelle ve favorileri yukle
    self.tours.subscribe(function() {
        if (self.viewMode() === 'map' && self.map) {
            self.updateMarkers();
        }
        // Favorileri ve takipleri yukle
        self.loadFavorites();
        self.loadWatches();
    });

    // Siralama degistiginde otomatik yukle
    self.sortBy.subscribe(function() {
        self.loadData();
    });

    // Arama kutusu debounce (300ms bekle)
    var searchTimeout;
    self.searchQuery.subscribe(function(newValue) {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function() {
            self.loadData();
        }, 300);
    });

    self.userLocation.subscribe(function() {
        if (self.viewMode() === 'map' && self.map) {
            self.updateMarkers();
        }
    });

    self.userLocationLabel.subscribe(function() {
        if (self.viewMode() === 'map' && self.map && self.userLocation()) {
            self.updateMarkers();
        }
    });


    // Detay sayfasina yonlendir
    self.showDetails = function(tour) {
        window.location.href = '/Tours/Details/' + tour.id;
    };

    // Kategori ve Zorluk helper fonksiyonlari
    var difficultyMap = {
        0: { key: 'TourDifficulty.Easy', fallback: 'Kolay', css: 'bg-success', icon: 'bi-emoji-smile' },
        1: { key: 'TourDifficulty.Moderate', fallback: 'Orta', css: 'bg-info', icon: 'bi-emoji-neutral' },
        2: { key: 'TourDifficulty.Challenging', fallback: 'Zor', css: 'bg-warning text-dark', icon: 'bi-emoji-frown' },
        3: { key: 'TourDifficulty.Expert', fallback: 'Uzman', css: 'bg-danger', icon: 'bi-exclamation-triangle' }
    };

    self.getDifficultyName = function(id) {
        return difficultyMap[id] ? TL(difficultyMap[id].key, difficultyMap[id].fallback) : '';
    };
    self.getDifficultyBadgeClass = function(id) {
        return difficultyMap[id] ? difficultyMap[id].css : 'bg-secondary';
    };

    self.getCategoryName = function(id) {
        var cat = self.categories().find(function(c) { return c.id === id; });
        return cat ? TL(cat.nameResourceKey, cat.systemName) : '';
    };
    self.getCategoryIcon = function(id) {
        var cat = self.categories().find(function(c) { return c.id === id; });
        return cat ? cat.icon : '';
    };
    self.getCategoryBadgeClass = function(id) {
        var cat = self.categories().find(function(c) { return c.id === id; });
        return cat ? cat.cssClass : 'bg-secondary';
    };

    // Kategorileri yukle
    self.loadCategories = function() {
        $.ajax({
            url: apiBaseUrl + '/api/tours/categories',
            method: 'GET',
            success: function(data) {
                self.categories(data);
            }
        });
    };

    // Sayfa yuklendiginde
    $(document).ready(function() {
        watchModal = new bootstrap.Modal(document.getElementById('watchModal'));
        self.loadData();
        self.loadCompanies();
        self.loadCategories();
    });

    $(document).on('languageChanged', function() {
        self.tours.valueHasMutated();
        self.categories.valueHasMutated();
        if (self.viewMode() === 'map' && self.map) {
            self.updateMarkers();
        }
    });
}

var toursVM = new ToursViewModel();
ko.applyBindings(toursVM, document.getElementById('toursApp'));
