using edocs.helper;
using edocs.Models;
using edocs.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace edocs.Controllers
{
    [NoDirectAccess]
    [Authorize]
    public class UserCategoryController : BaseController
    {
        private readonly IUserCategoryService _userCategoryService;
        private readonly ICategoryService _categoryService;

        public UserCategoryController(
            IUserCategoryService userCategoryService,
            ICategoryService categoryService)
        {
            _userCategoryService = userCategoryService;
            _categoryService = categoryService;
        }

        private ApplicationUserManager UserManager
        {
            get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
        }

        public async Task<ActionResult> Index(StatusMessage.MessageId? message)
        {
            ViewBag.StatusMessage = StatusMessage.GetStatusMessage(message);
            ViewBag.columns = new UserCategoryColumns().names.ToString();
            ViewBag.mode = "HasData";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> GetList()
        {
            int start = Convert.ToInt32(Request["start"]);
            int length = Convert.ToInt32(Request["length"]);
            var draw = Request["draw"];

            int skip = start;
            int pagesize = length > 0 ? length : int.MaxValue;

            string searchValue = Request["search[value]"]?.ToLower() ?? "";
            string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][data]"] ?? "ID";
            string sortDirection = Request["order[0][dir]"] ?? "asc";

            var userCategories = await _userCategoryService.GetUsersCategoriesAsync();
            var query = userCategories.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(c =>
                    (c.TYPE ?? "").ToLower().Contains(searchValue) ||
                    (c.UserFK.UserName ?? "").ToLower().Contains(searchValue) ||
                    (c.CategoryFK.NAME ?? "").ToLower().Contains(searchValue));
            }

            query = ApplySorting(query, sortColumnName, sortDirection);

            int recordsTotal = query.Count();

            var data = query.Skip(skip).Take(pagesize)
                .Select(c => new
                {
                    ID = c.ID,
                    NAME = c.CategoryFK != null ? c.CategoryFK.NAME : "",
                    UserName = c.UserFK != null ? c.UserFK.UserName : "",
                    TYPE = c.TYPE
                })
                .ToList();

            return Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data }, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<UserCategory> ApplySorting(IQueryable<UserCategory> query, string sortColumn, string sortDirection)
        {
            switch (sortColumn)
            {
                case "ID":
                    return sortDirection == "asc" ? query.OrderBy(c => c.ID) : query.OrderByDescending(c => c.ID);

                case "TYPE":
                    return sortDirection == "asc" ? query.OrderBy(c => c.TYPE) : query.OrderByDescending(c => c.TYPE);

                case "UserName":
                    return sortDirection == "asc" ? query.OrderBy(c => c.UserFK.UserName) : query.OrderByDescending(c => c.UserFK.UserName);

                case "NAME":
                    return sortDirection == "asc" ? query.OrderBy(c => c.CategoryFK.NAME) : query.OrderByDescending(c => c.CategoryFK.NAME);
                default:
                    return query.OrderBy(c => c.ID);
            }
        }

        // GET: UserCategory/Details/5
        public async System.Threading.Tasks.Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var userCategory = await _userCategoryService.GetByIdAsync(id.Value);
            if (userCategory == null)
                return HttpNotFound();

            return View(userCategory);
        }

        // GET: UserCategory/Create
        public async System.Threading.Tasks.Task<ActionResult> Create()
        {
            await PopulateDropDownsAsync();
            return View();
        }

        // POST: UserCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Create(UserCategory userCategory)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userCategoryService.GetUserAsync(userCategory.USERID.Value);
                userCategory.TYPE = user.USERTYPE;
                await _userCategoryService.CreateAsync(userCategory);
                return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.AddSuccess });
            }

            await PopulateDropDownsAsync(userCategory.USERID, userCategory.CATEGORYID);
            return View(userCategory);
        }

        // GET: UserCategory/Edit/5
        public async System.Threading.Tasks.Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var userCategory = await _userCategoryService.GetByIdAsync(id.Value);
            if (userCategory == null)
                return HttpNotFound();

            await PopulateDropDownsAsync(userCategory.USERID, userCategory.CATEGORYID);
            return View(userCategory);
        }

        // POST: UserCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Edit(UserCategory userCategory)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userCategoryService.GetUserAsync(userCategory.USERID.Value);
                userCategory.TYPE = user.USERTYPE;
                await _userCategoryService.UpdateAsync(userCategory);
                return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.EditeSuccess });
            }

            await PopulateDropDownsAsync(userCategory.USERID, userCategory.CATEGORYID);
            return View(userCategory);
        }

        // GET: UserCategory/Delete/5
        public async System.Threading.Tasks.Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var userCategory = await _userCategoryService.GetByIdAsync(id.Value);
            if (userCategory == null)
                return HttpNotFound();

            return View(userCategory);
        }

        // POST: UserCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> DeleteConfirmed(int id)
        {
            await _userCategoryService.DeleteAsync(id);
            return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.DeleteSuccess });
        }

        private async System.Threading.Tasks.Task PopulateDropDownsAsync(int? selectedUserId = null, int? selectedCategoryId = null)
        {
            var users = UserManager.Users.ToList();
            ViewBag.USERID = new SelectList(users, "Id", "UserName", selectedUserId);

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.CATEGORYID = new SelectList(categories, "ID", "NAME", selectedCategoryId);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
