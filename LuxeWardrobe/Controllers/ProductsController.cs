using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using LuxeWardrobe.Data;
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
            return View(new ProductDetailsViewModel { Product = product });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
