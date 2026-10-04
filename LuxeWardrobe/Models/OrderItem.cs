using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeWardrobe.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        [Required, StringLength(120)]
        public string ProductName { get; set; }

        [Required, StringLength(10)]
        public string Size { get; set; }

        [Required, StringLength(80)]
        public string Color { get; set; }

        [Column(TypeName = "money")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
    }
}
