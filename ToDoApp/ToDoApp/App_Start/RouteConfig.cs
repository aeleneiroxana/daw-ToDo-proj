using System.Web.Mvc;
using System.Web.Routing;

namespace ToDoApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Dashboard",
                url: "Dashboard",
                defaults: new { controller = "Home", action = "Dashboard" }
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "TasksDetails",
                url: "{controller}/{action}/{title}",
                defaults: new { controller = "Home", action = "Index", title = UrlParameter.Optional }
            );

        }
    }
}