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
    var categoryKeys = {
        0: 'Blog.Category.TravelTips', 1: 'Blog.Category.Destinations', 2: 'Blog.Category.News',
        3: 'Blog.Category.Culture', 4: 'Blog.Category.FoodAndDrink', 5: 'Blog.Category.Adventure'
    };
    var categoryBadgeClasses = {
        0: 'bg-info', 1: 'bg-success', 2: 'bg-primary',
        3: 'bg-warning text-dark', 4: 'bg-danger', 5: 'bg-dark'
    };
    var statusKeys = { 0: 'Blog.Status.Draft', 1: 'Blog.Status.Published', 2: 'Blog.Status.Archived' };
    var statusBadgeClasses = { 0: 'bg-secondary', 1: 'bg-success', 2: 'bg-warning text-dark' };

    self.getCategoryName = function (id) { return categoryKeys[id] ? T(categoryKeys[id]) : T('Blog.Category.Other'); };
    self.getCategoryBadgeClass = function (id) { return categoryBadgeClasses[id] || 'bg-secondary'; };
    self.getStatusName = function (id) { return statusKeys[id] ? T(statusKeys[id]) : T('Common.Unknown'); };
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
            toastr.error(T('Error.DataLoadFailed'));
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
            toastr.warning(T('Validation.TitleSummaryRequired'));
            return;
        }

        if (summernoteInitialized) {
            fd.content = $('#contentEditor').summernote('code');
        }

        if (!fd.content || fd.content === '<p><br></p>') {
            toastr.warning(T('Validation.ContentRequired'));
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
                toastr.success(T('Success.PostUpdated'));
                formModal.hide();
                self.loadData();
                self.isSaving(false);
            }).fail(function (xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
                self.isSaving(false);
            });
        } else {
            $.ajax({
                url: apiBaseUrl + '/api/blogs',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(postData)
            }).done(function () {
                toastr.success(T('Success.PostCreated'));
                formModal.hide();
                self.loadData();
                self.isSaving(false);
            }).fail(function (xhr) {
                toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
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
            toastr.success(T('Success.PostDeleted'));
            deleteModal.hide();
            self.loadData();
            self.isSaving(false);
        }).fail(function (xhr) {
            toastr.error(T(xhr.responseJSON?.message) || T('Common.Error'));
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

$(document).ready(function() {
    ko.applyBindings(new CompanyBlogViewModel(), document.getElementById('companyBlogApp'));
});
