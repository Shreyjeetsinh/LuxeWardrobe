using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using LuxeWardrobe.Data;
using LuxeWardrobe.Models;
using LuxeWardrobe.Services;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly LuxeWardrobeDbContext _db = new LuxeWardrobeDbContext();

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["CustomerId"] == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });

            var cart = CartPricingService.Build(_db, SessionCartService.GetItems(Session), SessionCartService.GetPromoCode(Session));
            if (cart.IsEmpty) return RedirectToAction("Index", "Cart");

            var customerId = Session["CustomerId"] as int?;
            var customer = customerId.HasValue ? _db.CustomerAccounts.FirstOrDefault(x => x.Id == customerId.Value) : null;

            return View(new CheckoutViewModel
            {
                Cart = cart,
                CustomerName = customer == null ? Session["CustomerName"] as string : customer.FullName,
                Email = customer == null ? Session["CustomerEmail"] as string : customer.Email,
                Phone = customer == null ? Session["CustomerPhone"] as string : customer.Phone
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(CheckoutViewModel model)
        {
            if (Session["CustomerId"] == null)
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });

            var customerId = Session["CustomerId"] as int?;
            var customer = customerId.HasValue ? _db.CustomerAccounts.FirstOrDefault(x => x.Id == customerId.Value) : null;

            model.CustomerName = customer == null ? Session["CustomerName"] as string : customer.FullName;
            model.Email = customer == null ? Session["CustomerEmail"] as string : customer.Email;

            var sessionItems = SessionCartService.GetItems(Session).ToList();
            var cart = CartPricingService.Build(_db, sessionItems, SessionCartService.GetPromoCode(Session));
            model.Cart = cart;

            if (cart.IsEmpty)
            {
                ModelState.AddModelError("", "Your bag is empty.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var line in cart.Lines)
                    {
                        var inventory = _db.ProductSizeInventories.SingleOrDefault(x => x.ProductId == line.ProductId && x.Size == line.Size);
                        if (inventory == null || inventory.StockQuantity < line.Quantity)
                        {
                            ModelState.AddModelError("", line.ProductName + " in size " + line.Size + " no longer has enough stock. Please update your cart.");
                            transaction.Rollback();
                            return View(model);
                        }
                    }

                    var order = new Order
                    {
                        CustomerId = customerId,
                        OrderNumber = CreateOrderNumber(),
                        CustomerName = model.CustomerName.Trim(),
                        Email = model.Email.Trim(),
                        Phone = model.Phone.Trim(),
                        AddressLine1 = model.AddressLine1.Trim(),
                        AddressLine2 = string.IsNullOrWhiteSpace(model.AddressLine2) ? null : model.AddressLine2.Trim(),
                        City = model.City.Trim(),
                        State = model.State.Trim(),
                        PostalCode = model.PostalCode.Trim(),
                        Status = "Placed",
                        PromoCode = string.IsNullOrWhiteSpace(cart.PromoCode) ? null : cart.PromoCode,
                        Subtotal = cart.Subtotal,
                        Discount = cart.Discount,
                        DeliveryFee = cart.DeliveryFee,
                        Total = cart.Total,
                        OrderedAtUtc = DateTime.UtcNow
                    };

                    foreach (var line in cart.Lines)
                    {
                        order.Items.Add(new OrderItem
                        {
                            ProductId = line.ProductId,
                            ProductName = line.ProductName,
                            Size = line.Size,
                            Color = line.Color,
                            UnitPrice = line.UnitPrice,
                            Quantity = line.Quantity
                        });

                        var inventory = _db.ProductSizeInventories.Single(x => x.ProductId == line.ProductId && x.Size == line.Size);
                        inventory.StockQuantity -= line.Quantity;
                    }

                    _db.Orders.Add(order);

                    if (customer != null && !string.Equals(customer.Phone, model.Phone, StringComparison.Ordinal))
                    {
                        customer.Phone = model.Phone.Trim();
                        Session["CustomerPhone"] = customer.Phone;
                    }

                    _db.SaveChanges();
                    transaction.Commit();

                    SessionCartService.Clear(Session);
                    TempData["OrderId"] = order.Id;
                    return RedirectToAction("Success");
                }
                catch
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "We could not place the order. Please try again.");
                    return View(model);
                }
            }
        }

        public ActionResult Success()
        {
            var id = TempData["OrderId"] as int?;
            if (!id.HasValue) return RedirectToAction("Index", "Home");

            var order = _db.Orders.Include(x => x.Items).FirstOrDefault(x => x.Id == id.Value);
            if (order == null) return RedirectToAction("Index", "Home");
            return View(new OrderConfirmationViewModel { Order = order });
        }

        private static string CreateOrderNumber()
        {
            return "LW" + DateTime.UtcNow.ToString("yyyyMMdd") + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
