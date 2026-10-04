using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.Filters;
using LuxeWardrobe.Services;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Controllers
{
    public class AdminController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Session["AdminUserId"] != null) return RedirectToAction("Dashboard");
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdminLoginViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(AdminLoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var user = _db.AdminUsers.FirstOrDefault(x => x.Username == model.Username);
            if (user == null || !PasswordSecurity.Verify(model.Password, user.PasswordSalt, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            Session["AdminUserId"] = user.Id;
            Session["AdminUsername"] = user.Username;

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Dashboard");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Remove("AdminUserId");
            Session.Remove("AdminUsername");
            return RedirectToAction("Login");
        }

        [AdminAuthorize]
        public ActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                ActiveProducts = _db.Products.Count(x => x.IsActive),
                TotalOrders = _db.Orders.Count(),
                PendingOrders = _db.Orders.Count(x => x.Status == "Placed" || x.Status == "Processing"),
                Revenue = _db.Orders.Any() ? _db.Orders.Sum(x => x.Total) : 0m
            };
            return View(model);
        }

        [AdminAuthorize]
        public ActionResult Orders()
        {
            return View(_db.Orders.Include(x => x.Items).OrderByDescending(x => x.OrderedAtUtc).ToList());
        }

        [HttpPost, ValidateAntiForgeryToken, AdminAuthorize]
        public ActionResult UpdateStatus(int id, string status)
        {
            var allowed = new[] { "Placed", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!allowed.Contains(status)) return new HttpStatusCodeResult(400);

            var order = _db.Orders.Find(id);
            if (order == null) return HttpNotFound();
            order.Status = status;
            _db.SaveChanges();
            TempData["Success"] = "Order status updated.";
            return RedirectToAction("Orders");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
