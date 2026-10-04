using System.ComponentModel.DataAnnotations;

namespace LuxeWardrobe.ViewModels
{
    public class ProductEditViewModel
    {
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

        [Range(1, 9999999)]
        public decimal Price { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Image file name")]
        public string ImageFileName { get; set; }

        [Display(Name = "Visible in storefront")]
        public bool IsActive { get; set; }

        [Range(0, 999999)] public int StockS { get; set; }
        [Range(0, 999999)] public int StockM { get; set; }
        [Range(0, 999999)] public int StockL { get; set; }
        [Range(0, 999999)] public int StockXL { get; set; }
        [Range(0, 999999)] public int StockXXL { get; set; }
    }
}
