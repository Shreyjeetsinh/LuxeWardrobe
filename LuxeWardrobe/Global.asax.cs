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
            }
        }
    }
}
