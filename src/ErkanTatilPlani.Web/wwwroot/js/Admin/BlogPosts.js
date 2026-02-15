function AdminBlogPostsViewModel() {
    var self = this;

    // Data
    self.posts = ko.observableArray([]);
    self.isLoading = ko.observable(true);
    self.isSaving = ko.observable(false);
    self.searchQuery = ko.observable('');
    self.filterStatus = ko.observable(null);
    self.currentPage = ko.observable(1);
    self.totalPages = ko.observable(1);
    self.totalCount = ko.observable(0);

    // Form
    self.isEditing = ko.observable(false);
    self.selectedPost = ko.observable(null);
    self.formData = ko.observable({
        title: '', summary: '', content: '', imageUrl: '', categoryId: '0',
        statusId: '0', tags: '', metaTitle: '', metaDescription: ''
    });

    // Computed
    self.pageNumbers = ko.computed(function () {
        var pages = [];
        var total = self.totalPages();
        var current = self.currentPage();
        var start = Math.max(1, current - 2);
        var end = Math.min(total, current + 2);
        for (var i = start; i <= end; i++) pages.push(i);
        return pages;
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
        var params = { page: self.currentPage(), pageSize: 20 };
        if (self.filterStatus() !== null) params.statusId = self.filterStatus();
        if (self.searchQuery()) params.search = self.searchQuery();

        $.ajax({
            url: apiBaseUrl + '/api/blogs/admin',
            method: 'GET',
            data: params
        }).done(function (data) {
            self.posts(data.posts);
            self.totalPages(data.totalPages);
            self.totalCount(data.totalCount);
            self.isLoading(false);
        }).fail(function () {
            toastr.error('Veriler yuklenemedi');
            self.isLoading(false);
        });
    };

    // Status update
    self.updateStatus = function (postId, statusId) {
        $.ajax({
            url: apiBaseUrl + '/api/blogs/admin/' + postId + '/status',
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ statusId: statusId })
        }).done(function () {
            var statusMsg = statusId === 1 ? 'Yazi yayinlandi' : 'Yazi arsivlendi';
            toastr.success(statusMsg);
            self.loadData();
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.message || 'Islem basarisiz');
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
        self.formData({
            title: post.title,
            summary: post.summary || '',
            content: '',
            imageUrl: post.imageUrl || '',
            categoryId: String(post.categoryId),
            statusId: String(post.statusId),
            tags: post.tags || '',
            metaTitle: post.metaTitle || '',
            metaDescription: post.metaDescription || ''
        });

        // Icerik yuklenmesi icin slug ile tekrar cek
        $.ajax({
            url: apiBaseUrl + '/api/blogs/' + post.slug,
            method: 'GET'
        }).done(function (data) {
            initSummernote(data.content || '');
            formModal.show();
        }).fail(function () {
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

        // Summernote content
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
            method: 'DELETE'
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

    // Pagination
    self.changePage = function (page) {
        if (page < 1 || page > self.totalPages()) return;
        self.currentPage(page);
        self.loadData();
    };

    // Watchers
    var searchTimeout;
    self.searchQuery.subscribe(function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            self.currentPage(1);
            self.loadData();
        }, 300);
    });

    self.filterStatus.subscribe(function () {
        self.currentPage(1);
        self.loadData();
    });

    // Init
    $(document).ready(function () {
        formModal = new bootstrap.Modal(document.getElementById('formModal'));
        deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));

        // Summernote destroy on modal close
        document.getElementById('formModal').addEventListener('hidden.bs.modal', function () {
            if (summernoteInitialized) {
                $('#contentEditor').summernote('destroy');
                summernoteInitialized = false;
            }
        });

        self.loadData();
    });
}

ko.applyBindings(new AdminBlogPostsViewModel(), document.getElementById('adminBlogPostsApp'));
