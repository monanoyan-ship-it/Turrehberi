function FaqViewModel() {
    var self = this;

    // Observables
    self.faqs = ko.observableArray([]);
    self.selectedCategory = ko.observable('');
    self.categories = ko.observableArray(['']);
    self.apiFaqs = [];

    // Computed
    self.filteredFaqs = ko.computed(function () {
        var cat = self.selectedCategory();
        if (!cat) return self.faqs();
        return self.faqs().filter(function (f) { return f.category === cat; });
    });

    // Filter
    self.filterByCategory = function (category) {
        self.selectedCategory(category);
    };

    self.defaultFaqs = function () {
        return [
            {
                id: 'default-booking',
                category: TL('Faq.Category.Booking', 'Rezervasyon'),
                question: TL('Faq.DefaultBookingQuestion', 'Nasil rezervasyon yapabilirim?'),
                answer: TL('Faq.DefaultBookingAnswer', 'Tur detay sayfasindan tarih secip rezervasyon adimlarini takip ederek odeme sayfasina gecebilirsiniz.')
            },
            {
                id: 'default-payment',
                category: TL('Faq.Category.Payment', 'Odeme'),
                question: TL('Faq.DefaultPaymentQuestion', 'On odeme ve tam odeme farki nedir?'),
                answer: TL('Faq.DefaultPaymentAnswer', 'On odeme ile yerinizi ayirtirsiniz, kalan tutar tura katilimda tahsil edilir. Tam odeme ile tum tutari hemen odeyebilirsiniz.')
            },
            {
                id: 'default-company',
                category: TL('Faq.Category.Company', 'Firma'),
                question: TL('Faq.DefaultCompanyQuestion', 'Firma olarak nasil kayit olurum?'),
                answer: TL('Faq.DefaultCompanyAnswer', 'Kayit sayfasinda Tur Sirketi secenegini secip firma bilgilerinizi gonderin. Onay sonrasi turlarinizi yayinlayabilirsiniz.')
            }
        ];
    };

    self.applyFaqs = function (data) {
        var source = data && data.length > 0 ? data : self.defaultFaqs();
        self.faqs(source);
        var cats = [''];
        source.forEach(function (f) {
            if (f.category && cats.indexOf(f.category) === -1) {
                cats.push(f.category);
            }
        });
        self.categories(cats);
        if (self.selectedCategory() && cats.indexOf(self.selectedCategory()) === -1) {
            self.selectedCategory('');
        }
    };

    // Load data
    self.loadData = function () {
        $.ajax({
            url: apiBaseUrl + '/api/faqs/public',
            method: 'GET'
        }).done(function (data) {
            self.apiFaqs = data || [];
            self.applyFaqs(self.apiFaqs);
        }).fail(function () {
            self.apiFaqs = [];
            self.applyFaqs([]);
        });
    };

    // Init
    $(document).ready(function () {
        self.loadData();
    });

    $(document).on('languageChanged', function () {
        self.applyFaqs(self.apiFaqs);
    });
}

ko.applyBindings(new FaqViewModel(), document.getElementById('faqApp'));
