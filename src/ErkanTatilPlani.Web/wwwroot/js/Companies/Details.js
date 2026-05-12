function CompanyProfileViewModel() {
    var self = this;
    var slug = document.getElementById('companyProfileApp').getAttribute('data-slug');

    self.isLoading = ko.observable(true);
    self.error = ko.observable(null);
    self.company = ko.observable(null);
    self.tours = ko.observableArray([]);
    self.tourCount = ko.observable(0);
    self.stats = ko.observable({ totalReviews: 0, averageRating: 0, ratingDistribution: { fiveStar: 0, fourStar: 0, threeStar: 0, twoStar: 0, oneStar: 0 } });
    self.recentReviews = ko.observableArray([]);
    self.companyPages = ko.observableArray([]);
    self.companyBlogPosts = ko.observableArray([]);

    // Schema.org JSON-LD for SEO
    self.schemaOrgJson = ko.computed(function() {
        if (!self.company()) return '{}';
        var c = self.company();
        var schema = {
            "@context": "https://schema.org",
            "@type": "TravelAgency",
            "name": c.name,
            "description": c.description,
            "url": window.location.href,
            "logo": c.logoUrl,
            "image": c.coverImageUrl,
            "telephone": c.phone,
            "email": c.email,
            "address": {
                "@type": "PostalAddress",
                "streetAddress": c.address,
                "addressLocality": c.city,
                "addressCountry": "TR"
            }
        };
        if (c.foundedYear) schema.foundingDate = c.foundedYear.toString();
        if (self.stats().totalReviews > 0) {
            schema.aggregateRating = {
                "@type": "AggregateRating",
                "ratingValue": self.stats().averageRating,
                "reviewCount": self.stats().totalReviews,
                "bestRating": 5,
                "worstRating": 1
            };
        }
        return JSON.stringify(schema);
    });

    // Update page meta tags
    self.updateMetaTags = function() {
        if (!self.company()) return;
        var c = self.company();
        var description = c.description || c.tagline || c.name || '';

        // Title
        document.title = (c.metaTitle || c.name) + ' - Erkan Tatil Plani';

        // Meta Description
        var metaDesc = document.querySelector('meta[name="description"]');
        if (!metaDesc) {
            metaDesc = document.createElement('meta');
            metaDesc.name = 'description';
            document.head.appendChild(metaDesc);
        }
        metaDesc.content = c.metaDescription || description.substring(0, 160);

        // Open Graph
        self.setMetaTag('og:title', c.metaTitle || c.name);
        self.setMetaTag('og:description', c.metaDescription || description.substring(0, 160));
        self.setMetaTag('og:image', c.coverImageUrl || c.logoUrl);
        self.setMetaTag('og:url', window.location.href);
        self.setMetaTag('og:type', 'business.business');

        // Twitter Card
        self.setMetaTag('twitter:card', 'summary_large_image');
        self.setMetaTag('twitter:title', c.metaTitle || c.name);
        self.setMetaTag('twitter:description', c.metaDescription || description.substring(0, 160));
        self.setMetaTag('twitter:image', c.coverImageUrl || c.logoUrl);
    };

    self.setMetaTag = function(property, content) {
        var meta = document.querySelector('meta[property="' + property + '"]') ||
                   document.querySelector('meta[name="' + property + '"]');
        if (!meta) {
            meta = document.createElement('meta');
            if (property.startsWith('og:')) {
                meta.setAttribute('property', property);
            } else {
                meta.name = property;
            }
            document.head.appendChild(meta);
        }
        meta.content = content || '';
    };

    self.value = function(obj, camelName, pascalName, fallback) {
        if (!obj) return fallback;
        if (obj[camelName] !== undefined && obj[camelName] !== null) return obj[camelName];
        if (pascalName && obj[pascalName] !== undefined && obj[pascalName] !== null) return obj[pascalName];
        return fallback;
    };

    self.normalizeCompany = function(company) {
        company = company || {};
        return {
            id: self.value(company, 'id', 'Id', 0),
            name: self.value(company, 'name', 'Name', ''),
            slug: self.value(company, 'slug', 'Slug', slug),
            description: self.value(company, 'description', 'Description', ''),
            tagline: self.value(company, 'tagline', 'Tagline', ''),
            email: self.value(company, 'email', 'Email', ''),
            phone: self.value(company, 'phone', 'Phone', ''),
            address: self.value(company, 'address', 'Address', ''),
            city: self.value(company, 'city', 'City', ''),
            website: self.value(company, 'website', 'Website', ''),
            logoUrl: self.value(company, 'logoUrl', 'LogoUrl', ''),
            coverImageUrl: self.value(company, 'coverImageUrl', 'CoverImageUrl', ''),
            foundedYear: self.value(company, 'foundedYear', 'FoundedYear', null),
            socialLinks: self.value(company, 'socialLinks', 'SocialLinks', ''),
            metaTitle: self.value(company, 'metaTitle', 'MetaTitle', ''),
            metaDescription: self.value(company, 'metaDescription', 'MetaDescription', '')
        };
    };

    self.normalizeTours = function(tours) {
        return (tours || []).map(function(t) {
            return {
                id: self.value(t, 'id', 'Id', 0),
                name: self.value(t, 'name', 'Name', ''),
                description: self.value(t, 'description', 'Description', ''),
                destination: self.value(t, 'destination', 'Destination', ''),
                price: Number(self.value(t, 'price', 'Price', 0)) || 0,
                durationDays: Number(self.value(t, 'durationDays', 'DurationDays', 0)) || 0,
                maxCapacity: self.value(t, 'maxCapacity', 'MaxCapacity', null),
                imageUrl: self.value(t, 'imageUrl', 'ImageUrl', ''),
                isFeatured: !!self.value(t, 'isFeatured', 'IsFeatured', false),
                averageRating: Number(self.value(t, 'averageRating', 'AverageRating', 0)) || 0,
                reviewCount: Number(self.value(t, 'reviewCount', 'ReviewCount', 0)) || 0
            };
        });
    };

    self.normalizeStats = function(stats) {
        stats = stats || {};
        var ratingDistribution = self.value(stats, 'ratingDistribution', 'RatingDistribution', {}) || {};
        return {
            totalReviews: Number(self.value(stats, 'totalReviews', 'TotalReviews', 0)) || 0,
            averageRating: Number(self.value(stats, 'averageRating', 'AverageRating', 0)) || 0,
            ratingDistribution: {
                fiveStar: Number(self.value(ratingDistribution, 'fiveStar', 'FiveStar', 0)) || 0,
                fourStar: Number(self.value(ratingDistribution, 'fourStar', 'FourStar', 0)) || 0,
                threeStar: Number(self.value(ratingDistribution, 'threeStar', 'ThreeStar', 0)) || 0,
                twoStar: Number(self.value(ratingDistribution, 'twoStar', 'TwoStar', 0)) || 0,
                oneStar: Number(self.value(ratingDistribution, 'oneStar', 'OneStar', 0)) || 0
            }
        };
    };

    // Load company profile
    self.loadProfile = function() {
        self.isLoading(true);
        self.error(null);

        $.ajax({
            url: apiBaseUrl + '/api/companies/profile/' + slug,
            method: 'GET'
        })
        .done(function(data) {
            var company = self.normalizeCompany(data.company || data.Company || data);
            var tours = self.normalizeTours(data.tours || data.Tours || []);
            self.company(company);
            self.tours(tours);
            self.tourCount(Number(data.tourCount || data.TourCount || tours.length) || tours.length);
            self.stats(self.normalizeStats(data.stats || data.Stats));
            self.recentReviews(data.recentReviews || data.RecentReviews || []);
            self.isLoading(false);

            // Update meta tags after data loads
            self.updateMetaTags();

            if (tours.length === 0 && company.id) {
                self.loadCompanyTours(company.id);
            }

            // Firma sayfalarini yukle
            $.ajax({
                url: apiBaseUrl + '/api/companies/pages/' + company.slug,
                method: 'GET'
            }).done(function(pages) {
                self.companyPages(pages);
            });

            // Firma blog yazilarini yukle (son 4)
            $.ajax({
                url: apiBaseUrl + '/api/blogs/company/' + company.id,
                method: 'GET',
                data: { page: 1, pageSize: 4 }
            }).done(function(blogData) {
                self.companyBlogPosts(blogData.posts || []);
            });

            // Render share buttons
            var shareUrl = window.location.href;
            var shareText = company.name + ' - ' + (company.tagline || 'Tur Firmasi') + ' | Erkan Tatil Plani';
            $('#companyShareButtons').html(SocialShare.renderButtons({
                url: shareUrl,
                text: shareText,
                size: 'sm',
                platforms: ['facebook', 'twitter', 'whatsapp', 'telegram', 'copy']
            }));
        })
        .fail(function(xhr) {
            self.error(xhr.responseJSON ? xhr.responseJSON.message : 'Firma bulunamadi');
            self.isLoading(false);
        });
    };

    self.loadCompanyTours = function(companyId) {
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + companyId + '/tours',
            method: 'GET'
        }).done(function(data) {
            var tours = self.normalizeTours(data);
            if (tours.length > 0) {
                self.tours(tours);
                self.tourCount(tours.length);
            }
        });
    };

    // Company Contact Form
    self.contactName = ko.observable('');
    self.contactEmail = ko.observable('');
    self.contactMessage = ko.observable('');
    var companyContactModal = null;

    self.canSendContact = ko.computed(function () {
        return !!(self.contactName().trim() && self.contactEmail().trim() && self.contactMessage().trim());
    });

    self.syncContactFromDom = function () {
        var root = $('#companyProfileApp');
        self.contactName((root.find('[data-field="contactName"]').val() || '').trim());
        self.contactEmail((root.find('[data-field="contactEmail"]').val() || '').trim());
        self.contactMessage((root.find('[data-field="contactMessage"]').val() || '').trim());
    };

    self.openContactModal = function () {
        var user = getUser();
        if (user) {
            self.contactName(user.firstName + ' ' + (user.lastName || ''));
            self.contactEmail(user.email);
        }
        if (!companyContactModal) {
            companyContactModal = new bootstrap.Modal(document.getElementById('companyContactModal'));
        }
        companyContactModal.show();
    };

    self.sendContactMessage = function () {
        self.syncContactFromDom();
        if (!self.canSendContact()) return;
        var companyName = self.company() ? self.company().name : '';

        $.ajax({
            url: apiBaseUrl + '/api/support',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                name: self.contactName(),
                email: self.contactEmail(),
                message: self.contactMessage(),
                subject: 'CompanyContact: ' + companyName
            })
        }).done(function () {
            toastr.success(TL('CompanyContact.Success', 'Mesajiniz gonderildi'));
            self.contactMessage('');
            companyContactModal.hide();
        }).fail(function () {
            toastr.error(TL('CompanyContact.Error', 'Mesaj gonderilemedi'));
        });
    };

    // Initialize
    $(document).ready(function() {
        companyContactModal = new bootstrap.Modal(document.getElementById('companyContactModal'));
        if (slug) {
            self.loadProfile();
        } else {
            self.error('Gecersiz firma adresi');
            self.isLoading(false);
        }
    });
}

ko.applyBindings(new CompanyProfileViewModel(), document.getElementById('companyProfileApp'));
