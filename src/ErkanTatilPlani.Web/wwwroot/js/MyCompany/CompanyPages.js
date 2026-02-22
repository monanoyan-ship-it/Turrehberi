function CompanyPagesViewModel() {
    var self = this;

    // TODO: Gercek auth sistemi kuruldugunda session'dan alinacak
    var companyId = 1;

    // Data
    self.pages = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);

    // Form
    self.isEditing = ko.observable(false);
    self.selectedPage = ko.observable(null);
    self.formData = ko.observable({
        title: '', content: '', icon: '', isPublished: 'false', metaTitle: '', metaDescription: ''
    });

    // Helpers
    self.formatDate = function (dateStr) {
        if (!dateStr) return '-';
        var d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR');
    };

    // Load data
    self.loadData = function () {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + companyId + '/pages',
            method: 'GET',
        }).done(function (data) {
            self.pages(data);
            self.isLoading(false);
        }).fail(function () {
            toastr.error('Veriler yuklenemedi');
            self.isLoading(false);
        });
    };

    // Modal operations
    var formModal, deleteModal, summernoteInitialized = false;

    self.openCreateModal = function () {
        self.isEditing(false);
        self.formData({
            title: '', content: '', icon: '', isPublished: 'false', metaTitle: '', metaDescription: ''
        });
        initSummernote('');
        formModal.show();
    };

    self.openEditModal = function (page) {
        self.isEditing(true);
        self.selectedPage(page);

        // Tam icerigi cek
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + companyId + '/pages/' + page.id,
            method: 'GET',
        }).done(function (data) {
            self.formData({
                title: data.title,
                content: data.content || '',
                icon: data.icon || '',
                isPublished: String(data.isPublished),
                metaTitle: data.metaTitle || '',
                metaDescription: data.metaDescription || ''
            });
            initSummernote(data.content || '');
            formModal.show();
        }).fail(function () {
            self.formData({
                title: page.title, content: '', icon: page.icon || '',
                isPublished: String(page.isPublished), metaTitle: page.metaTitle || '', metaDescription: page.metaDescription || ''
            });
            initSummernote('');
            formModal.show();
        });
    };

    self.openDeleteModal = function (page) {
        self.selectedPage(page);
        deleteModal.show();
    };

    function initSummernote(content) {
        var $editor = $('#contentEditor');
        if (summernoteInitialized) {
            $editor.summernote('destroy');
        }
        $editor.summernote({
            height: 300,
            toolbar: [
                ['style', ['style']],
                ['font', ['bold', 'italic', 'underline', 'strikethrough']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['insert', ['link', 'picture', 'table']],
                ['view', ['codeview']]
            ],
            callbacks: {
                onChange: function (contents) {
                    var fd = self.formData();
                    fd.content = contents;
                    self.formData(fd);
                }
            }
        });
        $editor.summernote('code', content);
        summernoteInitialized = true;
    }

    // Save
    self.savePage = function () {
        var fd = self.formData();
        if (!fd.title) {
            toastr.warning('Baslik zorunludur');
            return;
        }

        if (summernoteInitialized) {
            fd.content = $('#contentEditor').summernote('code');
        }

        if (!fd.content || fd.content === '<p><br></p>') {
            toastr.warning('Icerik zorunludur');
            return;
        }

        self.isSaving(true);

        var pageData = {
            title: fd.title,
            content: fd.content,
            icon: fd.icon || '',
            isPublished: fd.isPublished === 'true',
            metaTitle: fd.metaTitle || null,
            metaDescription: fd.metaDescription || null
        };

        if (self.isEditing()) {
            $.ajax({
                url: apiBaseUrl + '/api/companies/' + companyId + '/pages/' + self.selectedPage().id,
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(pageData)
            }).done(function () {
                toastr.success('Sayfa guncellendi');
                formModal.hide();
                self.loadData();
                self.isSaving(false);
            }).fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Guncelleme basarisiz');
                self.isSaving(false);
            });
        } else {
            $.ajax({
                url: apiBaseUrl + '/api/companies/' + companyId + '/pages',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(pageData)
            }).done(function () {
                toastr.success('Sayfa olusturuldu');
                formModal.hide();
                self.loadData();
                self.isSaving(false);
            }).fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Olusturma basarisiz');
                self.isSaving(false);
            });
        }
    };

    // Delete
    self.deletePage = function () {
        if (!self.selectedPage()) return;
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + companyId + '/pages/' + self.selectedPage().id,
            method: 'DELETE',
        }).done(function () {
            toastr.success('Sayfa silindi');
            deleteModal.hide();
            self.loadData();
            self.isSaving(false);
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Silme basarisiz');
            self.isSaving(false);
        });
    };

    // Reorder
    self.moveUp = function (page) {
        var arr = self.pages();
        var idx = arr.indexOf(page);
        if (idx <= 0) return;
        // Swap
        var ids = arr.map(function (p) { return p.id; });
        var tmp = ids[idx];
        ids[idx] = ids[idx - 1];
        ids[idx - 1] = tmp;
        saveOrder(ids);
    };

    self.moveDown = function (page) {
        var arr = self.pages();
        var idx = arr.indexOf(page);
        if (idx >= arr.length - 1) return;
        var ids = arr.map(function (p) { return p.id; });
        var tmp = ids[idx];
        ids[idx] = ids[idx + 1];
        ids[idx + 1] = tmp;
        saveOrder(ids);
    };

    function saveOrder(ids) {
        $.ajax({
            url: apiBaseUrl + '/api/companies/' + companyId + '/pages/reorder',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(ids)
        }).done(function () {
            toastr.success('Siralama guncellendi');
            self.loadData();
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Siralama basarisiz');
        });
    }

    // Init
    $(document).ready(function () {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));

        document.getElementById('formModal').addEventListener('hidden.bs.modal', function () {
            if (summernoteInitialized) {
                $('#contentEditor').summernote('destroy');
                summernoteInitialized = false;
            }
        });

        self.loadData();
    });
}

ko.applyBindings(new CompanyPagesViewModel(), document.getElementById('companyPagesApp'));
