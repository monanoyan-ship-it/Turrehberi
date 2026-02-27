function ToursViewModel() {
    var self = this;

    // Tur verileri
    self.tours = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.selectedTour = ko.observable(null);
    self.totalCount = ko.observable(0);

    // Harita gorunumu
    self.viewMode = ko.observable('list');
    self.map = null;
    self.markers = [];

    // Koordinatli tur sayisi
    self.toursWithCoordinates = ko.computed(function() {
        return self.tours().filter(function(t) { return t.latitude && t.longitude; }).length;
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
        var shareUrl = window.location.origin + '/Tours?id=' + tour.id;
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

    // Tur tarihleri (musaitlik takvimi)
    self.tourDates = ko.observableArray([]);
    self.isLoadingDates = ko.observable(false);
    self.selectedTourDateId = ko.observable(null);
    self.selectedTourDateObj = ko.observable(null);
    self.selectedDateToken = ko.observable(null);
    self.selectedReservationDate = ko.observable(null); // Secilen gun (yyyy-MM-dd)

    // Benzersiz musait tarihler (gun bazinda gruplama - seans filtreleme icin)
    self.availableDates = ko.computed(function() {
        var dates = self.tourDates();
        var dateMap = {};
        dates.forEach(function(d) {
            var key = d.date;
            if (!dateMap[key]) {
                dateMap[key] = { dateKey: key, count: 0, hasAvailable: false };
            }
            dateMap[key].count++;
            if (d.isAvailable) dateMap[key].hasAvailable = true;
        });
        return Object.values(dateMap).filter(function(d) { return d.hasAvailable; }).sort(function(a, b) { return a.dateKey.localeCompare(b.dateKey); });
    });

    // Date picker min/max
    self.datePickerMin = ko.computed(function() {
        var dates = self.availableDates();
        return dates.length > 0 ? dates[0].dateKey : '';
    });
    self.datePickerMax = ko.computed(function() {
        var dates = self.availableDates();
        return dates.length > 0 ? dates[dates.length - 1].dateKey : '';
    });
    self.datePickerError = ko.observable('');

    self.onDatePickerChange = function() {
        var dateKey = self.selectedReservationDate();
        self.datePickerError('');
        self.selectedTourDateId(null);
        self.selectedTourDateObj(null);
        self.selectedDateToken(null);
        if (!dateKey) return;
        var hasSession = self.availableDates().some(function(d) { return d.dateKey === dateKey; });
        if (!hasSession) {
            self.datePickerError(T('TourDate.NoSessions') || 'Bu tarihte musait seans yok');
        }
    };

    // Secilen gundeki seanslar
    self.sessionsForDate = ko.computed(function() {
        var dateKey = self.selectedReservationDate();
        if (!dateKey) return [];
        return self.tourDates().filter(function(d) {
            return d.isAvailable && d.date === dateKey;
        }).sort(function(a, b) { return (a.startTime || '').localeCompare(b.startTime || ''); });
    });

    self.selectTourDate = function(tourDate) {
        var tokenOrId = tourDate.token || ('d:' + tourDate.id);
        if (self.selectedDateToken() === tokenOrId) {
            self.selectedTourDateId(null);
            self.selectedTourDateObj(null);
            self.selectedDateToken(null);
        } else {
            self.selectedTourDateId(tourDate.id || null);
            self.selectedTourDateObj(tourDate);
            self.selectedDateToken(tokenOrId);
        }
    };

    self.formatDate = function(dateStr) {
        var d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR', {weekday:'short', day:'numeric', month:'short'});
    };

    self.formatTime = function(dateStr) {
        var d = new Date(dateStr);
        return String(d.getUTCHours()).padStart(2,'0') + ':' + String(d.getUTCMinutes()).padStart(2,'0');
    };

    self.formatDateWithTime = function(dateStr) {
        var d = new Date(dateStr);
        var dateText = d.toLocaleDateString('tr-TR', {day:'numeric', month:'short'});
        var hours = d.getUTCHours();
        var minutes = d.getUTCMinutes();
        if (hours === 0 && minutes === 0) return dateText;
        return dateText + ' ' + String(hours).padStart(2,'0') + ':' + String(minutes).padStart(2,'0');
    };

    self.isHourlyTour = function(session) {
        return (session.durationUnit || '').toLowerCase() === 'hour';
    };

    self.formatSessionDuration = function(session) {
        var time = session.startTime || '00:00';
        var val = session.durationValue || 1;
        var unit = (session.durationUnit || 'Day').toLowerCase();
        if (unit === 'hour') {
            // Baslangic + sure ile bitis saatini hesapla
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
    self.loadTourDates = function(tourId) {
        self.isLoadingDates(true);
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tourId + '/dates',
            method: 'GET',
            success: function(data) {
                self.tourDates(data);
                self.isLoadingDates(false);
            },
            error: function() {
                self.tourDates([]);
                self.isLoadingDates(false);
            }
        });
    };


    // Rezervasyon verileri
    self.reservationData = ko.observable({
        fullName: '',
        email: '',
        phone: '',
        numberOfPeople: 1,
        notes: ''
    });

    // Katilimci bilgileri
    self.participants = ko.observableArray([]);

    self.addParticipant = function() {
        self.participants.push({ name: ko.observable(''), age: ko.observable(''), diet: ko.observable(''), health: ko.observable('') });
    };

    self.removeParticipant = function(participant) {
        self.participants.remove(participant);
    };

    // Kisi sayisi icin ayri observable (fiyat hesaplamasi icin)
    self.numberOfPeople = ko.observable(1);

    // Kupon ve indirim
    self.couponCode = ko.observable('');
    self.couponDiscount = ko.observable(0);
    self.appliedDiscounts = ko.observableArray([]);
    self.totalDiscount = ko.observable(0);

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

    // Odeme adimi gosterimi
    self.showPaymentStep = ko.observable(false);

    // Odeme yontemi secimi
    self.selectedPaymentMethod = ko.observable('iyzico');

    // Odeme tipi: 'deposit' (on odeme) veya 'full' (tam odeme)
    self.paymentType = ko.observable('deposit');

    self.selectPaymentMethod = function(method) {
        self.selectedPaymentMethod(method);
    };

    self.selectPaymentType = function(type) {
        self.paymentType(type);
    };

    // Tam odeme tutari (indirimli)
    self.calculateFinalTotal = ko.computed(function() {
        var tour = self.selectedTour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        var total = tour.price * people;
        var discount = self.totalDiscount() || 0;
        return (total - discount).toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    // Simdi odenecek tutar (secime gore)
    self.calculatePayNow = ko.computed(function() {
        var tour = self.selectedTour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        var total = tour.price * people;
        var discount = self.totalDiscount() || 0;
        var finalTotal = total - discount;
        if (self.paymentType() === 'full') {
            return finalTotal.toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
        }
        var percentage = self.getDepositPercentage();
        var deposit = finalTotal * (percentage / 100);
        return deposit.toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    // Modal referanslari
    var reservationModal, watchModal;

    // Seyahat tipi isimleri
    // Tarih formatlama
    self.formatDate = function(dateStr) {
        if (!dateStr) return '';
        var date = new Date(dateStr);
        return date.toLocaleDateString('tr-TR');
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
                self.tours(response.tours);
                self.totalCount(response.totalCount);

                // Promosyon badge'lerini yukle
                var tourIds = response.tours.map(function(t) { return t.id; });
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
        if (!mapContainer) return;

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

        // Koordinatli turlari filtrele
        var toursWithCoords = self.tours().filter(function(t) {
            return t.latitude && t.longitude;
        });

        if (toursWithCoords.length === 0) return;

        var bounds = [];

        toursWithCoords.forEach(function(tour) {
            var marker = L.marker([tour.latitude, tour.longitude]).addTo(self.map);

            // Popup icerigi
            var popupContent = '<div class="tour-popup">' +
                '<strong>' + tour.name + '</strong><br/>' +
                '<i class="bi bi-geo-alt"></i> ' + tour.destination + '<br/>' +
                '<span class="badge bg-primary">' + tour.price.toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'}) + '</span> ' +
                '<small class="text-muted">' + tour.durationDays + ' ' + T('Tour.Days') + '</small><br/>' +
                (tour.reviewCount > 0 ? '<i class="bi bi-star-fill text-warning"></i> ' + tour.averageRating.toFixed(1) + ' (' + tour.reviewCount + ')' : '') +
                '<br/><a class="btn btn-sm btn-outline-primary mt-2" href="/Tours/Details/' + tour.id + '">' +
                '<i class="bi bi-info-circle"></i> ' + T('Tours.Details') + '</a>' +
                '</div>';

            marker.bindPopup(popupContent);
            self.markers.push(marker);
            bounds.push([tour.latitude, tour.longitude]);
        });

        // Haritayi marker'lara sigdir
        if (bounds.length > 0) {
            self.map.fitBounds(bounds, { padding: [20, 20] });
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


    // Detay sayfasina yonlendir
    self.showDetails = function(tour) {
        window.location.href = '/Tours/Details/' + tour.id;
    };


    // Rezervasyon modali
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

        self.selectedTour(tour);
        self.numberOfPeople(1); // Kisi sayisini sifirla
        self.showPaymentStep(false); // Odeme adimini sifirla
        self.paymentType('deposit'); // Odeme tipini sifirla
        self.selectedTourDateId(null);
        self.selectedTourDateObj(null);
        self.selectedDateToken(null);

        // Tur tarihlerini yukle (modal icin)
        if (tour && tour.id) {
            self.loadTourDates(tour.id);
        }

        // Giris yapilmissa kullanici bilgilerini doldur
        if (user) {
            self.reservationData({
                fullName: (user.firstName || '') + ' ' + (user.lastName || ''),
                email: user.email || '',
                phone: user.phone || '',
                numberOfPeople: 1,
                notes: ''
            });
        } else {
            self.reservationData({
                fullName: '',
                email: '',
                phone: '',
                numberOfPeople: 1,
                notes: ''
            });
        }
        reservationModal.show();
    };

    // Fiyat hesaplama fonksiyonlari (computed)
    self.calculateTotal = ko.computed(function() {
        var tour = self.selectedTour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        var total = tour.price * people;
        return total.toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    // On odeme yuzdesi (firma ayarindan veya varsayilan %30)
    self.getDepositPercentage = function() {
        var tour = self.selectedTour();
        if (tour && tour.company && tour.company.depositPercentage) {
            return tour.company.depositPercentage;
        }
        return 30; // Varsayilan
    };

    self.calculateDeposit = ko.computed(function() {
        var tour = self.selectedTour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        var total = tour.price * people;
        var percentage = self.getDepositPercentage();
        var deposit = total * (percentage / 100);
        return deposit.toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    self.calculateRemaining = ko.computed(function() {
        var tour = self.selectedTour();
        var people = self.numberOfPeople() || 1;
        if (!tour) return '\u20BA0';
        var total = tour.price * people;
        var percentage = self.getDepositPercentage();
        var remaining = total * ((100 - percentage) / 100);
        return remaining.toLocaleString('tr-TR', {style: 'currency', currency: 'TRY'});
    });

    // Kupon uygula
    self.applyCoupon = function() {
        var tour = self.selectedTour();
        var code = self.couponCode();
        if (!tour || !code) {
            toastr.warning('Lutfen bir kupon kodu girin');
            return;
        }
        $.ajax({
            url: apiBaseUrl + '/api/tours/' + tour.id + '/calculate-price',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                numberOfPeople: self.numberOfPeople() || 1,
                couponCode: code
            })
        }).done(function(result) {
            if (result.totalDiscount > 0) {
                self.totalDiscount(result.totalDiscount);
                self.appliedDiscounts(result.appliedDiscounts || []);
                var couponDisc = (result.appliedDiscounts || []).filter(function(d) { return d.promotionType === 'Coupon'; });
                self.couponDiscount(couponDisc.length > 0 ? couponDisc[0].discountAmount : 0);
                toastr.success('Kupon uyguland\u0131');
            } else {
                self.couponDiscount(0);
                self.totalDiscount(0);
                self.appliedDiscounts([]);
                toastr.warning('Bu kupon gecerli degil veya indirim uygulanamadi');
            }
        }).fail(function() {
            toastr.error('Kupon dogrulanamadi');
        });
    };

    // Step 1'den Step 2'ye gec (form validasyonu yapip odeme ekranina)
    self.goToPaymentStep = function() {
        // Tarih secimi zorunlu
        if (!self.selectedDateToken()) {
            toastr.warning(T('TourDate.NoSessionSelected') || 'Lutfen bir tarih ve saat secin');
            return;
        }

        var form = document.getElementById('reservationForm');
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        // numberOfPeople'i reservationData'ya kopyala
        var data = self.reservationData();
        data.numberOfPeople = self.numberOfPeople();

        self.showPaymentStep(true);
    };

    // Step 2'den Step 1'e geri don
    self.goBackToForm = function() {
        self.showPaymentStep(false);
    };

    // On odeme yap - gercek API cagrisi
    self.submitPayment = function() {
        var tour = self.selectedTour();
        var data = self.reservationData();

        if (!tour) {
            toastr.error('Tur bilgisi bulunamadi');
            return;
        }

        if (!self.selectedPaymentMethod()) {
            toastr.warning('Lutfen bir odeme yontemi secin');
            return;
        }

        self.isSaving(true);

        // Katilimci bilgilerini JSON olarak hazirla
        var participantInfo = null;
        if (self.participants().length > 0) {
            participantInfo = JSON.stringify(self.participants().map(function(p) {
                return { name: p.name(), age: p.age(), diet: p.diet(), health: p.health() };
            }));
        }

        var requestData = {
            tourId: tour.id,
            dateToken: self.selectedDateToken() || null,
            fullName: data.fullName,
            email: data.email,
            phone: data.phone,
            numberOfPeople: self.numberOfPeople(),
            notes: data.notes || '',
            couponCode: self.couponCode() || null,
            participantInfo: participantInfo,
            payFullAmount: self.paymentType() === 'full'
        };

        $.ajax({
            url: apiBaseUrl + '/api/reservations/public/create',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function(response) {
                if (response.success && response.paymentPageUrl) {
                    // Odeme sayfasina yonlendir
                    reservationModal.hide();
                    self.showPaymentStep(false);
                    toastr.info('Odeme sayfasina yonlendiriliyorsunuz...');

                    // iyzico odeme sayfasina yonlendir
                    setTimeout(function() {
                        window.location.href = response.paymentPageUrl;
                    }, 500);
                } else {
                    toastr.error(response.message || 'Rezervasyon olusturulamadi');
                    self.isSaving(false);
                }
            },
            error: function(xhr) {
                var msg = 'Bir hata olustu';
                if (xhr.responseJSON) {
                    msg = xhr.responseJSON.message || xhr.responseJSON.error || msg;
                }
                toastr.error(msg);
                self.isSaving(false);
            }
        });
    };

    // Kategori ve Zorluk helper fonksiyonlari
    var difficultyMap = {
        0: { name: T('TourDifficulty.Easy') || 'Kolay', css: 'bg-success', icon: 'bi-emoji-smile' },
        1: { name: T('TourDifficulty.Moderate') || 'Orta', css: 'bg-info', icon: 'bi-emoji-neutral' },
        2: { name: T('TourDifficulty.Challenging') || 'Zor', css: 'bg-warning text-dark', icon: 'bi-emoji-frown' },
        3: { name: T('TourDifficulty.Expert') || 'Uzman', css: 'bg-danger', icon: 'bi-exclamation-triangle' }
    };

    self.getDifficultyName = function(id) {
        return difficultyMap[id] ? difficultyMap[id].name : '';
    };
    self.getDifficultyBadgeClass = function(id) {
        return difficultyMap[id] ? difficultyMap[id].css : 'bg-secondary';
    };

    self.getCategoryName = function(id) {
        var cat = self.categories().find(function(c) { return c.id === id; });
        return cat ? (T(cat.nameResourceKey) || cat.systemName) : '';
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
        reservationModal = new bootstrap.Modal(document.getElementById('reservationModal'));
        watchModal = new bootstrap.Modal(document.getElementById('watchModal'));
        self.loadData();
        self.loadCompanies();
        self.loadCategories();
    });
}

var toursVM = new ToursViewModel();
ko.applyBindings(toursVM, document.getElementById('toursApp'));
