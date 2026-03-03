function ContactViewModel() {
    var self = this;

    self.name = ko.observable('');
    self.email = ko.observable('');
    self.subject = ko.observable('');
    self.message = ko.observable('');
    self.sending = ko.observable(false);

    self.canSend = ko.computed(function() {
        return self.name().trim() && self.email().trim() && self.subject() && self.message().trim() && !self.sending();
    });

    self.send = function() {
        if (!self.canSend()) return;
        self.sending(true);

        $.ajax({
            url: apiBaseUrl + '/api/support',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                name: self.name(),
                email: self.email(),
                subject: self.subject(),
                message: self.message()
            })
        }).done(function() {
            toastr.success(T('Contact.SuccessMessage') || 'Mesajiniz basariyla gonderildi. En kisa surede donus yapacagiz.');
            self.name('');
            self.email('');
            self.subject('');
            self.message('');
        }).fail(function() {
            toastr.error(T('Contact.ErrorMessage') || 'Mesaj gonderilemedi. Lutfen tekrar deneyin.');
        }).always(function() {
            self.sending(false);
        });
    };

    // Pre-fill if logged in
    var user = getUser();
    if (user) {
        if (user.firstName) self.name(user.firstName + ' ' + (user.lastName || ''));
        if (user.email) self.email(user.email);
    }
}

ko.applyBindings(new ContactViewModel(), document.getElementById('contactApp'));
