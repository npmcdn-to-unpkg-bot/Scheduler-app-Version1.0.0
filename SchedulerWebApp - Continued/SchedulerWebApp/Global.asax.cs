using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Newtonsoft.Json;
using SchedulerWebApp.Models.DBContext;

namespace SchedulerWebApp
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer(new CustomDbInitializer());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                                                {
                                                    Formatting = Newtonsoft.Json.Formatting.Indented,
                                                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                                                };

            GlobalFilters.Filters.Add(new RequireHttpsAttribute());
        }
    }
}