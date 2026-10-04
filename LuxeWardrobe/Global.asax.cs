using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using LuxeWardrobe.Data;
using LuxeWardrobe.Models;

namespace LuxeWardrobe
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Database.SetInitializer(new LuxeWardrobeInitializer());
            using (var db = new LuxeWardrobeDbContext())
            {
                db.Database.Initialize(false);
                EnsureRuntimeSchema(db);
                ImportBundledProductImages(db);
            }
        }

        private static void EnsureRuntimeSchema(LuxeWardrobeDbContext db)
        {
            db.Database.ExecuteSqlCommand(@"
IF OBJECT_ID(N'[dbo].[CustomerAccounts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CustomerAccounts]
    (
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_CustomerAccounts] PRIMARY KEY,
        [FullName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(150) NOT NULL,
        [PasswordHash] NVARCHAR(256) NOT NULL,
        [PasswordSalt] NVARCHAR(128) NOT NULL,
        [CreatedAtUtc] DATETIME NOT NULL,
        CONSTRAINT [UQ_CustomerAccounts_Email] UNIQUE ([Email])
    );
END

IF COL_LENGTH('dbo.CustomerAccounts', 'Phone') IS NULL
BEGIN
    ALTER TABLE [dbo].[CustomerAccounts] ADD [Phone] NVARCHAR(20) NULL;
END

IF COL_LENGTH('dbo.Orders', 'CustomerId') IS NULL
BEGIN
    ALTER TABLE [dbo].[Orders] ADD [CustomerId] INT NULL;
    CREATE INDEX [IX_Orders_CustomerId] ON [dbo].[Orders]([CustomerId]);
END

UPDATE O
SET O.CustomerId = C.Id
FROM [dbo].[Orders] O
INNER JOIN [dbo].[CustomerAccounts] C ON LOWER(O.Email) = LOWER(C.Email)
WHERE O.CustomerId IS NULL;

IF OBJECT_ID(N'[dbo].[ProductImages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProductImages]
    (
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ProductImages] PRIMARY KEY,
        [ProductId] INT NOT NULL,
        [FileName] NVARCHAR(200) NOT NULL,
        [ContentType] NVARCHAR(100) NOT NULL,
        [ImageData] VARBINARY(MAX) NOT NULL,
        [IsPrimary] BIT NOT NULL CONSTRAINT [DF_ProductImages_IsPrimary] DEFAULT(0),
        [SortOrder] INT NOT NULL CONSTRAINT [DF_ProductImages_SortOrder] DEFAULT(0),
        [CreatedAtUtc] DATETIME NOT NULL,
        CONSTRAINT [FK_ProductImages_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products]([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ProductImage_Product_Sort]
        ON [dbo].[ProductImages]([ProductId], [SortOrder]);
END

UPDATE [dbo].[Products] SET [ImageFileName] = '1.avif' WHERE [Id] = 1 AND [ImageFileName] LIKE 't-%';
UPDATE [dbo].[Products] SET [ImageFileName] = '2A.avif' WHERE [Id] = 2 AND [ImageFileName] LIKE 't-%';
UPDATE [dbo].[Products] SET [ImageFileName] = '3.avif' WHERE [Id] = 3 AND [ImageFileName] LIKE 't-%';
");
        }

        private static void ImportBundledProductImages(LuxeWardrobeDbContext db)
        {
            var products = db.Products.ToList();
            var extensions = new[] { ".avif", ".jpg", ".jpeg", ".png", ".webp" };
            var suffixes = new[] { "", "A", "B", "C", "D", "E", "F" };

            foreach (var product in products)
            {
                if (db.ProductImages.Any(x => x.ProductId == product.Id)) continue;

                var imported = 0;
                foreach (var suffix in suffixes)
                {
                    foreach (var extension in extensions)
                    {
                        var fileName = product.Id + suffix + extension;
                        var virtualPath = "~/Content/images/products/" + fileName;
                        var physicalPath = HostingEnvironment.MapPath(virtualPath);

                        if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath)) continue;

                        db.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            FileName = fileName,
                            ContentType = GetContentType(extension),
                            ImageData = File.ReadAllBytes(physicalPath),
                            IsPrimary = imported == 0,
                            SortOrder = imported + 1,
                            CreatedAtUtc = DateTime.UtcNow
                        });

                        if (imported == 0) product.ImageFileName = fileName;
                        imported++;
                    }
                }
            }

            db.SaveChanges();
        }

        private static string GetContentType(string extension)
        {
            switch ((extension ?? "").ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".webp": return "image/webp";
                case ".avif": return "image/avif";
                default: return "application/octet-stream";
            }
        }
    }
}
