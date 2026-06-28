using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;


namespace edocs.Controllers
{
    [Authorize]
    public class LogController : BaseController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LogController));

        // GET: Default

        public ActionResult Index()
        {
            try
            {
                Log.Debug("Hi I am log4net Debug Level");
                Log.Info("Hi I am log4net Info Level");
                Log.Warn("Hi I am log4net Warn Level");
               // throw new NullReferenceException();
                return View();
            }
            catch (Exception ex)
            {
                Log.Error("Hi I am log4net Error Level", ex);
                Log.Fatal("Hi I am log4net Fatal Level", ex);
                throw;
            }
        }
    }
}