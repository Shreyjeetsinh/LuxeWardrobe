using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.Filters;
using LuxeWardrobe.Models;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Controllers
{
    [AdminAuthorize]
    public class AdminProductsController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();
        private static readonly string[] Sizes = { "S", "M", "L", "XL", "XXL" };

        public ActionResult Index()
        {
            return View(_db.Products.Include(x => x.Sizes).OrderBy(x => x.Name).ToList());
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View("Edit", new ProductEditViewModel { IsActive = true, ImageFileName = "placeholder.svg" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(ProductEditViewModel model)
        {
            if (!ModelState.IsValid) return View("Edit", model);
            if (_db.Products.Any(x => x.Slug == model.Slug))
            {
                ModelState.AddModelError("Slug", "That slug is already in use.");
                return View("Edit", model);
            }

            var product = new Product { CreatedAtUtc = DateTime.UtcNow };
            Apply(model, product);
            _db.Products.Add(product);
            _db.SaveChanges();
            TempData["Success"] = "Product created.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var product = _db.Products.Include(x => x.Sizes).FirstOrDefault(x => x.Id == id);
            if (product == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            return View(ToViewModel(product));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(ProductEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            if (_db.Products.Any(x => x.Slug == model.Slug && x.Id != model.Id))
            {
                ModelState.AddModelError("Slug", "That slug is already in use.");
                return View(model);
            }

            var product = _db.Products.Include(x => x.Sizes).FirstOrDefault(x => x.Id == model.Id);
            if (product == null) return HttpNotFound();
            Apply(model, product);
            _db.SaveChanges();
            TempData["Success"] = "Product updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null) return HttpNotFound();
            product.IsActive = false;
            _db.SaveChanges();
            TempData["Success"] = "Product hidden from the storefront.";
            return RedirectToAction("Index");
        }

        private static ProductEditViewModel ToViewModel(Product product)
        {
            Func<string, int> stock = size => product.Sizes.Where(x => x.Size == size).Select(x => x.StockQuantity).FirstOrDefault();
            return new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Category = product.Category,
                Color = product.Color,
                Price = product.Price,
                ImageFileName = product.ImageFileName,
                IsActive = product.IsActive,
                StockS = stock("S"), StockM = stock("M"), StockL = stock("L"), StockXL = stock("XL"), StockXXL = stock("XXL")
            };
        }

        private static void Apply(ProductEditViewModel model, Product product)
        {
            product.Name = model.Name.Trim();
            product.Slug = model.Slug.Trim().ToLowerInvariant().Replace(" ", "-");
            product.Description = model.Description;
            product.Category = model.Category.Trim();
            product.Color = model.Color.Trim();
            product.Price = model.Price;
            product.ImageFileName = model.ImageFileName.Trim();
            product.IsActive = model.IsActive;

            var stocks = new[] { model.StockS, model.StockM, model.StockL, model.StockXL, model.StockXXL };
            for (var i = 0; i < Sizes.Length; i++)
            {
                var sizeName = Sizes[i];
                var inventory = product.Sizes.FirstOrDefault(x => x.Size == sizeName);
                if (inventory == null)
                {
                    product.Sizes.Add(new ProductSizeInventory { Size = sizeName, StockQuantity = stocks[i] });
                }
                else
                {
                    inventory.StockQuantity = stocks[i];
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
