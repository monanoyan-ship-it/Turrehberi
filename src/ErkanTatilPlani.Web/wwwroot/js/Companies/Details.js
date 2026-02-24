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

        // Title
        document.title = (c.metaTitle || c.name) + ' - Erkan Tatil Plani';

        // Meta Description
        var metaDesc = document.querySelector('meta[name="description"]');
        if (!metaDesc) {
            metaDesc = document.createElement('meta');
            metaDesc.name = 'description';
            document.head.appendChild(metaDesc);
        }
        metaDesc.content = c.metaDescription || c.description.substring(0, 160);

        // Open Graph
        self.setMetaTag('og:title', c.metaTitle || c.name);
        self.setMetaTag('og:description', c.metaDescription || c.description.substring(0, 160));
        self.setMetaTag('og:image', c.coverImageUrl || c.logoUrl);
        self.setMetaTag('og:url', window.location.href);
        self.setMetaTag('og:type', 'business.business');

        // Twitter Card
        self.setMetaTag('twitter:card', 'summary_large_image');
        self.setMetaTag('twitter:title', c.metaTitle || c.name);
        self.setMetaTag('twitter:description', c.metaDescription || c.description.substring(0, 160));
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

    // Load company profile
    self.loadProfile = function() {
        self.isLoading(true);
        self.error(null);

        $.ajax({
            url: apiBaseUrl + '/api/companies/profile/' + slug,
            method: 'GET'
        })
        .done(function(data) {
            self.company(data.company);
            self.tours(data.tours);
            self.tourCount(data.tourCount);
            self.stats(data.stats);
            self.recentReviews(data.recentReviews);
            self.isLoading(false);

            // Update meta tags after data loads
            self.updateMetaTags();

            // Firma sayfalarini yukle
            $.ajax({
                url: apiBaseUrl + '/api/companies/pages/' + data.company.slug,
                method: 'GET'
            }).done(function(pages) {
                self.companyPages(pages);
            });

            // Firma blog yazilarini yukle (son 4)
            $.ajax({
                url: apiBaseUrl + '/api/blogs/company/' + data.company.id,
                method: 'GET',
                data: { page: 1, pageSize: 4 }
            }).done(function(blogData) {
                self.companyBlogPosts(blogData.posts || []);
            });

            // Render share buttons
            var shareUrl = window.location.href;
            var shareText = data.company.name + ' - ' + (data.company.tagline || 'Tur Firmasi') + ' | Erkan Tatil Plani';
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

    // Initialize
    $(document).ready(function() {
        if (slug) {
            self.loadProfile();
        } else {
            self.error('Gecersiz firma adresi');
            self.isLoading(false);
        }
    });
}

ko.applyBindings(new CompanyProfileViewModel(), document.getElementById('companyProfileApp'));
