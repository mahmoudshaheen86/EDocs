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
    public class DocAttributesController : BaseController
    {
        private readonly IDocAttributeService _docAttributeService;
        private readonly IDocumentService _documentService;
        private readonly IAttributeService _attributeService;

        public DocAttributesController(
            IDocAttributeService docAttributeService,
            IDocumentService documentService,
            IAttributeService attributeService)
        {
            _docAttributeService = docAttributeService;
            _documentService = documentService;
            _attributeService = attributeService;
        }

        // GET: DocAttributes
        public async Task<ActionResult> Index(StatusMessage.MessageId? message)
        {
            ViewBag.StatusMessage = StatusMessage.GetStatusMessage(message);
            var docAttributes = await _docAttributeService.GetByDocumentAsync(0); // Get all
            return View(docAttributes);
        }

        // GET: DocAttributes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var docAttribute = await _docAttributeService.GetByDocumentAsync(id.Value);
            var item = docAttribute.FirstOrDefault(da => da.ID == id);
            if (item == null)
                return HttpNotFound();

            return View(item);
        }

        // GET: DocAttributes/Create
        public async Task<ActionResult> Create()
        {
            await PopulateSelectListsAsync(null);
            return View();
        }

        // POST: DocAttributes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DocAttributes docAttribute)
        {
            if (ModelState.IsValid)
            {
                await _docAttributeService.CreateAsync(docAttribute);
                return RedirectToAction("Index", new { Message = DOCAttributeMessageId.AddSuccess });
            }

            await PopulateSelectListsAsync(docAttribute.DOCUMENTID);
            return View(docAttribute);
        }

        // GET: DocAttributes/Edit/5
        public async System.Threading.Tasks.Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var docAttr = await _docAttributeService.GetByDocumentAsync(id.Value);
            var docAttribute = docAttr.FirstOrDefault(da => da.ID == id);
            if (docAttribute == null)
                return HttpNotFound();

            await PopulateSelectListsAsync(docAttribute.DOCUMENTID);
            return View(docAttribute);
        }

        // POST: DocAttributes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DocAttributes docAttribute)
        {
            if (ModelState.IsValid)
            {
                await _docAttributeService.UpdateAsync(docAttribute);
                return RedirectToAction("Index", new { Message = DOCAttributeMessageId.EditeSuccess });
            }

            await PopulateSelectListsAsync(docAttribute.DOCUMENTID);
            return View(docAttribute);
        }

        // GET: DocAttributes/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var docAttr = await _docAttributeService.GetByDocumentAsync(id.Value);
            var docAttribute = docAttr.FirstOrDefault(da => da.ID == id);
            if (docAttribute == null)
                return HttpNotFound();

            return View(docAttribute);
        }

        // POST: DocAttributes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _docAttributeService.DeleteAsync(id);
            return RedirectToAction("Index", new { Message = DOCAttributeMessageId.DeleteSuccess });
        }

        private async System.Threading.Tasks.Task PopulateSelectListsAsync(int? documentId)
        {
            var attributes = await _attributeService.GetAttributesByCategoryAsync(0); // Get all
            ViewBag.ATTRIBUTEID = new SelectList(attributes, "ID", "NAME");
            var documents = await _documentService.GetRecentDocumentsAsync(0, 1000); // get many
            ViewBag.DOCUMENTID = new SelectList(documents, "ID", "NUMBEROF", documentId);
        }

        public enum DOCAttributeMessageId
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