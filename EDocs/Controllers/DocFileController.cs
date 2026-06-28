using edocs.helper;
using edocs.Models;
using edocs.Services;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace edocs.Controllers
{
    [NoDirectAccess]
    [Authorize]
    public class DocFileController : BaseController
    {
        private readonly IFileService _fileService;
        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService;

        public DocFileController(IFileService fileService, IDocumentService documentService, ICategoryService categoryService)
        {
            _fileService = fileService;
            _documentService = documentService;
            _categoryService = categoryService;
        }

        // Allowed file extensions for upload
        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif", ".txt", ".rtf", ".tif", ".tiff" };

        // GET: DocFile
        public async Task<ActionResult> Index(StatusMessage.MessageId? message)
        {
            ViewBag.StatusMessage = StatusMessage.GetStatusMessage(message);
            var files = await _fileService.GetFilesByUserAsync(0);
            return View(files);
        }

        // GET: DocFile/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var file = await _fileService.GetFileAsync(id.Value);
            if (file == null)
                return HttpNotFound();

            return View(file);
        }

        // GET: DocFile/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DocFile/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DocFile file, HttpPostedFileBase uploadfile)
        {
            if (ModelState.IsValid && uploadfile != null && uploadfile.ContentLength > 0)
            {
                string originalFileName = Path.GetFileName(uploadfile.FileName);
                string fileExtension = Path.GetExtension(originalFileName).ToLower();

                // Validate extension
                if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("uploadfile", "نوع الملف غير مسموح به. الأنواع المسموح بها: " + string.Join(", ", AllowedExtensions));
                    return View(file);
                }

                string safeFileName = PathSecurityHelper.SanitizePathComponent(originalFileName);
                string uploadDir = Server.MapPath("~/uploads");
                Directory.CreateDirectory(uploadDir);
                string fullPath = Path.Combine(uploadDir, safeFileName);
                uploadfile.SaveAs(fullPath);

                file.NAME = safeFileName;
                file.DATEOF = DateTime.Now;
                file.STATUS = 1;
                file.USERID = int.Parse(Session["UserId"].ToString());

                await _fileService.CreateFileAsync(file, uploadfile, uploadDir);
                return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.AddSuccess });
            }

            return View(file);
        }

        // GET: DocFile/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var file = await _fileService.GetFileAsync(id.Value);
            if (file == null)
                return HttpNotFound();

            return View(file);
        }

        // POST: DocFile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DocFile file, HttpPostedFileBase uploadfile)
        {
            if (ModelState.IsValid)
            {
                var existingFile = await _fileService.GetFileAsync(file.ID);
                if (existingFile == null)
                {
                    return HttpNotFound();
                }

                string oldStorageName = existingFile.NAME;

                if (uploadfile != null && uploadfile.ContentLength > 0)
                {
                    string originalFileName = Path.GetFileName(uploadfile.FileName);
                    string fileExtension = Path.GetExtension(originalFileName).ToLower();

                    if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("uploadfile", "نوع الملف غير مسموح به.");
                        return View(file);
                    }

                    string safeFileName = PathSecurityHelper.SanitizePathComponent(originalFileName);
                    string uploadDir = Server.MapPath("~/uploads");
                    string newPath = Path.Combine(uploadDir, safeFileName);
                    uploadfile.SaveAs(newPath);

                    // Delete old file
                    try
                    {
                        string oldPath = Path.Combine(uploadDir, oldStorageName);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }
                    catch { /* ignore */ }

                    file.NAME = safeFileName;
                }
                else
                {
                    file.NAME = oldStorageName;
                }

                await _fileService.UpdateFileAsync(file);
                return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.EditeSuccess });
            }

            return View(file);
        }

        // GET: DocFile/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var file = await _fileService.GetFileAsync(id.Value);
            if (file == null)
                return HttpNotFound();

            return View(file);
        }

        // POST: DocFile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var file = await _fileService.GetFileAsync(id);
            if (file != null)
            {
                try
                {
                    string uploadDir = Server.MapPath("~/uploads");
                    string filePath = Path.Combine(uploadDir, file.NAME);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch { /* ignore */ }

                await _fileService.DeleteFileAsync(id);
                var docId = file.DOCUMENTID;
                return RedirectToAction("Edit", "document", new { ID = docId, Message = edocs.helper.StatusMessage.MessageId.DeleteSuccess });
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
