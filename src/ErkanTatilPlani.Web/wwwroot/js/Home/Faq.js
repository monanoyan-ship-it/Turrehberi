function FaqViewModel() {
    var self = this;

    // Observables
    self.faqs = ko.observableArray([]);
    self.selectedCategory = ko.observable('');
    self.categories = ko.observableArray(['']);

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

    // Load data
    self.loadData = function () {
        $.ajax({
            url: apiBaseUrl + '/api/faqs/public',
            method: 'GET'
        }).done(function (data) {
            self.faqs(data);
            var cats = [''];
            data.forEach(function (f) {
                if (f.category && cats.indexOf(f.category) === -1) {
                    cats.push(f.category);
                }
            });
            self.categories(cats);
        }).fail(function () {
            toastr.error('SSS verileri yuklenemedi');
        });
    };

    // Init
    $(document).ready(function () {
        self.loadData();
    });
}

ko.applyBindings(new FaqViewModel(), document.getElementById('faqApp'));
