function ContactViewModel() {
    var self = this;

    self.name = ko.observable('');
    self.email = ko.observable('');
    self.subject = ko.observable('');
    self.message = ko.observable('');

    self.canSend = ko.computed(function() {
        return self.name().trim() && self.email().trim() && self.subject() && self.message().trim();
    });

    self.send = function() {
        if (!self.canSend()) return;
        toastr.success(T('Contact.SuccessMessage') || 'Mesajiniz basariyla gonderildi. En kisa surede donus yapacagiz.');
        self.name('');
        self.email('');
        self.subject('');
        self.message('');
    };

    // Pre-fill if logged in
    var token = localStorage.getItem('token');
    if (token) {
        $.ajax({
            url: apiBaseUrl + '/api/auth/me',
            headers: { 'Authorization': 'Bearer ' + token },
            success: function(user) {
                if (user.firstName) self.name(user.firstName + ' ' + (user.lastName || ''));
                if (user.email) self.email(user.email);
            }
        });
    }
}

ko.applyBindings(new ContactViewModel(), document.getElementById('contactApp'));
