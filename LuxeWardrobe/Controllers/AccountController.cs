using System;
using System.Linq;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.Models;
using LuxeWardrobe.Services;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Controllers
{
    public class AccountController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Session["CustomerId"] != null)
                return SafeRedirect(returnUrl);

            ViewBag.ReturnUrl = returnUrl;
            return View(new CustomerLoginViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(CustomerLoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            var customer = _db.CustomerAccounts.FirstOrDefault(x => x.Email == email);

            if (customer == null || !PasswordSecurity.Verify(model.Password, customer.PasswordSalt, customer.PasswordHash))
            {
                ModelState.AddModelError("", "Incorrect email or password.");
                return View(model);
            }

            SignIn(customer);
            TempData["Success"] = "Welcome back, " + customer.FullName + ".";
            return SafeRedirect(returnUrl);
        }

        [HttpGet]
        public ActionResult Register(string returnUrl)
        {
            if (Session["CustomerId"] != null)
                return SafeRedirect(returnUrl);

            ViewBag.ReturnUrl = returnUrl;
            return View(new CustomerRegisterViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Register(CustomerRegisterViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLowerInvariant();
            if (_db.CustomerAccounts.Any(x => x.Email == email))
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            string salt;
            var customer = new CustomerAccount
            {
                FullName = model.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordSecurity.HashPassword(model.Password, out salt),
                PasswordSalt = salt,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.CustomerAccounts.Add(customer);
            _db.SaveChanges();

            SignIn(customer);
            TempData["Success"] = "Your LuxeWardrobe account is ready.";
            return SafeRedirect(returnUrl);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Remove("CustomerId");
            Session.Remove("CustomerName");
            Session.Remove("CustomerEmail");
            TempData["Success"] = "You have been signed out.";
            return RedirectToAction("Index", "Home");
        }

        private void SignIn(CustomerAccount customer)
        {
            Session["CustomerId"] = customer.Id;
            Session["CustomerName"] = customer.FullName;
            Session["CustomerEmail"] = customer.Email;
        }

        private ActionResult SafeRedirect(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
