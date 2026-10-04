using System.Collections.Generic;
using LuxeWardrobe.Models;

namespace LuxeWardrobe.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<string> GalleryImages { get; set; }
    }
}
