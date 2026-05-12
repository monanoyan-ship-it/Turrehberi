(function() {
    var placeholders = {
        avatar: '/images/placeholders/avatar-placeholder.svg',
        brand: '/images/placeholders/brand-placeholder.svg',
        image: '/images/placeholders/image-placeholder.svg'
    };

    function addSource(sources, seen, value) {
        var normalized = (value || '').trim();
        if (!normalized || seen[normalized]) {
            return;
        }

        seen[normalized] = true;
        sources.push(normalized);
    }

    function addSrcsetSources(sources, seen, value) {
        if (!value) {
            return;
        }

        value.split(',').forEach(function(part) {
            var candidate = (part || '').trim().split(/\s+/)[0];
            addSource(sources, seen, candidate);
        });
    }

    function getImageSources(img) {
        var sources = [];
        var seen = {};

        if (!img) {
            return sources;
        }

        addSource(sources, seen, img.currentSrc);
        addSource(sources, seen, img.getAttribute('src'));
        addSource(sources, seen, img.getAttribute('data-src'));
        addSource(sources, seen, img.getAttribute('data-original'));
        addSrcsetSources(sources, seen, img.getAttribute('srcset'));
        addSrcsetSources(sources, seen, img.getAttribute('data-srcset'));

        return sources;
    }

    function isRemoteAsset(url) {
        if (!url || url.indexOf('data:') === 0 || url.indexOf('blob:') === 0) {
            return false;
        }

        try {
            return new URL(url, window.location.href).origin !== window.location.origin;
        } catch (error) {
            return false;
        }
    }

    function getRemoteSource(img, preferredSource) {
        if (isRemoteAsset(preferredSource)) {
            return preferredSource;
        }

        var sources = getImageSources(img);
        for (var i = 0; i < sources.length; i++) {
            if (isRemoteAsset(sources[i])) {
                return sources[i];
            }
        }

        return '';
    }

    function resolvePlaceholder(img, preferredSource) {
        var src = (preferredSource || getRemoteSource(img) || '').toLowerCase();
        var alt = (img.getAttribute('alt') || '').toLowerCase();
        var className = (img.className || '').toLowerCase();
        var width = img.clientWidth || parseInt(img.getAttribute('width') || '0', 10) || 0;
        var height = img.clientHeight || parseInt(img.getAttribute('height') || '0', 10) || 0;
        var maxEdge = Math.max(width, height);

        if (src.indexOf('ui-avatars.com') >= 0 || alt.indexOf('avatar') >= 0 || (className.indexOf('rounded-circle') >= 0 && maxEdge > 0 && maxEdge <= 180)) {
            return placeholders.avatar;
        }

        if (alt.indexOf('logo') >= 0 || src.indexOf('logo') >= 0) {
            return placeholders.brand;
        }

        return placeholders.image;
    }

    function applyFallback(img, preferredSource) {
        if (!img || img.dataset.assetFallbackApplied === 'true') {
            return false;
        }

        var remoteSource = getRemoteSource(img, preferredSource);
        if (!remoteSource) {
            return false;
        }

        img.dataset.assetFallbackApplied = 'true';
        img.removeAttribute('srcset');
        img.removeAttribute('data-srcset');
        img.removeAttribute('data-src');
        img.removeAttribute('data-original');
        img.src = resolvePlaceholder(img, remoteSource);
        img.classList.add('asset-fallback-applied', 'img-error-fallback');
        return true;
    }

    function repairBrokenImages() {
        Array.prototype.forEach.call(document.images, function(img) {
            if (img.dataset.assetFallbackApplied === 'true') {
                return;
            }

            var remoteSource = getRemoteSource(img);
            if (!remoteSource) {
                return;
            }

            if (img.complete && img.naturalWidth === 0) {
                applyFallback(img, remoteSource);
            }
        });
    }

    function patchLazyLoad() {
        if (!window.LazyLoad || typeof window.LazyLoad.loadImage !== 'function' || window.LazyLoad.__assetFallbackPatched) {
            return;
        }

        var originalLoadImage = window.LazyLoad.loadImage;
        window.LazyLoad.loadImage = function(img) {
            var remoteSource = img ? (img.getAttribute('data-src') || img.getAttribute('data-original') || '') : '';
            if (!isRemoteAsset(remoteSource)) {
                return originalLoadImage.call(this, img);
            }

            img.classList.add('lazy-img');

            var preloadImage = new Image();
            preloadImage.onload = function() {
                img.src = remoteSource;
                img.classList.add('loaded', 'fade-in-img');
                img.removeAttribute('data-src');
                img.removeAttribute('data-original');
            };
            preloadImage.onerror = function() {
                applyFallback(img, remoteSource);
            };
            preloadImage.src = remoteSource;
        };

        window.LazyLoad.__assetFallbackPatched = true;
    }

    window.AssetFallbacks = {
        applyFallback: applyFallback,
        repairBrokenImages: repairBrokenImages,
        getRemoteSource: function(img) {
            return getRemoteSource(img);
        }
    };

    document.addEventListener('error', function(event) {
        if (event.target && event.target.tagName === 'IMG') {
            applyFallback(event.target);
        }
    }, true);

    patchLazyLoad();

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', repairBrokenImages);
    } else {
        repairBrokenImages();
    }

    window.addEventListener('load', function() {
        patchLazyLoad();
        repairBrokenImages();
        window.setTimeout(repairBrokenImages, 500);
        window.setTimeout(repairBrokenImages, 1500);
    });
})();
