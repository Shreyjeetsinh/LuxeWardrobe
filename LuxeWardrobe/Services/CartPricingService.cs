using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using LuxeWardrobe.Data;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Services
{
    public static class CartPricingService
    {
        public const string SupportedPromoCode = "LUXE10";

        public static CartViewModel Build(LuxeWardrobeDbContext db, IEnumerable<SessionCartItem> sessionItems, string promoCode)
        {
            var result = new CartViewModel { PromoCode = promoCode };
            var items = sessionItems.ToList();
            var productIds = items.Select(x => x.ProductId).Distinct().ToList();

            var products = db.Products
                .Include(x => x.Sizes)
                .Where(x => productIds.Contains(x.Id) && x.IsActive)
                .ToList();

            foreach (var cartItem in items)
            {
                var product = products.FirstOrDefault(x => x.Id == cartItem.ProductId);
                if (product == null) continue;

                var inventory = product.Sizes.FirstOrDefault(x => string.Equals(x.Size, cartItem.Size, StringComparison.OrdinalIgnoreCase));
                if (inventory == null) continue;

                var safeQuantity = Math.Max(1, Math.Min(cartItem.Quantity, 10));
                result.Lines.Add(new CartLineViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Slug = product.Slug,
                    ImageFileName = product.ImageFileName,
                    Color = product.Color,
                    Size = inventory.Size,
                    Quantity = safeQuantity,
                    AvailableStock = inventory.StockQuantity,
                    UnitPrice = product.Price
                });
            }

            result.Subtotal = result.Lines.Sum(x => x.LineTotal);
            result.DeliveryFee = result.IsEmpty || result.Subtotal >= 2000m ? 0m : 20m;

            if (!string.IsNullOrWhiteSpace(promoCode))
            {
                if (string.Equals(promoCode, SupportedPromoCode, StringComparison.OrdinalIgnoreCase) && result.Subtotal >= 1500m)
                {
                    result.Discount = Math.Min(Math.Round(result.Subtotal * 0.10m, 2), 500m);
                    result.PromoMessage = "LUXE10 applied — 10% off (up to ₹500).";
                }
                else if (string.Equals(promoCode, SupportedPromoCode, StringComparison.OrdinalIgnoreCase))
                {
                    result.PromoMessage = "LUXE10 needs a cart value of at least ₹1,500.";
                }
                else
                {
                    result.PromoMessage = "That promo code is not valid.";
                }
            }

            result.Total = Math.Max(0m, result.Subtotal - result.Discount + result.DeliveryFee);
            return result;
        }
    }
}
