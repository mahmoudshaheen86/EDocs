using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System.Web.ModelBinding;
using edocs.Models;

[assembly: OwinStartupAttribute(typeof(edocs.Startup))]
namespace edocs
{
    public partial class Startup
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public void Configuration(IAppBuilder app)
        {             
            ConfigureAuth(app);          
        }
    }
}
