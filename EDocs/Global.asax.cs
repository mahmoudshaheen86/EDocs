using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using edocs.Models;
using edocs.Infrastructure;

namespace edocs
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static SimpleDependencyResolver _resolver;

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Database.SetInitializer<Models.ApplicationDbContext>(null);

            // Initialize Dependency Injection
            _resolver = new SimpleDependencyResolver();
            DependencyResolver.SetResolver(_resolver);
        }

        protected void Application_EndRequest()
        {
            // Dispose request-scoped dependencies
            if (_resolver != null)
            {
                _resolver.ClearRequestInstances();
            }
        }
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NoDirectAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.UrlReferrer == null ||
                        filterContext.HttpContext.Request.Url.Host != filterContext.HttpContext.Request.UrlReferrer.Host)
            {
                filterContext.Result = new RedirectToRouteResult(new
                               RouteValueDictionary(new { controller = "Account", action = "Login", area = "" }));
            }
        }
    }
}
