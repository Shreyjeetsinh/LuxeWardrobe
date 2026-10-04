using System;
using System.Collections.Generic;
using System.Data.Entity;
using LuxeWardrobe.Models;
using LuxeWardrobe.Services;

namespace LuxeWardrobe.Data
{
    public class LuxeWardrobeInitializer : CreateDatabaseIfNotExists<LuxeWardrobeDbContext>
    {
        protected override void Seed(LuxeWardrobeDbContext context)
        {
            var products = new List<Product>
            {
                BuildProduct("Printed cotton rugby shirt", "printed-cotton-rugby-shirt", "A polished striped rugby shirt in breathable cotton.", "Shirts", "Navy blue / Striped", 1299m, "1.avif", 9),
                BuildProduct("Cotton shirt", "cotton-shirt-light-blue", "Everyday cotton shirt with a clean, relaxed fit.", "Shirts", "Light Blue", 799m, "2A.avif", 12),
                BuildProduct("Cotton shirt", "cotton-shirt-white", "Minimal white cotton shirt designed for easy everyday styling.", "Shirts", "White", 799m, "3.avif", 10)
            };

            products.ForEach(x => context.Products.Add(x));

            string salt;
            var hash = PasswordSecurity.HashPassword("ChangeMe123!", out salt);
            context.AdminUsers.Add(new AdminUser
            {
                Username = "admin",
                PasswordHash = hash,
                PasswordSalt = salt
            });

            context.SaveChanges();
            base.Seed(context);
        }

        private static Product BuildProduct(string name, string slug, string description, string category, string color, decimal price, string image, int baseStock)
        {
            var product = new Product
            {
                Name = name,
                Slug = slug,
                Description = description,
                Category = category,
                Color = color,
                Price = price,
                ImageFileName = image,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            foreach (var size in new[] { "S", "M", "L", "XL", "XXL" })
            {
                product.Sizes.Add(new ProductSizeInventory { Size = size, StockQuantity = baseStock });
            }

            return product;
        }
    }
}
