using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using edocs.Controllers;

namespace edocs.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index(edocs.helper.StatusMessage.MessageId? message)
        {
            ViewBag.StatusMessage = edocs.helper.StatusMessage.GetStatusMessage(message);
            if (Session["UserName"] == null)
            {
                return Redirect("~/Account/Login");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "صفحة تسجيل الطلبات للتواصل معنا .";

            return View();
        }
    }
}