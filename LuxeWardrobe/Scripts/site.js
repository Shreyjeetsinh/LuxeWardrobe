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
            document.querySelectorAll("[data-gallery-image]").forEach(function (item) {
                item.classList.remove("active");
            });
            button.classList.add("active");
        });
    });

    document.querySelectorAll("[data-add-to-cart]").forEach(function (form) {
        form.addEventListener("submit", function (event) {
            event.preventDefault();
            addToCart(form);
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

function addToCart(form) {
    if (form.getAttribute("data-busy") === "true") return;

    var selectedSize = form.querySelector('input[name="size"]:checked');
    var status = form.querySelector("[data-add-cart-status]");
    var submitButton = form.querySelector('button[type="submit"]');

    if (!selectedSize) {
        setAddCartStatus(status, "Please select a size.", true);
        return;
    }

    form.setAttribute("data-busy", "true");
    if (submitButton) {
        submitButton.disabled = true;
        submitButton.setAttribute("data-original-text", submitButton.innerHTML);
        submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Adding...';
    }

    fetch(form.action, {
        method: "POST",
        body: new FormData(form),
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        }
    })
    .then(function (response) {
        if (!response.ok) throw new Error("Could not add this item.");
        return response.json();
    })
    .then(function (result) {
        if (!result.success) {
            setAddCartStatus(status, result.message || "Could not add this item.", true);
            return;
        }

        updateText("[data-cart-count]", result.cartCount);
        setAddCartStatus(status, "Added to bag. " + result.remainingStock + " left after your selection.", false);
        updateSelectedSizeStock(selectedSize, result.remainingStock);
        showCartToast(result);
    })
    .catch(function () {
        setAddCartStatus(status, "Something went wrong. Please try again.", true);
    })
    .finally(function () {
        form.removeAttribute("data-busy");
        if (submitButton) {
            submitButton.disabled = false;
            submitButton.innerHTML = submitButton.getAttribute("data-original-text") || "Add to bag";
        }
    });
}

function updateSelectedSizeStock(selectedInput, remainingStock) {
    if (!selectedInput) return;
    var option = selectedInput.closest("[data-size-option]");
    if (!option) return;

    var stockText = option.querySelector("[data-stock-count]");
    if (stockText) {
        stockText.textContent = remainingStock <= 0 ? "In bag / no more left" : remainingStock + " left after bag";
    }
}

function setAddCartStatus(element, message, isError) {
    if (!element) return;
    element.classList.remove("text-danger", "text-success");
    element.classList.add(isError ? "text-danger" : "text-success");
    element.textContent = message;
}

function showCartToast(result) {
    var host = document.getElementById("cartToastHost");
    if (!host) return;

    var toast = document.createElement("div");
    toast.className = "toast show border-0 shadow-lg luxe-cart-toast";
    toast.setAttribute("role", "status");
    toast.innerHTML =
        '<div class="toast-header">' +
            '<i class="bi bi-check-circle-fill text-success me-2"></i>' +
            '<strong class="me-auto">Added to your bag</strong>' +
            '<button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>' +
        '</div>' +
        '<div class="toast-body">' +
            '<div class="fw-semibold"></div>' +
            '<div class="small text-secondary cart-toast-meta"></div>' +
            '<a class="small fw-semibold d-inline-block mt-2" href="/Cart">View bag</a>' +
        '</div>';

    toast.querySelector(".fw-semibold").textContent = result.productName;
    toast.querySelector(".cart-toast-meta").textContent = "Size " + result.size + " · ₹" + result.price;
    host.appendChild(toast);

    if (window.bootstrap && bootstrap.Toast) {
        var instance = bootstrap.Toast.getOrCreateInstance(toast, { delay: 3500 });
        toast.addEventListener("hidden.bs.toast", function () { toast.remove(); });
        instance.show();
    } else {
        window.setTimeout(function () { toast.remove(); }, 3500);
    }
}

function updateCartQuantity(form) {
    form.setAttribute("data-busy", "true");

    fetch(form.action, {
        method: "POST",
        body: new FormData(form),
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
            if (lineTotal) lineTotal.textContent = "₹" + result.lineTotal;
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
    img.onerror = null;
    img.src = img.getAttribute("data-placeholder") || "/Content/images/products/placeholder.svg";
};
