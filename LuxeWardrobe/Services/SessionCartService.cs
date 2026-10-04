using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuxeWardrobe.Services
{
    [Serializable]
    public class SessionCartItem
    {
        public int ProductId { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
    }

    public static class SessionCartService
    {
        private const string CartKey = "LuxeWardrobe.Cart";
        private const string PromoKey = "LuxeWardrobe.Promo";

        public static List<SessionCartItem> GetItems(HttpSessionStateBase session)
        {
            var items = session[CartKey] as List<SessionCartItem>;
            if (items == null)
            {
                items = new List<SessionCartItem>();
                session[CartKey] = items;
            }
            return items;
        }

        public static void Add(HttpSessionStateBase session, int productId, string size, int quantity)
        {
            var items = GetItems(session);
            var existing = items.FirstOrDefault(x => x.ProductId == productId && string.Equals(x.Size, size, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                items.Add(new SessionCartItem { ProductId = productId, Size = size, Quantity = quantity });
            }
            else
            {
                existing.Quantity += quantity;
            }
        }

        public static void SetQuantity(HttpSessionStateBase session, int productId, string size, int quantity)
        {
            var items = GetItems(session);
            var item = items.FirstOrDefault(x => x.ProductId == productId && string.Equals(x.Size, size, StringComparison.OrdinalIgnoreCase));
            if (item == null) return;

            if (quantity <= 0) items.Remove(item);
            else item.Quantity = quantity;
        }

        public static void Remove(HttpSessionStateBase session, int productId, string size)
        {
            var items = GetItems(session);
            var item = items.FirstOrDefault(x => x.ProductId == productId && string.Equals(x.Size, size, StringComparison.OrdinalIgnoreCase));
            if (item != null) items.Remove(item);
        }

        public static int Count(HttpSessionStateBase session)
        {
            return GetItems(session).Sum(x => x.Quantity);
        }

        public static string GetPromoCode(HttpSessionStateBase session)
        {
            return session[PromoKey] as string;
        }

        public static void SetPromoCode(HttpSessionStateBase session, string code)
        {
            session[PromoKey] = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
        }

        public static void Clear(HttpSessionStateBase session)
        {
            session.Remove(CartKey);
            session.Remove(PromoKey);
        }
    }
}
