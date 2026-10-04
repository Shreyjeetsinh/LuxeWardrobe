using System;
using System.Data.Entity;
using LuxeWardrobe.Models;

namespace LuxeWardrobe.Data
{
    public class LuxeWardrobeDbContext : DbContext
    {
        public LuxeWardrobeDbContext() : base(GetConnectionString())
        {
        }

        private static string GetConnectionString()
        {
            return Environment.GetEnvironmentVariable("SQLAZURECONNSTR_LuxeWardrobeDb")
                ?? Environment.GetEnvironmentVariable("SQLCONNSTR_LuxeWardrobeDb")
                ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_LuxeWardrobeDb")
                ?? "name=LuxeWardrobeDb";
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSizeInventory> ProductSizeInventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
    }
}
