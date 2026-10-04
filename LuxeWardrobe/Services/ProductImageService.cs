using System.Collections.Generic;
using System.Linq;
using LuxeWardrobe.Data;
using LuxeWardrobe.ViewModels;

namespace LuxeWardrobe.Services
{
    public static class ProductImageService
    {
        public static List<ProductImageInfoViewModel> GetImages(LuxeWardrobeDbContext db, int productId)
        {
            return db.ProductImages
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .Select(x => new ProductImageInfoViewModel
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    FileName = x.FileName,
                    IsPrimary = x.IsPrimary,
                    SortOrder = x.SortOrder
                })
                .ToList();
        }

        public static Dictionary<int, int> GetPrimaryImageIds(LuxeWardrobeDbContext db, IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();
            var metadata = db.ProductImages
                .Where(x => ids.Contains(x.ProductId))
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    x.IsPrimary,
                    x.SortOrder
                })
                .ToList();

            return metadata
                .GroupBy(x => x.ProductId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(y => y.IsPrimary)
                          .ThenBy(y => y.SortOrder)
                          .ThenBy(y => y.Id)
                          .First().Id);
        }
    }
}
