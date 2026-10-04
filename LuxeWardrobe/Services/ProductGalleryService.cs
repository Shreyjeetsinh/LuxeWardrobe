using System.Collections.Generic;
using LuxeWardrobe.Models;

namespace LuxeWardrobe.Services
{
    public static class ProductGalleryService
    {
        public static List<string> GetImages(Product product)
        {
            var images = new List<string>
            {
                "~/Content/images/products/" + product.ImageFileName
            };

            switch ((product.ImageFileName ?? "").ToLowerInvariant())
            {
                case "t-1.avif":
                    images.Add("https://images.unsplash.com/photo-1598033129183-c4f50c736f10?auto=format&fit=crop&w=1200&q=88");
                    images.Add("https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf?auto=format&fit=crop&w=1200&q=88");
                    break;

                case "t-2.avif":
                    images.Add("https://images.unsplash.com/photo-1603252109303-2751441dd157?auto=format&fit=crop&w=1200&q=88");
                    images.Add("https://images.unsplash.com/photo-1607345366928-199ea26cfe3e?auto=format&fit=crop&w=1200&q=88");
                    break;

                case "t-3.avif":
                    images.Add("https://images.unsplash.com/photo-1620012253295-c15cc3e65df4?auto=format&fit=crop&w=1200&q=88");
                    images.Add("https://images.unsplash.com/photo-1596755094514-f87e34085b2c?auto=format&fit=crop&w=1200&q=88");
                    break;
            }

            return images;
        }
    }
}
