using edocs.Models;
using edocs.Services;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace edocs.Controllers
{
    [Authorize]
    public class AttributeListController : BaseController
    {
        private readonly IAttributeListService _attributeListService;
        private readonly IAttributeService _attributeService;

        public AttributeListController(IAttributeListService attributeListService, IAttributeService attributeService)
        {
            _attributeListService = attributeListService;
            _attributeService = attributeService;
        }

        // GET: AttributeList
        public async Task<ActionResult> Index(int? id, AttributeListMessageId? message)
        {
            ViewBag.StatusMessage = GetStatusMessage(message);
            ViewBag.AttId = id;

            if (id.HasValue)
            {
                var values = await _attributeListService.GetValuesByAttributeAsync(id.Value);
                return View(values);
            }

            return View();
        }

        // GET: AttributeList/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var value = await _attributeListService.GetValuesByAttributeAsync(id.Value);
            var item = value.FirstOrDefault(v => v.ID == id);
            if (item == null)
                return HttpNotFound();

            return View(item);
        }

        // GET: AttributeList/Create
        public async Task<ActionResult> Create(int? id)
        {
            if (id.HasValue)
            {
                var attribute = await _attributeService.GetAttributeAsync(id.Value);
                if (attribute != null && attribute.TYPE == "قائمة")
                {
                    ViewBag.ATTRID = new SelectList(new[] { attribute }, "ID", "NAME", id);
                }
            }
            return View();
        }

        // POST: AttributeList/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(AttributeList model)
        {
            if (ModelState.IsValid)
            {
                await _attributeListService.CreateValueAsync(model);
                return RedirectToAction("Index", new { id = model.ATTRID, Message = AttributeListMessageId.AddSuccess });
            }

            ViewBag.ATTRID = new SelectList(await _attributeService.GetAttributesByCategoryAsync(0), "ID", "NAME", model.ATTRID);
            return View(model);
        }

        // GET: AttributeList/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var value = await _attributeListService.GetValuesByAttributeAsync(id.Value);
            var item = value.FirstOrDefault(v => v.ID == id);
            if (item == null)
                return HttpNotFound();

            ViewBag.ATTRID = new SelectList(await _attributeService.GetAttributesByCategoryAsync(0), "ID", "NAME", item.ATTRID);
            return View(item);
        }

        // POST: AttributeList/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AttributeList model)
        {
            if (ModelState.IsValid)
            {
                await _attributeListService.UpdateValueAsync(model);
                return RedirectToAction("Index", new { id = model.ATTRID, Message = AttributeListMessageId.EditeSuccess });
            }

            ViewBag.ATTRID = new SelectList(await _attributeService.GetAttributesByCategoryAsync(0), "ID", "NAME", model.ATTRID);
            return View(model);
        }

        // GET: AttributeList/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var value = await _attributeListService.GetValuesByAttributeAsync(id.Value);
            var item = value.FirstOrDefault(v => v.ID == id);
            if (item == null)
                return HttpNotFound();

            return View(item);
        }

        // POST: AttributeList/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var value = await _attributeListService.GetValuesByAttributeAsync(id);
            var item = value.FirstOrDefault(v => v.ID == id);
            var attrId = item?.ATTRID;

            await _attributeListService.DeleteValueAsync(id);
            return RedirectToAction("Index", new { id = attrId, Message = AttributeListMessageId.DeleteSuccess });
        }

        private string GetStatusMessage(AttributeListMessageId? message)
        {
            if (!message.HasValue) return "";

            switch (message.Value)
            {
                case AttributeListMessageId.AddSuccess:
                    return "<div class=\"alert alert-success\"><strong>إضافة</strong>تم عملية الإضافة بنجاح </div>";

                case AttributeListMessageId.AddError:
                    return "<div class=\"alert alert-danger\"><strong>تنبيه</strong>فشل في عملية الإضافة </div>";

                case AttributeListMessageId.EditeSuccess:
                    return "<div class=\"alert alert-info\"><strong>تعديل</strong>تم عملية التعديل بنجاح </div>";

                case AttributeListMessageId.DeleteSuccess:
                    return "<div class=\"alert alert-danger\"><strong>حذف</strong>تمت عملية الحذف بنجاح</div>";

                default:
                    return "";
            }
        }

        public enum AttributeListMessageId
        {
            AddSuccess,
            AddError,
            EditeSuccess,
            EditeError,
            DeleteSuccess,
            DeleteError
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
