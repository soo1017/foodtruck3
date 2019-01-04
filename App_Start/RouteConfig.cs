using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FoodTruck3
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Account", "Account/{action}/{id}", new { controller = "Account", action = "Index", id = UrlParameter.Optional }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("Cart", "Cart/{action}/{id}", new { controller = "Cart", action = "Index", id = UrlParameter.Optional }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("ProductOnCategory", "Shop/{action}/{name}", new { controller = "Shop", action = "ProductOnCategory", name = UrlParameter.Optional }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("ProductOneCategoryPartial", "Pages/{action}/{name}", new { controller = "Pages", action = "ProductOneCategoryPartial", name = UrlParameter.Optional }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("ProductAllCategoryPartial", "Pages/ProductAllCategoryPartial", new { controller = "Pages", action = "ProductAllCategoryPartial" }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("CategoryMenuPartial", "Pages/CategoryMenuPartial", new { controller = "Pages", action = "CategoryMenuPartial" }, new[] { "FoodTruck3.Controllers" });           
            routes.MapRoute("PagesMenuPartial", "Pages/PagesMenuPartial", new { controller = "Pages", action = "PagesMenuPartial" }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("PagesFTLocPartial", "Pages/PagesFTLocPartial", new { controller = "Pages", action = "PagesFTLocPartial" }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("PagesMapFTLocPartial", "Pages/PagesMapFTLocPartial", new { controller = "Pages", action = "PagesMapFTLocPartial" }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("Pages", "{page}", new { controller = "Pages", action = "Index" }, new[] { "FoodTruck3.Controllers" });
            routes.MapRoute("Default", "", new { controller = "Pages", action = "Index" }, new[] { "FoodTruck3.Controllers" });
            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}
