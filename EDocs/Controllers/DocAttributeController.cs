using edocs.helper;
using edocs.Models;
using edocs.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace edocs.Controllers
{
    [NoDirectAccess]
    [Authorize]
    public class DocAttributeController : BaseController
    {
        private readonly IAttributeService _attributeService;
        private readonly ICategoryService _categoryService;

        public DocAttributeController(IAttributeService attributeService, ICategoryService categoryService)
        {
            _attributeService = attributeService;
            _categoryService = categoryService;
        }

        // GET: DocAttribute
        public async Task<ActionResult> Index(int? id, StatusMessage.MessageId? message)
        {
            ViewBag.StatusMessage = StatusMessage.GetStatusMessage(message);
            ViewBag.CatID = id;
            var attributes = Enumerable.Empty<DocAttribute>();
            if (id.HasValue)
            {
                attributes = await _attributeService.GetAttributesByCategoryAsync(id.Value);
            }
            else
            {
                attributes = await _attributeService.GetAttributesAsync();
            }

            return View(attributes);
        }

        // GET: DocAttribute/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var attribute = await _attributeService.GetAttributeAsync(id.Value);
            if (attribute == null)
                return HttpNotFound();

            return View(attribute);
        }

        // GET: DocAttribute/Create
        public async Task<ActionResult> Create(int? id)
        {
            await PopulateSelectListsAsync(id);
            return View();
        }

        // POST: DocAttribute/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DocAttribute attribute)
        {
            if (ModelState.IsValid)
            {
                await _attributeService.CreateAttributeAsync(attribute);
                return RedirectToAction("Index", new { id = attribute.Category, Message = edocs.helper.StatusMessage.MessageId.AddSuccess });
            }

            await PopulateSelectListsAsync(attribute.Category);
            return View(attribute);
        }

        // GET: DocAttribute/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var attribute = await _attributeService.GetAttributeAsync(id.Value);
            if (attribute == null)
                return HttpNotFound();

            ViewBag.TYPE = new SelectList(new[] { "رقم", "تاريخ", "نص", "ملف", "قائمة" }, attribute.TYPE);
            await PopulateCategoriesSelectListAsync(attribute.Category);

            var maxOrder = await GetMaxOrderAsync(attribute.Category.Value);
            ViewBag.max_col_order = maxOrder;

            return View(attribute);
        }

        // POST: DocAttribute/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DocAttribute attribute)
        {
            if (ModelState.IsValid)
            {
                await _attributeService.UpdateAttributeAsync(attribute);
                return RedirectToAction("Index", new { id = attribute.Category, Message = edocs.helper.StatusMessage.MessageId.EditeSuccess });
            }

            ViewBag.TYPE = new SelectList(new[] { "رقم", "تاريخ", "نص", "ملف", "قائمة" }, attribute.TYPE);
            await PopulateCategoriesSelectListAsync(attribute.Category);
            return View(attribute);
        }

        // GET: DocAttribute/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var attribute = await _attributeService.GetAttributeAsync(id.Value);
            if (attribute == null)
                return HttpNotFound();

            return View(attribute);
        }

        // POST: DocAttribute/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var attribute = await _attributeService.GetAttributeAsync(id);

            if (attribute == null)
            {
                return HttpNotFound();
            }

            var catId = attribute.Category;

            try
            {
                bool isUsed = await _attributeService.IsAttributeUsedAsync(id);

                if (isUsed)
                {
                    ModelState.AddModelError(
                        "",
                        "لا يمكن حذف هذه الواصفة لأنها مستخدمة في وثائق موجودة. لم تكتمل العملية، يرجى مراجعة مدير النظام."
                    );

                    return View("Delete", attribute);
                }

                await _attributeService.DeleteAttributeAsync(id);

                return RedirectToAction("Index", new
                {
                    id = catId,
                    Message = edocs.helper.StatusMessage.MessageId.DeleteSuccess
                });
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "حدث خطأ أثناء تنفيذ العملية. لم تكتمل العملية، يرجى مراجعة مدير النظام."
                );

                return View("Delete", attribute);
            }
        }

        private async System.Threading.Tasks.Task PopulateSelectListsAsync(int? categoryId)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Category = new SelectList(categories, "ID", "NAME", categoryId);
            ViewBag.TYPE = new SelectList(new[] { "رقم", "تاريخ", "نص", "ملف", "قائمة" });

            if (categoryId.HasValue)
            {
                var maxOrder = await GetMaxOrderAsync(categoryId.Value);
                ViewBag.max_col_order = maxOrder;
            }
        }

        private async System.Threading.Tasks.Task PopulateCategoriesSelectListAsync(int? selectedCategoryId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Category = new SelectList(categories, "ID", "NAME", selectedCategoryId);
        }

        private async Task<int> GetMaxOrderAsync(int categoryId)
        {
            var attributes = await _attributeService.GetAttributesByCategoryAsync(categoryId);
            var maxOrder = attributes.Any() ? attributes.Max(a => a.COLM_ORDER) : 0;
            return maxOrder;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}