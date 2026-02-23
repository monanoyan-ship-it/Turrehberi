// Push Notification Management
var PushNotificationManager = (function () {
    var apiBaseUrl = window.apiBaseUrl || '';
    var isSubscribed = false;
    var swRegistration = null;

    function init() {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            console.log('Push notifications not supported');
            return;
        }

        navigator.serviceWorker.register('/sw.js')
            .then(function (reg) {
                swRegistration = reg;
                checkSubscription();
            })
            .catch(function (err) {
                console.error('Service Worker registration failed:', err);
            });
    }

    function checkSubscription() {
        if (!swRegistration) return;

        swRegistration.pushManager.getSubscription()
            .then(function (subscription) {
                isSubscribed = subscription !== null;
                updateUI();
            });
    }

    function updateUI() {
        var btn = document.getElementById('pushNotificationBtn');
        if (!btn) return;

        if (isSubscribed) {
            btn.innerHTML = '<i class="bi bi-bell-fill"></i> ' + T('Push.Subscribed');
            btn.classList.remove('btn-outline-primary');
            btn.classList.add('btn-primary');
        } else {
            btn.innerHTML = '<i class="bi bi-bell"></i> ' + T('Push.Subscribe');
            btn.classList.remove('btn-primary');
            btn.classList.add('btn-outline-primary');
        }
    }

    function subscribe() {
        if (!swRegistration) {
            toastr.error(T('Push.NotSupported'));
            return;
        }

        // VAPID public key placeholder - gercek implementasyonda server'dan alinacak
        var applicationServerKey = null;

        Notification.requestPermission().then(function (permission) {
            if (permission !== 'granted') {
                toastr.warning(T('Push.PermissionDenied'));
                return;
            }

            var options = {
                userVisibleOnly: true
            };

            if (applicationServerKey) {
                options.applicationServerKey = urlBase64ToUint8Array(applicationServerKey);
            }

            swRegistration.pushManager.subscribe(options)
                .then(function (subscription) {
                    var key = subscription.getKey('p256dh');
                    var auth = subscription.getKey('auth');

                    var data = {
                        endpoint: subscription.endpoint,
                        p256dh: key ? btoa(String.fromCharCode.apply(null, new Uint8Array(key))) : '',
                        auth: auth ? btoa(String.fromCharCode.apply(null, new Uint8Array(auth))) : ''
                    };

                    $.ajax({
                        url: apiBaseUrl + '/api/pushsubscriptions/subscribe',
                        method: 'POST',
                        contentType: 'application/json',
                        headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
                        data: JSON.stringify(data)
                    }).done(function () {
                        isSubscribed = true;
                        updateUI();
                        toastr.success(T('Push.SubscribeSuccess'));
                    }).fail(function () {
                        toastr.error(T('Push.SubscribeFailed'));
                    });
                })
                .catch(function (err) {
                    console.error('Push subscription failed:', err);
                    toastr.error(T('Push.SubscribeFailed'));
                });
        });
    }

    function unsubscribe() {
        if (!swRegistration) return;

        swRegistration.pushManager.getSubscription()
            .then(function (subscription) {
                if (!subscription) return;

                var endpoint = subscription.endpoint;

                subscription.unsubscribe().then(function () {
                    $.ajax({
                        url: apiBaseUrl + '/api/pushsubscriptions/unsubscribe',
                        method: 'DELETE',
                        contentType: 'application/json',
                        headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') },
                        data: JSON.stringify({ endpoint: endpoint })
                    }).done(function () {
                        isSubscribed = false;
                        updateUI();
                        toastr.success(T('Push.UnsubscribeSuccess'));
                    }).fail(function () {
                        toastr.error(T('Push.UnsubscribeFailed'));
                    });
                });
            });
    }

    function toggle() {
        if (isSubscribed) {
            unsubscribe();
        } else {
            subscribe();
        }
    }

    function urlBase64ToUint8Array(base64String) {
        var padding = '='.repeat((4 - base64String.length % 4) % 4);
        var base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        var rawData = window.atob(base64);
        var outputArray = new Uint8Array(rawData.length);
        for (var i = 0; i < rawData.length; i++) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }

    return {
        init: init,
        subscribe: subscribe,
        unsubscribe: unsubscribe,
        toggle: toggle,
        isSubscribed: function () { return isSubscribed; }
    };
})();

// Auto-init when user is logged in
$(document).ready(function () {
    if (localStorage.getItem('token')) {
        PushNotificationManager.init();
    }
});
