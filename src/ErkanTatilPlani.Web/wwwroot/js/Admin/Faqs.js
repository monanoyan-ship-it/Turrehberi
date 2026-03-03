function FaqsViewModel() {
    var self = this;

    // Observables
    self.faqs = ko.observableArray([]);
    self.isEditing = ko.observable(false);
    self.editingId = ko.observable(null);
    self.formData = ko.observable({
        question: ko.observable(''),
        answer: ko.observable(''),
        category: ko.observable(''),
        displayOrder: ko.observable(0)
    });

    // Modal reference
    var faqModal = null;

    // Load data
    self.loadData = function () {
        $.ajax({
            url: apiBaseUrl + '/api/faqs',
            method: 'GET'
        }).done(function (data) {
            self.faqs(data);
        }).fail(function () {
            toastr.error('SSS verileri yuklenemedi');
        });
    };

    // Reset form
    self.resetForm = function () {
        self.formData().question('');
        self.formData().answer('');
        self.formData().category('');
        self.formData().displayOrder(0);
        self.isEditing(false);
        self.editingId(null);
    };

    // Open add modal
    self.openAddModal = function () {
        self.resetForm();
        faqModal.show();
    };

    // Open edit modal
    self.openEditModal = function (faq) {
        self.isEditing(true);
        self.editingId(faq.id);
        self.formData().question(faq.question);
        self.formData().answer(faq.answer);
        self.formData().category(faq.category || '');
        self.formData().displayOrder(faq.displayOrder);
        faqModal.show();
    };

    // Save
    self.saveFaq = function () {
        var data = {
            question: self.formData().question(),
            answer: self.formData().answer(),
            category: self.formData().category() || null,
            displayOrder: parseInt(self.formData().displayOrder()) || 0
        };

        if (!data.question || !data.answer) {
            toastr.warning('Soru ve cevap alanlari zorunludur');
            return;
        }

        if (self.isEditing()) {
            $.ajax({
                url: apiBaseUrl + '/api/faqs/' + self.editingId(),
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(data)
            }).done(function () {
                toastr.success('Soru guncellendi');
                faqModal.hide();
                self.loadData();
            }).fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Guncelleme basarisiz');
            });
        } else {
            $.ajax({
                url: apiBaseUrl + '/api/faqs',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data)
            }).done(function () {
                toastr.success('Soru eklendi');
                faqModal.hide();
                self.loadData();
            }).fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Ekleme basarisiz');
            });
        }
    };

    // Delete
    self.deleteFaq = function (faq) {
        if (!confirm('Bu soruyu silmek istediginize emin misiniz?')) return;

        $.ajax({
            url: apiBaseUrl + '/api/faqs/' + faq.id,
            method: 'DELETE'
        }).done(function () {
            toastr.success('Soru silindi');
            self.loadData();
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Silme basarisiz');
        });
    };

    // Init
    $(document).ready(function () {
        faqModal = new bootstrap.Modal(document.getElementById('faqModal'));
        self.loadData();
    });
}

ko.applyBindings(new FaqsViewModel(), document.getElementById('faqsApp'));
