// ============================================================
// TypeDefinitions - Backend TypeDefinitions.cs'den API ile yuklenir
// T() ve apiBaseUrl _Layout.cshtml'de tanimli
// ============================================================

var TypeDefinitions = {
    _data: {},
    _ready: false,
    _callbacks: [],

    load: function() {
        var self = this;
        return $.ajax({
            url: apiBaseUrl + '/api/type-definitions',
            method: 'GET'
        }).done(function(data) {
            self._data = data || {};
        }).always(function() {
            self._ready = true;
            self._callbacks.forEach(function(cb) { cb(); });
            self._callbacks = [];
        });
    },

    onReady: function(callback) {
        if (this._ready) { callback(); }
        else { this._callbacks.push(callback); }
    },

    _find: function(group, systemName) {
        var items = this._data[group] || [];
        return items.find(function(x) { return x.systemName === systemName; });
    },

    _findById: function(group, id) {
        var items = this._data[group] || [];
        return items.find(function(x) { return x.id === id; });
    },

    localize: function(group, systemName) {
        var item = this._find(group, systemName);
        return item ? T(item.nameResourceKey) : (systemName || '');
    },

    localizeById: function(group, id) {
        var item = this._findById(group, id);
        return item ? T(item.nameResourceKey) : '';
    },

    getIcon: function(group, systemName) {
        var item = this._find(group, systemName);
        return item ? item.icon : '';
    },

    getCssClass: function(group, systemName) {
        var item = this._find(group, systemName);
        return item ? item.cssClass : '';
    },

    getItems: function(group) {
        return this._data[group] || [];
    },

    DurationUnits: {
        localize: function(systemName) { return TypeDefinitions.localize('durationUnits', systemName); },
        getById: function(id) { return TypeDefinitions._findById('durationUnits', id); },
        getBySystemName: function(name) { return TypeDefinitions._find('durationUnits', name); }
    },

    ReservationStatuses: {
        localize: function(systemName) { return TypeDefinitions.localize('reservationStatuses', systemName); },
        getCssClass: function(systemName) { return TypeDefinitions.getCssClass('reservationStatuses', systemName); }
    }
};

// Sayfa yuklendiginde otomatik yukle
TypeDefinitions.load();
