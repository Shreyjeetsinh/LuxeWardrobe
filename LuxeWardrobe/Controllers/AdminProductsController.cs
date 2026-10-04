using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.Filters;
using LuxeWardrobe.Models;
using LuxeWardrobe.Services;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Controllers
{
    [AdminAuthorize]
    public class AdminProductsController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();
        private static readonly string[] Sizes = { "S", "M", "L", "XL", "XXL" };
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".avif" };
        private const int MaxImagesPerProduct = 8;
        private const int MaxImageBytes = 5 * 1024 * 1024;

        public ActionResult Index()
        {
            var products = _db.Products.Include(x => x.Sizes).OrderByDescending(x => x.CreatedAtUtc).ToList();
            ViewBag.PrimaryImageIds = ProductImageService.GetPrimaryImageIds(_db, products.Select(x => x.Id));
            return View(products);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View("Edit", new ProductEditViewModel { IsActive = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(ProductEditViewModel model, IEnumerable<HttpPostedFileBase> images)
        {
            var uploads = GetUploads(images);
            ValidateUploads(uploads, 0, true);

            var slug = BuildUniqueSlug(model.Slug, model.Name, 0);
            if (!ModelState.IsValid)
            {
                model.Slug = slug;
                return View("Edit", model);
            }

            var product = new Product
            {
                CreatedAtUtc = DateTime.UtcNow,
                ImageFileName = "placeholder.svg"
            };

            Apply(model, product, slug);
            _db.Products.Add(product);
            _db.SaveChanges();

            SaveUploadedImages(product, uploads);
            _db.SaveChanges();

            TempData["Success"] = "Product created and published to the storefront.";
            return RedirectToAction("Edit", new { id = product.Id });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var product = _db.Products.Include(x => x.Sizes).FirstOrDefault(x => x.Id == id);
            if (product == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            return View(ToViewModel(product));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(ProductEditViewModel model, IEnumerable<HttpPostedFileBase> images)
        {
            var product = _db.Products.Include(x => x.Sizes).FirstOrDefault(x => x.Id == model.Id);
            if (product == null) return HttpNotFound();

            var uploads = GetUploads(images);
            var existingCount = _db.ProductImages.Count(x => x.ProductId == product.Id);
            ValidateUploads(uploads, existingCount, false);

            var slug = BuildUniqueSlug(model.Slug, model.Name, model.Id);
            if (!ModelState.IsValid)
            {
                model.Slug = slug;
                model.ExistingImages = ProductImageService.GetImages(_db, product.Id);
                return View(model);
            }

            Apply(model, product, slug);
            SaveUploadedImages(product, uploads);
            _db.SaveChanges();

            TempData["Success"] = "Product changes are live on the storefront.";
            return RedirectToAction("Edit", new { id = product.Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetPrimaryImage(int productId, int imageId)
        {
            var images = _db.ProductImages.Where(x => x.ProductId == productId).ToList();
            var selected = images.FirstOrDefault(x => x.Id == imageId);
            if (selected == null) return HttpNotFound();

            foreach (var image in images) image.IsPrimary = image.Id == imageId;

            var product = _db.Products.Find(productId);
            if (product != null) product.ImageFileName = selected.FileName;

            _db.SaveChanges();
            TempData["Success"] = "Primary product image updated.";
            return RedirectToAction("Edit", new { id = productId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteImage(int productId, int imageId)
        {
            var image = _db.ProductImages.FirstOrDefault(x => x.Id == imageId && x.ProductId == productId);
            if (image == null) return HttpNotFound();

            var wasPrimary = image.IsPrimary;
            _db.ProductImages.Remove(image);
            _db.SaveChanges();

            if (wasPrimary)
            {
                var next = _db.ProductImages
                    .Where(x => x.ProductId == productId)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Id)
                    .FirstOrDefault();

                if (next != null) next.IsPrimary = true;

                var product = _db.Products.Find(productId);
                if (product != null) product.ImageFileName = next == null ? "placeholder.svg" : next.FileName;
                _db.SaveChanges();
            }

            TempData["Success"] = "Product image removed.";
            return RedirectToAction("Edit", new { id = productId });
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

        private ProductEditViewModel ToViewModel(Product product)
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
                IsActive = product.IsActive,
                StockS = stock("S"),
                StockM = stock("M"),
                StockL = stock("L"),
                StockXL = stock("XL"),
                StockXXL = stock("XXL"),
                ExistingImages = ProductImageService.GetImages(_db, product.Id)
            };
        }

        private static void Apply(ProductEditViewModel model, Product product, string slug)
        {
            product.Name = (model.Name ?? "").Trim();
            product.Slug = slug;
            product.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
            product.Category = (model.Category ?? "").Trim();
            product.Color = (model.Color ?? "").Trim();
            product.Price = model.Price;
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

        private List<HttpPostedFileBase> GetUploads(IEnumerable<HttpPostedFileBase> images)
        {
            return (images ?? Enumerable.Empty<HttpPostedFileBase>())
                .Where(x => x != null && x.ContentLength > 0)
                .ToList();
        }

        private void ValidateUploads(List<HttpPostedFileBase> uploads, int existingCount, bool requireImage)
        {
            if (requireImage && uploads.Count == 0)
            {
                ModelState.AddModelError("", "Upload at least one product image.");
                return;
            }

            if (existingCount + uploads.Count > MaxImagesPerProduct)
            {
                ModelState.AddModelError("", "A product can have up to " + MaxImagesPerProduct + " images.");
            }

            foreach (var upload in uploads)
            {
                var extension = Path.GetExtension(upload.FileName ?? "").ToLowerInvariant();

                if (!AllowedImageExtensions.Contains(extension))
                    ModelState.AddModelError("", "Only JPG, PNG, WEBP and AVIF product images are supported.");

                if (upload.ContentLength > MaxImageBytes)
                    ModelState.AddModelError("", "Each product image must be 5 MB or smaller.");
            }
        }

        private void SaveUploadedImages(Product product, List<HttpPostedFileBase> uploads)
        {
            if (uploads.Count == 0) return;

            var currentCount = _db.ProductImages.Count(x => x.ProductId == product.Id);
            var hasPrimary = _db.ProductImages.Any(x => x.ProductId == product.Id && x.IsPrimary);
            var sortOrder = _db.ProductImages.Where(x => x.ProductId == product.Id)
                .Select(x => (int?)x.SortOrder)
                .Max() ?? 0;

            foreach (var upload in uploads)
            {
                using (var memory = new MemoryStream())
                {
                    upload.InputStream.CopyTo(memory);
                    var fileName = Path.GetFileName(upload.FileName);
                    var contentType = ResolveContentType(upload.ContentType, fileName);

                    var image = new ProductImage
                    {
                        ProductId = product.Id,
                        FileName = fileName,
                        ContentType = contentType,
                        ImageData = memory.ToArray(),
                        IsPrimary = !hasPrimary && currentCount == 0,
                        SortOrder = ++sortOrder,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    _db.ProductImages.Add(image);

                    if (image.IsPrimary)
                    {
                        hasPrimary = true;
                        product.ImageFileName = fileName;
                    }

                    currentCount++;
                }
            }
        }

        private string BuildUniqueSlug(string requestedSlug, string productName, int currentProductId)
        {
            var source = string.IsNullOrWhiteSpace(requestedSlug) ? productName : requestedSlug;
            var slug = Regex.Replace((source ?? "product").Trim().ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
            if (string.IsNullOrWhiteSpace(slug)) slug = "product";

            var candidate = slug;
            var suffix = 2;
            while (_db.Products.Any(x => x.Id != currentProductId && x.Slug == candidate))
            {
                candidate = slug + "-" + suffix;
                suffix++;
            }

            return candidate;
        }

        private static string ResolveContentType(string suppliedContentType, string fileName)
        {
            var extension = Path.GetExtension(fileName ?? "").ToLowerInvariant();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".webp": return "image/webp";
                case ".avif": return "image/avif";
                default: return string.IsNullOrWhiteSpace(suppliedContentType) ? "application/octet-stream" : suppliedContentType;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
