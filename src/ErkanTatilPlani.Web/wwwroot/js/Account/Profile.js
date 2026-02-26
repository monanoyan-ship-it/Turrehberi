function ProfileViewModel() {
    var self = this;

    // State
    self.isLoading = ko.observable(true);
    self.isLoggedIn = ko.observable(false);
    self.isSavingProfile = ko.observable(false);
    self.isChangingPassword = ko.observable(false);
    self.isSendingVerification = ko.observable(false);
    self.verificationSent = ko.observable(false);
    self.isUploadingAvatar = ko.observable(false);
    self.isDeletingAvatar = ko.observable(false);

    // User Info
    self.userId = ko.observable(0);
    self.firstName = ko.observable('');
    self.lastName = ko.observable('');
    self.email = ko.observable('');
    self.phone = ko.observable('');
    self.address = ko.observable('');
    self.birthDate = ko.observable(null);
    self.avatarUrl = ko.observable('');
    self.emailVerified = ko.observable(false);
    self.userTypeId = ko.observable(0);
    self.userTypeName = ko.observable('');
    self.companyId = ko.observable(null);
    self.companyName = ko.observable('');
    self.preferredLanguage = ko.observable('tr');
    self.prefEmail = ko.observable(true);
    self.prefWhatsapp = ko.observable(false);
    self.prefSms = ko.observable(false);
    self.isSavingPrefs = ko.observable(false);

    // Loyalty
    self.loyaltyTierIcon = ko.observable('bi-compass');
    self.loyaltyTierName = ko.observable('');
    self.loyaltyTierCss = ko.observable('bg-secondary');
    self.loyaltyCreditBalance = ko.observable(0);

    // Social
    self.bio = ko.observable('');
    self.isProfilePublic = ko.observable(true);
    self.followerCount = ko.observable(0);
    self.followingCount = ko.observable(0);
    self.tripStoryCount = ko.observable(0);

    // Computed
    self.fullName = ko.computed(function() {
        return self.firstName() + ' ' + self.lastName();
    });

    self.birthDateFormatted = ko.computed({
        read: function() {
            if (!self.birthDate()) return '';
            var d = new Date(self.birthDate());
            return d.toISOString().split('T')[0];
        },
        write: function(value) {
            if (value) {
                self.birthDate(new Date(value + 'T00:00:00Z').toISOString());
            } else {
                self.birthDate(null);
            }
        }
    });

    self.hasCustomAvatar = ko.computed(function() {
        var url = self.avatarUrl();
        return url && url.startsWith('/uploads/avatars/');
    });

    // Password Fields
    self.currentPassword = ko.observable('');
    self.newPassword = ko.observable('');
    self.confirmPassword = ko.observable('');


    // Load user data
    self.loadUser = function() {
        var token = localStorage.getItem('token');
        if (!token) {
            self.isLoading(false);
            self.isLoggedIn(false);
            return;
        }

        $.ajax({
            url: apiBaseUrl + '/api/auth/me',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + token },
            success: function(user) {
                self.userId(user.id);
                self.firstName(user.firstName || '');
                self.lastName(user.lastName || '');
                self.email(user.email || '');
                self.phone(user.phone || '');
                self.address(user.address || '');
                self.birthDate(user.birthDate);
                self.avatarUrl(user.avatarUrl || 'https://ui-avatars.com/api/?name=' + encodeURIComponent(user.firstName + '+' + user.lastName) + '&size=120&background=0d6efd&color=fff');
                self.emailVerified(user.emailVerified || false);
                self.userTypeId(user.userTypeId);
                self.userTypeName(T('UserType.' + user.userTypeName));
                self.companyId(user.companyId);
                self.companyName(user.companyName || '');
                self.preferredLanguage(user.preferredLanguage || 'tr');

                // Bildirim tercihleri
                if (user.notificationPreference) {
                    try {
                        var prefs = typeof user.notificationPreference === 'string' ? JSON.parse(user.notificationPreference) : user.notificationPreference;
                        self.prefEmail(prefs.email !== false);
                        self.prefWhatsapp(prefs.whatsapp === true);
                        self.prefSms(prefs.sms === true);
                    } catch(e) {}
                }

                // Social data
                self.bio(user.bio || '');
                self.isProfilePublic(user.isProfilePublic !== false);
                self.followerCount(user.followerCount || 0);
                self.followingCount(user.followingCount || 0);
                self.tripStoryCount(user.tripStoryCount || 0);

                self.isLoggedIn(true);
                self.isLoading(false);
                self.loadLoyalty();
            },
            error: function() {
                localStorage.removeItem('token');
                localStorage.removeItem('user');
                self.isLoading(false);
                self.isLoggedIn(false);
            }
        });
    };

    self.loadLoyalty = function() {
        var token = localStorage.getItem('token');
        if (!token) return;
        $.ajax({
            url: apiBaseUrl + '/api/loyalty/dashboard',
            method: 'GET',
            headers: { 'Authorization': 'Bearer ' + token }
        }).done(function(data) {
            self.loyaltyTierIcon(data.tierIcon || 'bi-compass');
            self.loyaltyTierName(T(data.tierNameKey || ''));
            self.loyaltyTierCss(data.tierCssClass || 'bg-secondary');
            self.loyaltyCreditBalance(data.creditBalance || 0);
        });
    };

    // Save profile
    self.saveProfile = function() {
        if (!self.firstName().trim()) {
            toastr.error(T('Register.Error.FirstNameRequired'));
            return;
        }
        if (!self.lastName().trim()) {
            toastr.error(T('Register.Error.LastNameRequired'));
            return;
        }

        self.isSavingProfile(true);

        var data = {
            firstName: self.firstName().trim(),
            lastName: self.lastName().trim(),
            phone: self.phone() ? self.phone().trim() : null,
            address: self.address() ? self.address().trim() : null,
            birthDate: self.birthDate(),
            bio: self.bio() ? self.bio().trim() : null
        };

        $.ajax({
            url: apiBaseUrl + '/api/auth/profile',
            method: 'PUT',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function(user) {
                // Update local storage
                var storedUser = JSON.parse(localStorage.getItem('user') || '{}');
                storedUser.firstName = user.firstName;
                storedUser.lastName = user.lastName;
                localStorage.setItem('user', JSON.stringify(storedUser));

                // Update avatar URL with new name
                self.avatarUrl(user.avatarUrl || 'https://ui-avatars.com/api/?name=' + encodeURIComponent(user.firstName + '+' + user.lastName) + '&size=120&background=0d6efd&color=fff');

                toastr.success(T('Profile.ProfileUpdated'));
                self.isSavingProfile(false);

                // Update header name if exists
                if (window.updateHeaderUser) {
                    window.updateHeaderUser();
                }
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.error || T('Common.Error'));
                self.isSavingProfile(false);
            }
        });
    };

    // Change password
    self.changePassword = function() {
        if (!self.currentPassword()) {
            toastr.error(T('Profile.CurrentPasswordRequired'));
            return;
        }
        if (!self.newPassword()) {
            toastr.error(T('Profile.NewPasswordRequired'));
            return;
        }
        if (self.newPassword().length < 6) {
            toastr.error(T('Register.Error.PasswordMin'));
            return;
        }
        if (self.newPassword() !== self.confirmPassword()) {
            toastr.error(T('Profile.PasswordMismatch'));
            return;
        }

        self.isChangingPassword(true);

        $.ajax({
            url: apiBaseUrl + '/api/auth/change-password',
            method: 'PUT',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            contentType: 'application/json',
            data: JSON.stringify({
                currentPassword: self.currentPassword(),
                newPassword: self.newPassword()
            }),
            success: function() {
                toastr.success(T('Profile.PasswordChanged'));
                self.currentPassword('');
                self.newPassword('');
                self.confirmPassword('');
                self.isChangingPassword(false);
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || T('Common.Error');
                if (msg.indexOf('hatali') >= 0 || msg.indexOf('wrong') >= 0) {
                    msg = T('Profile.WrongPassword');
                }
                toastr.error(msg);
                self.isChangingPassword(false);
            }
        });
    };

    // Change language
    self.changeLanguage = function() {
        var lang = self.preferredLanguage();

        $.ajax({
            url: apiBaseUrl + '/api/auth/language',
            method: 'PUT',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            contentType: 'application/json',
            data: JSON.stringify({ language: lang }),
            success: function() {
                // Update local storage
                var storedUser = JSON.parse(localStorage.getItem('user') || '{}');
                storedUser.preferredLanguage = lang;
                localStorage.setItem('user', JSON.stringify(storedUser));

                // Reload page to apply language
                toastr.success('Dil degistirildi / Language changed');
                setTimeout(function() {
                    window.location.reload();
                }, 500);
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.error || 'Hata');
            }
        });
    };

    // Save notification preferences
    self.saveNotificationPrefs = function() {
        self.isSavingPrefs(true);
        var prefs = {
            email: self.prefEmail(),
            whatsapp: self.prefWhatsapp(),
            sms: self.prefSms()
        };
        $.ajax({
            url: apiBaseUrl + '/api/auth/notification-preferences',
            method: 'PUT',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            contentType: 'application/json',
            data: JSON.stringify(prefs),
            success: function() {
                toastr.success(T('NotificationPref.Saved') || 'Bildirim tercihleri kaydedildi');
                self.isSavingPrefs(false);
            },
            error: function(xhr) {
                toastr.error(xhr.responseJSON?.error || T('Common.Error'));
                self.isSavingPrefs(false);
            }
        });
    };

    // Send verification email
    self.sendVerificationEmail = function() {
        self.isSendingVerification(true);
        self.verificationSent(false);

        $.ajax({
            url: apiBaseUrl + '/api/auth/send-verification-email',
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            contentType: 'application/json',
            data: JSON.stringify({ baseUrl: window.location.origin }),
            success: function(response) {
                self.verificationSent(true);
                self.isSendingVerification(false);
                toastr.success(T('VerifyEmail.LinkSent'));

                // Development icin - konsolda linki goster
                if (response.debug && response.debug.verifyUrl) {
                    console.log('Verify URL:', response.debug.verifyUrl);
                }
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || T('Common.Error');
                toastr.error(msg);
                self.isSendingVerification(false);
            }
        });
    };

    // Avatar selected
    self.onAvatarSelected = function(data, event) {
        var file = event.target.files[0];
        if (!file) return;

        // Dosya tipi kontrolu
        if (!file.type.startsWith('image/')) {
            toastr.error(T('Avatar.OnlyImages'));
            return;
        }

        // Dosya boyutu kontrolu (2MB)
        if (file.size > 2 * 1024 * 1024) {
            toastr.error(T('Avatar.MaxSize'));
            return;
        }

        self.uploadAvatar(file);
    };

    // Upload avatar
    self.uploadAvatar = function(file) {
        self.isUploadingAvatar(true);

        var formData = new FormData();
        formData.append('file', file);

        $.ajax({
            url: apiBaseUrl + '/api/auth/avatar',
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                self.avatarUrl(response.avatarUrl);
                self.isUploadingAvatar(false);
                toastr.success(T('Avatar.UploadSuccess'));

                // Update local storage
                var storedUser = JSON.parse(localStorage.getItem('user') || '{}');
                storedUser.avatarUrl = response.avatarUrl;
                localStorage.setItem('user', JSON.stringify(storedUser));

                // Update header if exists
                if (window.updateHeaderUser) {
                    window.updateHeaderUser();
                }
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || T('Common.Error');
                toastr.error(msg);
                self.isUploadingAvatar(false);
            }
        });

        // Input'u temizle
        document.getElementById('avatarInput').value = '';
    };

    // Delete avatar
    self.deleteAvatar = function() {
        if (!confirm(T('Avatar.DeleteConfirm'))) return;

        self.isDeletingAvatar(true);

        $.ajax({
            url: apiBaseUrl + '/api/auth/avatar',
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
            success: function() {
                // Varsayilan avatar'a don
                self.avatarUrl('https://ui-avatars.com/api/?name=' + encodeURIComponent(self.firstName() + '+' + self.lastName()) + '&size=120&background=0d6efd&color=fff');
                self.isDeletingAvatar(false);
                toastr.success(T('Avatar.DeleteSuccess'));

                // Update local storage
                var storedUser = JSON.parse(localStorage.getItem('user') || '{}');
                storedUser.avatarUrl = null;
                localStorage.setItem('user', JSON.stringify(storedUser));

                // Update header if exists
                if (window.updateHeaderUser) {
                    window.updateHeaderUser();
                }
            },
            error: function(xhr) {
                var msg = xhr.responseJSON?.error || T('Common.Error');
                toastr.error(msg);
                self.isDeletingAvatar(false);
            }
        });
    };

    // Toggle profile visibility
    self.toggleProfileVisibility = function () {
        $.ajax({
            url: apiBaseUrl + '/api/travelers/visibility',
            method: 'PUT',
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
        }).done(function (data) {
            toastr.success(T(data.message) || 'Profil gorunurlugu guncellendi');
        }).fail(function (xhr) {
            toastr.error(xhr.responseJSON?.error || T('Common.Error'));
            // Revert
            self.isProfilePublic(!self.isProfilePublic());
        });
    };

    // Initialize
    self.loadUser();
}

ko.applyBindings(new ProfileViewModel(), document.getElementById('profileApp'));
