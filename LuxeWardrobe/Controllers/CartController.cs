using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.Services;

namespace LuxeWardrobe.Controllers
{
    public class CartController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();

        public ActionResult Index()
        {
            var model = CartPricingService.Build(_db, SessionCartService.GetItems(Session), SessionCartService.GetPromoCode(Session));
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Add(int productId, string size, int quantity = 1, string returnUrl = null)
        {
            quantity = Math.Max(1, Math.Min(quantity, 10));
            var product = _db.Products.Include(x => x.Sizes).FirstOrDefault(x => x.Id == productId && x.IsActive);
            var inventory = product == null ? null : product.Sizes.FirstOrDefault(x => x.Size == size);

            if (product == null || inventory == null)
            {
                TempData["Error"] = "Please choose a valid product and size.";
                return RedirectToAction("Index", "Home");
            }

            var current = SessionCartService.GetItems(Session).FirstOrDefault(x => x.ProductId == productId && x.Size == size);
            var requestedTotal = (current == null ? 0 : current.Quantity) + quantity;

            if (inventory.StockQuantity < requestedTotal)
            {
                TempData["Error"] = "Only " + inventory.StockQuantity + " item(s) are available in size " + size + ".";
                return SafeRedirect(returnUrl, productId, product.Slug);
            }

            SessionCartService.Add(Session, productId, size, quantity);

            TempData["CartToastName"] = product.Name;
            TempData["CartToastSize"] = size;
            TempData["CartToastPrice"] = product.Price.ToString("N0");

            return SafeRedirect(returnUrl, productId, product.Slug);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Update(int productId, string size, int quantity)
        {
            UpdateCartQuantity(productId, size, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult UpdateQuantity(int productId, string size, int quantity)
        {
            var success = UpdateCartQuantity(productId, size, quantity);
            var cart = CartPricingService.Build(_db, SessionCartService.GetItems(Session), SessionCartService.GetPromoCode(Session));
            var line = cart.Lines.FirstOrDefault(x => x.ProductId == productId && x.Size == size);

            return Json(new
            {
                success,
                quantity = line == null ? 0 : line.Quantity,
                lineTotal = line == null ? "0" : line.LineTotal.ToString("N0"),
                subtotal = cart.Subtotal.ToString("N0"),
                discount = cart.Discount.ToString("N0"),
                delivery = cart.DeliveryFee == 0 ? "Free" : "₹" + cart.DeliveryFee.ToString("N0"),
                total = cart.Total.ToString("N0"),
                cartCount = SessionCartService.Count(Session),
                removed = line == null
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Remove(int productId, string size)
        {
            SessionCartService.Remove(Session, productId, size);
            TempData["Success"] = "Item removed from your bag.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ApplyPromo(string promoCode)
        {
            SessionCartService.SetPromoCode(Session, promoCode);
            return RedirectToAction("Index");
        }

        private bool UpdateCartQuantity(int productId, string size, int quantity)
        {
            var inventory = _db.ProductSizeInventories.FirstOrDefault(x => x.ProductId == productId && x.Size == size);

            if (inventory == null)
            {
                SessionCartService.Remove(Session, productId, size);
                return false;
            }

            var safeQuantity = Math.Max(0, Math.Min(quantity, Math.Min(inventory.StockQuantity, 10)));
            SessionCartService.SetQuantity(Session, productId, size, safeQuantity);
            return safeQuantity == quantity;
        }

        private ActionResult SafeRedirect(string returnUrl, int productId, string slug)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Details", "Products", new { id = productId, slug = slug });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
