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

    window.setTimeout(function () {
        document.querySelectorAll(".luxe-cart-toast").forEach(function (el) {
            if (window.bootstrap && bootstrap.Toast) {
                bootstrap.Toast.getOrCreateInstance(el).hide();
            } else {
                el.remove();
            }
        });
    }, 4000);

    document.querySelectorAll("[data-gallery-image]").forEach(function (button) {
        button.addEventListener("click", function () {
            var mainImage = document.getElementById("mainProductImage");
            if (!mainImage) return;

            mainImage.src = button.getAttribute("data-gallery-image");
            mainImage.removeAttribute("data-fallback-applied");

            document.querySelectorAll("[data-gallery-image]").forEach(function (item) {
                item.classList.remove("active");
            });
            button.classList.add("active");
        });
    });

    document.addEventListener("click", function (event) {
        var plus = event.target.closest("[data-qty-plus]");
        var minus = event.target.closest("[data-qty-minus]");
        if (!plus && !minus) return;

        var button = plus || minus;
        var form = button.closest("[data-qty-form]");
        if (!form || form.getAttribute("data-busy") === "true") return;

        var input = form.querySelector('input[name="quantity"]');
        if (!input) return;

        var quantity = parseInt(input.value || "1", 10);
        var max = parseInt(input.getAttribute("max") || "10", 10);

        if (plus) quantity = Math.min(max, quantity + 1);
        if (minus) quantity = Math.max(1, quantity - 1);

        if (quantity === parseInt(input.value || "1", 10)) return;

        input.value = quantity;
        updateCartQuantity(form);
    });
});

function updateCartQuantity(form) {
    form.setAttribute("data-busy", "true");

    var data = new FormData(form);

    fetch(form.action, {
        method: "POST",
        body: data,
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        }
    })
    .then(function (response) {
        if (!response.ok) throw new Error("Unable to update quantity.");
        return response.json();
    })
    .then(function (result) {
        var line = form.closest("[data-cart-line]");

        if (line && result.removed) {
            line.remove();
        } else if (line) {
            var quantityInput = line.querySelector('input[name="quantity"]');
            var lineTotal = line.querySelector("[data-line-total]");

            if (quantityInput) quantityInput.value = result.quantity;
            if (lineTotal) lineTotal.innerHTML = "&#8377;" + result.lineTotal;
        }

        updateText("[data-cart-subtotal]", "₹" + result.subtotal);
        updateText("[data-cart-discount]", "-₹" + result.discount);
        updateText("[data-cart-delivery]", result.delivery);
        updateText("[data-cart-total]", "₹" + result.total);
        updateText("[data-cart-count]", result.cartCount);
        updateText("[data-cart-total-number]", result.total);
    })
    .catch(function () {
        window.location.reload();
    })
    .finally(function () {
        form.removeAttribute("data-busy");
    });
}

function updateText(selector, value) {
    document.querySelectorAll(selector).forEach(function (element) {
        element.textContent = value;
    });
}

window.luxeImageFallback = function (img) {
    var imageFile = (img.getAttribute("data-image-file") || "").toLowerCase();
    var fallbacks = {
        "t-1.avif": "https://images.unsplash.com/photo-1598033129183-c4f50c736f10?auto=format&fit=crop&w=1000&q=85",
        "t-2.avif": "https://images.unsplash.com/photo-1603252109303-2751441dd157?auto=format&fit=crop&w=1000&q=85",
        "t-3.avif": "https://images.unsplash.com/photo-1620012253295-c15cc3e65df4?auto=format&fit=crop&w=1000&q=85"
    };

    if (img.getAttribute("data-fallback-applied") === "true") {
        img.onerror = null;
        img.src = img.getAttribute("data-placeholder") || "";
        return;
    }

    img.setAttribute("data-fallback-applied", "true");
    img.src = fallbacks[imageFile] || img.getAttribute("data-placeholder") || "";
};
