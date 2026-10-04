using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;
using LuxeWardrobe.Data;

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
");
        }
    }
}
