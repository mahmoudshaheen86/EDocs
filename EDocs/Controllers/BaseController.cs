using System.Web;
using System.Web.Mvc;

namespace edocs.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            // Check session user
            if (Session["Users"] == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }
        }
    }
}