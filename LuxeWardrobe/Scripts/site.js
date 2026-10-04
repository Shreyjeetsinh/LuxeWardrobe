document.addEventListener("DOMContentLoaded", function () {
    window.setTimeout(function () {
        document.querySelectorAll(".auto-dismiss").forEach(function (el) {
            el.style.transition = "opacity .25s ease";
            el.style.opacity = "0";
            window.setTimeout(function () { el.remove(); }, 260);
        });
    }, 2800);
});
