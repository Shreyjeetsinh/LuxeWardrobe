using System.Collections.Generic;
using LuxeWardrobe.Models;

namespace LuxeWardrobe.ViewModels
{
    public class CatalogViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string Query { get; set; }
        public string Category { get; set; }
        public string Size { get; set; }
        public string Sort { get; set; }
    }
}
