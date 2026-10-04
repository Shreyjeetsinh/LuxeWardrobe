using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.Services;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Controllers
{
    public class HomeController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();

        public ActionResult Index(string q, string category, string size, string sort)
        {
            var query = _db.Products.Include(x => x.Sizes).Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x => x.Name.Contains(q) || x.Description.Contains(q) || x.Color.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(size))
            {
                query = query.Where(x => x.Sizes.Any(s => s.Size == size && s.StockQuantity > 0));
            }

            switch (sort)
            {
                case "price-low": query = query.OrderBy(x => x.Price); break;
                case "price-high": query = query.OrderByDescending(x => x.Price); break;
                case "name": query = query.OrderBy(x => x.Name); break;
                default: query = query.OrderByDescending(x => x.CreatedAtUtc); break;
            }

            var products = query.ToList();
            var productIds = products.Select(x => x.Id).ToList();

            var model = new CatalogViewModel
            {
                Products = products,
                PrimaryImageIds = ProductImageService.GetPrimaryImageIds(_db, productIds),
                Categories = _db.Products.Where(x => x.IsActive).Select(x => x.Category).Distinct().OrderBy(x => x).ToList(),
                Query = q,
                Category = category,
                Size = size,
                Sort = sort
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
