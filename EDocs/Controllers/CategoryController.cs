using edocs.helper;
using edocs.Models;
using edocs.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace edocs.Controllers
{
    [NoDirectAccess]
    [Authorize]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Category
        public async Task<ActionResult> Index(StatusMessage.MessageId? message)
        {
            ViewBag.StatusMessage = StatusMessage.GetStatusMessage(message);
            ViewBag.columns = new Columns().names.ToString();
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

            var categories = await _categoryService.GetAllCategoriesAsync();
            var query = categories.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(c =>
                    (c.NAME ?? "").ToLower().Contains(searchValue) ||
                    (c.DESCR ?? "").ToLower().Contains(searchValue) ||
                    (c.FOLDER_NAME ?? "").ToLower().Contains(searchValue) ||
                    (c.ParentCategory.NAME ?? "").ToLower().Contains(searchValue));
            }

            query = ApplySorting(query, sortColumnName, sortDirection);

            int recordsTotal = query.Count();

            var data = query.Skip(skip).Take(pagesize)
                .Select(c => new
                {
                    ID = c.ID,
                    NAME = c.NAME,
                    PARENT_NAME = c.ParentCategory != null ? c.ParentCategory.NAME : "",
                    DESCR = c.DESCR,
                    FOLDER_NAME = c.FOLDER_NAME
                })
                .ToList();

            return Json(new { draw, recordsTotal, recordsFiltered = recordsTotal, data }, JsonRequestBehavior.AllowGet);
        }

        // GET: Category/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = await _categoryService.GetCategoryAsync(id.Value);
            if (category == null)
                return HttpNotFound();

            return View(category);
        }

        // GET: Category/Create
        public async Task<ActionResult> Create()
        {
            await PopulateParentCategoriesDropDown();
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.CreateCategoryAsync(category);
                return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.AddSuccess });
            }

            await PopulateParentCategoriesDropDown(category.PARENT_NAME);
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = await _categoryService.GetCategoryAsync(id.Value);
            if (category == null)
                return HttpNotFound();

            await PopulateParentCategoriesDropDown(category.PARENT_NAME);
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.UpdateCategoryAsync(category);
                return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.EditeSuccess });
            }

            await PopulateParentCategoriesDropDown(category.PARENT_NAME);
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = await _categoryService.GetCategoryAsync(id.Value);
            if (category == null)
                return HttpNotFound();

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.DeleteSuccess });
        }

        private async System.Threading.Tasks.Task PopulateParentCategoriesDropDown(int? selectedId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.PARENT_NAME = new SelectList(categories, "ID", "NAME", selectedId);
        }

        private IQueryable<Category> ApplySorting(IQueryable<Category> query, string sortColumn, string sortDirection)
        {
            switch (sortColumn)
            {
                case "ID":
                    return sortDirection == "asc" ? query.OrderBy(c => c.ID) : query.OrderByDescending(c => c.ID);

                case "NAME":
                    return sortDirection == "asc" ? query.OrderBy(c => c.NAME) : query.OrderByDescending(c => c.NAME);

                case "DESCR":
                    return sortDirection == "asc" ? query.OrderBy(c => c.DESCR) : query.OrderByDescending(c => c.DESCR);

                case "FOLDER_NAME":
                    return sortDirection == "asc" ? query.OrderBy(c => c.FOLDER_NAME) : query.OrderByDescending(c => c.FOLDER_NAME);
                default:
                    return query.OrderBy(c => c.ID);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
