document.addEventListener("DOMContentLoaded", function () {
    window.setTimeout(function () {
        document.querySelectorAll(".auto-dismiss").forEach(function (el) {
            if (window.bootstrap && bootstrap.Alert) {
                bootstrap.Alert.getOrCreateInstance(el).close();
            } else {
                el.remove();
            }
        });
    }, 5000);
});

window.luxeImageFallback = function (img) {
    var imageFile = (img.getAttribute("data-image-file") || "").toLowerCase();
    var fallbacks = {
        "t-1.avif": "https://unsplash.com/photos/iLrVy2RvFgE/download?force=true&w=1200",
        "t-2.avif": "https://unsplash.com/photos/UsWlwoB3lg4/download?force=true&w=1200",
        "t-3.avif": "https://unsplash.com/photos/XYeKylILW5I/download?force=true&w=1200"
    };

    if (img.getAttribute("data-fallback-applied") === "true") {
        img.onerror = null;
        img.src = img.getAttribute("data-placeholder") || "";
        return;
    }

    img.setAttribute("data-fallback-applied", "true");
    img.src = fallbacks[imageFile] || img.getAttribute("data-placeholder") || "";
};