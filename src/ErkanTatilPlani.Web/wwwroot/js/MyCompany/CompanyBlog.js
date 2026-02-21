function CompanyBlogViewModel() {
    var self = this;

    // TODO: Gercek auth sistemi kuruldugunda session'dan alinacak
    var companyId = 1;

    // Data
    self.allPosts = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.searchQuery = ko.observable('');
    self.filterStatus = ko.observable(null);

    // Form
    self.isEditing = ko.observable(false);
    self.selectedPost = ko.observable(null);
    self.formData = ko.observable({
        title: '', summary: '', content: '', imageUrl: '', categoryId: '0',
        statusId: '0', tags: '', metaTitle: '', metaDescription: ''
    });

    // Filtered posts (client-side filtering)
    self.filteredPosts = ko.computed(function () {
        var posts = self.allPosts();
        var status = self.filterStatus();
        var search = (self.searchQuery() || '').toLowerCase();

        if (status !== null) {
            posts = posts.filter(function (p) { return p.statusId === status; });
        }
        if (search) {
            posts = posts.filter(function (p) { return p.title.toLowerCase().indexOf(search) >= 0; });
        }
        return posts;
    });

    // Helpers
    var categoryNames = {
        0: 'Seyahat Ipuclari', 1: 'Gezi Rehberleri', 2: 'Haberler',
        3: 'Kultur & Yasam', 4: 'Yeme & Icme', 5: 'Macera & Doga'
    };
    var categoryBadgeClasses = {
        0: 'bg-info', 1: 'bg-success', 2: 'bg-primary',
        3: 'bg-warning text-dark', 4: 'bg-danger', 5: 'bg-dark'
    };
    var statusNames = { 0: 'Taslak', 1: 'Yayinda', 2: 'Arsiv' };
    var statusBadgeClasses = { 0: 'bg-secondary', 1: 'bg-success', 2: 'bg-warning text-dark' };

    self.getCategoryName = function (id) { return categoryNames[id] || 'Diger'; };
    self.getCategoryBadgeClass = function (id) { return categoryBadgeClasses[id] || 'bg-secondary'; };
    self.getStatusName = function (id) { return statusNames[id] || 'Bilinmiyor'; };
    self.getStatusBadgeClass = function (id) { return statusBadgeClasses[id] || 'bg-secondary'; };

    self.formatDate = function (dateStr) {
        if (!dateStr) return '-';
        var d = new Date(dateStr);
        return d.toLocaleDateString('tr-TR');
    };

    // Load data
    self.loadData = function () {
        self.isLoading(true);
        $.ajax({
            url: apiBaseUrl + '/api/blogs/company/' + companyId + '/manage',
            method: 'GET',
        }).done(function (data) {
            self.allPosts(data);
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
            title: '', summary: '', content: '', imageUrl: '', categoryId: '0',
            statusId: '0', tags: '', metaTitle: '', metaDescription: ''
        });
        initSummernote('');
        formModal.show();
    };

    self.openEditModal = function (post) {
        self.isEditing(true);
        self.selectedPost(post);

        // Icerik yuklenmesi icin slug ile tekrar cek
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + post.slug,
            method: 'GET'
        }).done(function (data) {
            self.formData({
                title: data.title,
                summary: data.summary || '',
                content: '',
                imageUrl: data.imageUrl || '',
                categoryId: String(data.categoryId),
                statusId: String(data.statusId),
                tags: data.tags || '',
                metaTitle: data.metaTitle || '',
                metaDescription: data.metaDescription || ''
            });
            initSummernote(data.content || '');
            formModal.show();
        }).fail(function () {
            self.formData({
                title: post.title, summary: post.summary || '', content: '',
                imageUrl: post.imageUrl || '', categoryId: String(post.categoryId),
                statusId: String(post.statusId), tags: post.tags || '',
                metaTitle: '', metaDescription: ''
            });
            initSummernote('');
            formModal.show();
        });
    };

    self.openDeleteModal = function (post) {
        self.selectedPost(post);
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
                ['insert', ['link', 'picture']],
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
    self.savePost = function () {
        var fd = self.formData();
        if (!fd.title || !fd.summary) {
            toastr.warning('Baslik ve ozet zorunludur');
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

        var postData = {
            title: fd.title,
            summary: fd.summary,
            content: fd.content,
            imageUrl: fd.imageUrl || null,
            categoryId: parseInt(fd.categoryId),
            statusId: parseInt(fd.statusId),
            tags: fd.tags || null,
            metaTitle: fd.metaTitle || null,
            metaDescription: fd.metaDescription || null
        };

        if (self.isEditing()) {
            postData.id = self.selectedPost().id;
            $.ajax({
                url: apiBaseUrl + '/api/blogs/' + self.selectedPost().id,
                method: 'PUT',
                contentType: 'application/json',
                data: JSON.stringify(postData)
            }).done(function () {
                toastr.success('Yazi guncellendi');
                formModal.hide();
                self.loadData();
                self.isSaving(false);
            }).fail(function (xhr) {
                toastr.error(xhr.responseJSON?.message || 'Guncelleme basarisiz');
                self.isSaving(false);
            });
        } else {
            $.ajax({
                url: apiBaseUrl + '/api/blogs',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(postData)
            }).done(function () {
                toastr.success('Yazi olusturuldu');
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
    self.deletePost = function () {
        if (!self.selectedPost()) return;
        self.isSaving(true);
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + self.selectedPost().id,
            method: 'DELETE',
        }).done(function () {
            toastr.success('Yazi silindi');
            deleteModal.hide();
            self.loadData();
            self.isSaving(false);
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Silme basarisiz');
            self.isSaving(false);
        });
    };

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

ko.applyBindings(new CompanyBlogViewModel(), document.getElementById('companyBlogApp'));
