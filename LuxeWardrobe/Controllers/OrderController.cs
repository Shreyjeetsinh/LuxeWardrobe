using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Controllers
{
    public class OrderController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();

        [HttpGet]
        public ActionResult Track()
        {
            return View(new TrackOrderViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Track(TrackOrderViewModel model)
        {
            model.SearchPerformed = true;
            if (!ModelState.IsValid) return View(model);

            var orderNumber = model.OrderNumber.Trim();
            var email = model.Email.Trim();
            model.Order = _db.Orders.Include(x => x.Items)
                .FirstOrDefault(x => x.OrderNumber == orderNumber && x.Email == email);
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
