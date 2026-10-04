using System.Collections.Generic;
using System.Linq;

namespace LuxeWardrobe.ViewModels
{
    public class CartLineViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Slug { get; set; }
        public int? ImageId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public int AvailableStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get { return UnitPrice * Quantity; } }
    }

    public class CartViewModel
    {
        public CartViewModel()
        {
            Lines = new List<CartLineViewModel>();
        }

        public List<CartLineViewModel> Lines { get; set; }
        public string PromoCode { get; set; }
        public string PromoMessage { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public bool IsEmpty { get { return !Lines.Any(); } }
    }
}
