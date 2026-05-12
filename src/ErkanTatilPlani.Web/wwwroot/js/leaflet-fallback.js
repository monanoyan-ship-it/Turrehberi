(function() {
    if (window.L) {
        return;
    }

    function ensureFallbackMap(target) {
        var mapElement = typeof target === 'string' ? document.getElementById(target) : target;
        if (!mapElement) {
            return null;
        }

        mapElement.classList.add('leaflet-fallback-map');
        if (mapElement.dataset.leafletFallbackReady === 'true') {
            return mapElement;
        }

        mapElement.dataset.leafletFallbackReady = 'true';
        mapElement.innerHTML =
            '<div class="leaflet-fallback-stage">' +
            '<img src="/images/placeholders/map-placeholder.svg" alt="" loading="lazy">' +
            '</div>';

        return mapElement;
    }

    function createMap(target) {
        ensureFallbackMap(target);

        return {
            setView: function() {
                ensureFallbackMap(target);
                return this;
            },
            fitBounds: function() {
                return this;
            },
            removeLayer: function() {
                return this;
            },
            invalidateSize: function() {
                return this;
            }
        };
    }

    function createLayer() {
        return {
            addTo: function() {
                return this;
            }
        };
    }

    function createMarker() {
        return {
            addTo: function() {
                return this;
            },
            bindPopup: function() {
                return this;
            },
            openPopup: function() {
                return this;
            }
        };
    }

    window.L = {
        map: createMap,
        tileLayer: createLayer,
        marker: createMarker
    };
})();
