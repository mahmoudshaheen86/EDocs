using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using edocs.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace edocs.Controllers
{
    [Authorize(Roles = "admin")]
    public class RolesController : BaseController
    {
        private ApplicationRoleManager _roleManager;

        public RolesController()
        {
        }

        public RolesController(ApplicationRoleManager roleManager)
        {
            RoleManager = roleManager;
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        // GET: Roles
        public ActionResult Index()
        {
            var list = RoleManager.Roles
                .ToList()
                .Select(role => new RoleViewModels(role))
                .ToList();

            return View(list);
        }

        // GET: Roles/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var role = await RoleManager.FindByIdAsync(id.Value);

            if (role == null)
            {
                return HttpNotFound();
            }

            var model = new RoleViewModels(role);

            return View(model);
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View(new RoleViewModels());
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RoleViewModels model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = new Role
            {
                Name = model.Name
            };

            var result = await RoleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            AddErrors(result);

            return View(model);
        }

        // GET: Roles/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var role = await RoleManager.FindByIdAsync(id.Value);

            if (role == null)
            {
                return HttpNotFound();
            }

            var model = new RoleViewModels(role);

            return View(model);
        }

        // POST: Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] RoleViewModels model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = await RoleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                return HttpNotFound();
            }

            role.Name = model.Name;

            var result = await RoleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            AddErrors(result);

            return View(model);
        }

        // GET: Roles/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var role = await RoleManager.FindByIdAsync(id.Value);

            if (role == null)
            {
                return HttpNotFound();
            }

            var model = new RoleViewModels(role);

            return View(model);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var role = await RoleManager.FindByIdAsync(id);

            if (role == null)
            {
                return HttpNotFound();
            }

            var result = await RoleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            AddErrors(result);

            var model = new RoleViewModels(role);

            return View("Delete", model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}