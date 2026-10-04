using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeWardrobe.Models
{
    public class ProductSizeInventory
    {
        public int Id { get; set; }

        [Index("IX_Product_Size", 1, IsUnique = true)]
        public int ProductId { get; set; }

        [Required, StringLength(10)]
        [Index("IX_Product_Size", 2, IsUnique = true)]
        public string Size { get; set; }

        [Range(0, 999999)]
        public int StockQuantity { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
