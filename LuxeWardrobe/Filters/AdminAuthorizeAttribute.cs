using System.Web.Mvc;

namespace LuxeWardrobe.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["AdminUserId"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Admin", action = "Login", returnUrl = filterContext.HttpContext.Request.RawUrl }));
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
