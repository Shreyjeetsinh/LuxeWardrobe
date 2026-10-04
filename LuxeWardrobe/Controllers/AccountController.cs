using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
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

            AttachLegacyOrders(customer);
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
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                PasswordHash = PasswordSecurity.HashPassword(model.Password, out salt),
                PasswordSalt = salt,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.CustomerAccounts.Add(customer);
            _db.SaveChanges();

            AttachLegacyOrders(customer);
            SignIn(customer);
            TempData["Success"] = "Your LuxeWardrobe account is ready.";
            return SafeRedirect(returnUrl);
        }

        [HttpGet]
        public ActionResult Profile()
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile", "Account") });

            AttachLegacyOrders(customer);

            var orders = _db.Orders
                .Include(x => x.Items)
                .Where(x => x.CustomerId == customer.Id || (x.CustomerId == null && x.Email == customer.Email))
                .OrderByDescending(x => x.OrderedAtUtc)
                .ToList();

            return View(new CustomerAccountViewModel
            {
                Profile = new CustomerProfileViewModel
                {
                    FullName = customer.FullName,
                    Email = customer.Email,
                    Phone = customer.Phone
                },
                Orders = orders
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(CustomerAccountViewModel pageModel)
        {
            var customer = GetCurrentCustomer();
            var model = pageModel == null ? null : pageModel.Profile;
            if (customer == null)
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile", "Account") });

            if (model == null)
            {
                ModelState.AddModelError("", "Profile details are required.");
                model = new CustomerProfileViewModel();
            }

            if (!ModelState.IsValid)
            {
                var orders = _db.Orders
                    .Include(x => x.Items)
                    .Where(x => x.CustomerId == customer.Id || (x.CustomerId == null && x.Email == customer.Email))
                    .OrderByDescending(x => x.OrderedAtUtc)
                    .ToList();

                return View("Profile", new CustomerAccountViewModel
                {
                    Profile = model,
                    Orders = orders
                });
            }

            var email = model.Email.Trim().ToLowerInvariant();
            if (_db.CustomerAccounts.Any(x => x.Id != customer.Id && x.Email == email))
            {
                ModelState.AddModelError("Email", "Another account already uses this email.");
                var orders = _db.Orders
                    .Include(x => x.Items)
                    .Where(x => x.CustomerId == customer.Id || (x.CustomerId == null && x.Email == customer.Email))
                    .OrderByDescending(x => x.OrderedAtUtc)
                    .ToList();

                return View("Profile", new CustomerAccountViewModel
                {
                    Profile = model,
                    Orders = orders
                });
            }

            var oldEmail = customer.Email;
            var legacyOrders = _db.Orders.Where(x => x.CustomerId == null && x.Email == oldEmail).ToList();
            foreach (var order in legacyOrders) order.CustomerId = customer.Id;

            customer.FullName = model.FullName.Trim();
            customer.Email = email;
            customer.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            _db.SaveChanges();

            SignIn(customer);
            TempData["Success"] = "Your profile has been updated.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public ActionResult OrderDetails(int id)
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
                return RedirectToAction("Login", new { returnUrl = Url.Action("OrderDetails", "Account", new { id }) });

            var order = _db.Orders
                .Include(x => x.Items)
                .FirstOrDefault(x => x.Id == id &&
                    (x.CustomerId == customer.Id || (x.CustomerId == null && x.Email == customer.Email)));

            if (order == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            if (!order.CustomerId.HasValue)
            {
                order.CustomerId = customer.Id;
                _db.SaveChanges();
            }

            return View(order);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Remove("CustomerId");
            Session.Remove("CustomerName");
            Session.Remove("CustomerEmail");
            Session.Remove("CustomerPhone");
            TempData["Success"] = "You have been signed out.";
            return RedirectToAction("Index", "Home");
        }

        private CustomerAccount GetCurrentCustomer()
        {
            var id = Session["CustomerId"] as int?;
            return id.HasValue ? _db.CustomerAccounts.FirstOrDefault(x => x.Id == id.Value) : null;
        }

        private void AttachLegacyOrders(CustomerAccount customer)
        {
            var legacyOrders = _db.Orders.Where(x => x.CustomerId == null && x.Email == customer.Email).ToList();
            if (!legacyOrders.Any()) return;

            foreach (var order in legacyOrders) order.CustomerId = customer.Id;
            _db.SaveChanges();
        }

        private void SignIn(CustomerAccount customer)
        {
            Session["CustomerId"] = customer.Id;
            Session["CustomerName"] = customer.FullName;
            Session["CustomerEmail"] = customer.Email;
            Session["CustomerPhone"] = customer.Phone;
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
