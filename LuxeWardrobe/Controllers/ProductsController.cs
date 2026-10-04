using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.Services;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Controllers
{
    public class ProductsController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();

        public ActionResult Details(int id)
        {
            var product = _db.Products.Include(x => x.Sizes).FirstOrDefault(x => x.Id == id && x.IsActive);
            if (product == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            return View(new ProductDetailsViewModel
            {
                Product = product,
                GalleryImages = ProductImageService.GetImages(_db, product.Id)
            });
        }

        [OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult Image(int id)
        {
            var image = _db.ProductImages.FirstOrDefault(x => x.Id == id && x.Product.IsActive);
            if (image == null) return HttpNotFound();

            return File(image.ImageData, image.ContentType);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
