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
            TempData["Success"] = product.Name + " added to your bag.";
            return SafeRedirect(returnUrl, productId, product.Slug);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Update(int productId, string size, int quantity)
        {
            var inventory = _db.ProductSizeInventories.FirstOrDefault(x => x.ProductId == productId && x.Size == size);
            if (inventory == null)
            {
                SessionCartService.Remove(Session, productId, size);
            }
            else
            {
                var safeQuantity = Math.Max(0, Math.Min(quantity, Math.Min(inventory.StockQuantity, 10)));
                SessionCartService.SetQuantity(Session, productId, size, safeQuantity);
            }
            return RedirectToAction("Index");
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
