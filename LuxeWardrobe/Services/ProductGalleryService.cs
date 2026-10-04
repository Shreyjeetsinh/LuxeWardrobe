using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using LuxeWardrobe.Models;

namespace LuxeWardrobe.Services
{
    public static class ProductGalleryService
    {
        private const string ProductFolder = "~/Content/images/products/";

        public static List<string> GetImages(Product product)
        {
            var images = new List<string>();
            if (product == null) return images;

            AddIfExists(images, product.ImageFileName);

            var group = GetImageGroup(product);
            foreach (var suffix in new[] { "", "A", "B", "C", "D", "E" })
            {
                AddIfExists(images, group + suffix + ".avif");
                AddIfExists(images, group + suffix + ".jpg");
                AddIfExists(images, group + suffix + ".jpeg");
                AddIfExists(images, group + suffix + ".png");
                AddIfExists(images, group + suffix + ".webp");
            }

            if (!images.Any())
                images.Add(ProductFolder + "placeholder.svg");

            return images;
        }

        private static string GetImageGroup(Product product)
        {
            var fileName = Path.GetFileNameWithoutExtension(product.ImageFileName ?? "");
            if (fileName.StartsWith("t-", StringComparison.OrdinalIgnoreCase))
            {
                var value = fileName.Substring(2);
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }

            var leadingDigits = new string(fileName.TakeWhile(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(leadingDigits) ? product.Id.ToString() : leadingDigits;
        }

        private static void AddIfExists(List<string> images, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;

            var virtualPath = ProductFolder + fileName;
            var physicalPath = HostingEnvironment.MapPath(virtualPath);

            if (!string.IsNullOrWhiteSpace(physicalPath) &&
                File.Exists(physicalPath) &&
                !images.Any(x => string.Equals(x, virtualPath, StringComparison.OrdinalIgnoreCase)))
            {
                images.Add(virtualPath);
            }
        }
    }
}