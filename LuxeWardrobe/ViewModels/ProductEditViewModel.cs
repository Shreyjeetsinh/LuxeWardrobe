using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LuxeWardrobe.ViewModels
{
    public class ProductEditViewModel
    {
        public ProductEditViewModel()
        {
            ExistingImages = new List<ProductImageInfoViewModel>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(120)]
        public string Name { get; set; }

        [StringLength(140)]
        [Display(Name = "URL slug")]
        public string Slug { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(60)]
        public string Category { get; set; }

        [Required(ErrorMessage = "Color is required.")]
        [StringLength(80)]
        public string Color { get; set; }

        [Range(1, 9999999, ErrorMessage = "Enter a valid price.")]
        public decimal Price { get; set; }

        [Display(Name = "Visible in storefront")]
        public bool IsActive { get; set; }

        [Range(0, 999999)] public int StockS { get; set; }
        [Range(0, 999999)] public int StockM { get; set; }
        [Range(0, 999999)] public int StockL { get; set; }
        [Range(0, 999999)] public int StockXL { get; set; }
        [Range(0, 999999)] public int StockXXL { get; set; }

        public List<ProductImageInfoViewModel> ExistingImages { get; set; }
    }
}
