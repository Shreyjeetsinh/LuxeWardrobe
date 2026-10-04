using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LuxeWardrobe.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Index("IX_ProductImage_Product_Sort", 1)]
        public int ProductId { get; set; }

        [Required, StringLength(200)]
        public string FileName { get; set; }

        [Required, StringLength(100)]
        public string ContentType { get; set; }

        [Required]
        public byte[] ImageData { get; set; }

        public bool IsPrimary { get; set; }

        [Index("IX_ProductImage_Product_Sort", 2)]
        public int SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
