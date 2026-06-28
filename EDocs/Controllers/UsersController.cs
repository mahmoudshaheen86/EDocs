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
    [Authorize]
    public class UsersController : BaseController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public UsersController()
        {
        }

        public UsersController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
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

        // GET: Users
        public ActionResult Index()
        {
            var users = UserManager.Users
                .OrderBy(u => u.UserName)
                .ToList();

            return View(users);
        }

        // GET: Users/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByIdAsync(id.Value);

            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserRoles = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            PrepareUserViewBags();
            return View(new UserFormViewModel());
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareUserViewBags(model.RoleName, model.USERTYPE);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FULLNAME = model.FULLNAME,
                USERTYPE = model.USERTYPE,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(model.RoleName))
                {
                    var roleResult = await UserManager.AddToRoleAsync(user.Id, model.RoleName);

                    if (!roleResult.Succeeded)
                    {
                        AddErrors(roleResult);
                        PrepareUserViewBags(model.RoleName, model.USERTYPE);
                        return View(model);
                    }
                }

                return RedirectToAction("Index");
            }

            AddErrors(result);
            PrepareUserViewBags(model.RoleName, model.USERTYPE);
            return View(model);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByIdAsync(id.Value);

            if (user == null)
            {
                return HttpNotFound();
            }

            var roles = await UserManager.GetRolesAsync(user.Id);
            var selectedRole = roles.FirstOrDefault();

            var model = new UserFormViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FULLNAME = user.FULLNAME,
                USERTYPE = user.USERTYPE,
                PhoneNumber = user.PhoneNumber,
                RoleName = selectedRole
            };

            PrepareUserViewBags(model.RoleName, model.USERTYPE);
            return View(model);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PrepareUserViewBags(model.RoleName, model.USERTYPE);
                return View(model);
            }

            var user = await UserManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return HttpNotFound();
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FULLNAME = model.FULLNAME;
            user.USERTYPE = model.USERTYPE;
            user.PhoneNumber = model.PhoneNumber;

            var updateResult = await UserManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                AddErrors(updateResult);
                PrepareUserViewBags(model.RoleName, model.USERTYPE);
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var passwordResult = await UserManager.ResetPasswordAsync(user.Id, token, model.Password);

                if (!passwordResult.Succeeded)
                {
                    AddErrors(passwordResult);
                    PrepareUserViewBags(model.RoleName, model.USERTYPE);
                    return View(model);
                }
            }

            var currentRoles = await UserManager.GetRolesAsync(user.Id);

            if (currentRoles.Any())
            {
                var removeResult = await UserManager.RemoveFromRolesAsync(user.Id, currentRoles.ToArray());

                if (!removeResult.Succeeded)
                {
                    AddErrors(removeResult);
                    PrepareUserViewBags(model.RoleName, model.USERTYPE);
                    return View(model);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.RoleName))
            {
                var addRoleResult = await UserManager.AddToRoleAsync(user.Id, model.RoleName);

                if (!addRoleResult.Succeeded)
                {
                    AddErrors(addRoleResult);
                    PrepareUserViewBags(model.RoleName, model.USERTYPE);
                    return View(model);
                }
            }

            return RedirectToAction("Index");
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = await UserManager.FindByIdAsync(id.Value);

            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.UserRoles = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            var result = await UserManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            AddErrors(result);
            ViewBag.UserRoles = await UserManager.GetRolesAsync(user.Id);
            return View("Delete", user);
        }

        private void PrepareUserViewBags(string selectedRole = null, string selectedUserType = null)
        {
            var roles = RoleManager.Roles
                .OrderBy(r => r.Name)
                .ToList()
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name,
                    Selected = r.Name == selectedRole
                })
                .ToList();

            ViewBag.RolesList = roles;

            var userTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "مدير النظام", Value = "admin" },
                new SelectListItem { Text = "مدير", Value = "manager" },
                new SelectListItem { Text = "محرر", Value = "Editor" },
                new SelectListItem { Text = "مستخدم", Value = "user" }
            };

            foreach (var item in userTypes)
            {
                item.Selected = item.Value == selectedUserType;
            }

            ViewBag.UserTypes = userTypes;
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
