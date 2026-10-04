using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace LuxeWardrobe.Models
{
    public class Product
    {
        public Product()
        {
            Sizes = new HashSet<ProductSizeInventory>();
            Images = new HashSet<ProductImage>();
        }

        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; }

        [Required, StringLength(140)]
        public string Slug { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required, StringLength(60)]
        public string Category { get; set; }

        [Required, StringLength(80)]
        public string Color { get; set; }

        [Column(TypeName = "money")]
        [Range(1, 9999999)]
        public decimal Price { get; set; }

        [Required, StringLength(200)]
        public string ImageFileName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public virtual ICollection<ProductSizeInventory> Sizes { get; set; }
        public virtual ICollection<ProductImage> Images { get; set; }

        [NotMapped]
        public int TotalStock
        {
            get { return Sizes == null ? 0 : Sizes.Sum(x => x.StockQuantity); }
        }
    }
}
