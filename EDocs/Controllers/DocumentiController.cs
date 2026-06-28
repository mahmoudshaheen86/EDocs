using edocs.Models;
using edocs.Services;
using System;
using System.Collections.Generic;
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
    public class DocumentiController : BaseController
    {
        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService;
        private readonly IUserCategoryService _userCategoryService;

        public DocumentiController(
            IDocumentService documentService,
            ICategoryService categoryService,
            IUserCategoryService userCategoryService)
        {
            _documentService = documentService;
            _categoryService = categoryService;
            _userCategoryService = userCategoryService;
        }

        // GET: Documenti
        [Authorize]
        public async Task<ActionResult> Index(edocs.helper.StatusMessage.MessageId? message, int? categoryID, int? ID)
        {
            try
            {
                if (categoryID.HasValue && Session["UserId"] != null)
                {
                    var userId = int.Parse(Session["UserId"].ToString());
                    var userRole = await _userCategoryService.GetUserRoleInCategoryAsync(userId, categoryID.Value);

                    if (string.IsNullOrEmpty(userRole))
                    {
                        return Redirect("~/Account/Login");
                    }

                    var viewModel = await _documentService.GetIndexViewModelAsync(categoryID.Value, message, userRole);

                    if (viewModel == null)
                    {
                        return RedirectToAction("Index", "home", new { message = edocs.helper.StatusMessage.MessageId.connectionerror });
                    }

                    viewModel.UserRole = userRole;
                    viewModel.StatusMessage = edocs.helper.StatusMessage.GetStatusMessage(message);
                    Session["selectedcategory"] = viewModel.Category;

                    return View(viewModel);
                }
                else
                {
                    return Redirect("~/Account/Login");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "home", new { message = edocs.helper.StatusMessage.MessageId.connectionerror });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> GetList(int? catId)
        {
            try
            {

                if (Session["UserId"] == null)
                    return Json(new { error = "Unauthorized" });
                var userId = int.Parse(Session["UserId"].ToString());
                var userRole = await _userCategoryService.GetUserRoleInCategoryAsync(userId, catId ?? 0);

                int draw = Convert.ToInt32(Request["draw"]);
                int start = Convert.ToInt32(Request["start"]);
                int length = Convert.ToInt32(Request["length"]);

                if (string.IsNullOrEmpty(userRole))
                {
                    return Json(new
                    {
                        draw = draw,
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>(),
                        error = "Unauthorized category role"
                    });
                }

                var allowedCategoryIds =
                    await _userCategoryService.GetAllowedCategoryIdsAsync(userId);

                var request = new DataTableRequest
                {
                    Draw = draw,
                    Start = start,
                    Length = length,
                    SearchValue = Request["search[value]"],
                    SortColumn = Request[$"columns[{Request["order[0][column]"]}][data]"],
                    SortDirection = Request["order[0][dir]"],
                    AllowedCategoryIds = allowedCategoryIds
                };

                var result = await _documentService
    .GetDataTableResultAsync(request, catId ?? 0, userRole);

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = result.RecordsTotal,
                    recordsFiltered = result.RecordsFiltered,
                    data = result.Data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = Request["draw"],
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message,
                    details = ex.StackTrace
                });
            }
        }

        [Authorize]
        // GET: Documenti/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            try
            {
                if (Session["selectedcategory"] == null || Session["UserId"] == null)
                {
                    return Redirect("~/Account/Login");
                }

                var category = (Category)Session["selectedcategory"];
                if (id == null)
                {
                    return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.dataerror, categoryID = category.ID });
                }

                var document = await _documentService.GetDocumentDetailsAsync(id.Value);
                if (document == null)
                {
                    return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.connectionerror, categoryID = category.ID });
                }

                var userId = int.Parse(Session["UserId"].ToString());
                var userRole = await _userCategoryService.GetUserRoleInCategoryAsync(userId, category.ID);
                ViewBag.userrole = userRole;

                return View(document);
            }
            catch (Exception)
            {
                if (Session["selectedcategory"] != null)
                {
                    var category = (Category)Session["selectedcategory"];
                    return RedirectToAction("Index", new { Message = edocs.helper.StatusMessage.MessageId.connectionerror, categoryID = category.ID });
                }
                return Redirect("~/Account/Login");
            }
        }

        // GET: Documenti/Create
        public async Task<ActionResult> Create(int? categoryID, edocs.helper.StatusMessage.MessageId? message)
        {
            if (!categoryID.HasValue || Session["UserId"] == null)
            {
                return Redirect("~/Account/Login");
            }

            var category = await _categoryService.GetCategoryAsync(categoryID.Value);

            if (category == null)
            {
                return RedirectToAction("Index", "Document");
            }

            var attrs = category.Attributes != null
                ? category.Attributes.OrderBy(a => a.COLM_ORDER).ToList()
                : new List<edocs.Models.DocAttribute>();

            var document = new edocs.Models.Documenti
            {
                DATEIN = DateTime.Now,
                DATEOFDOC = DateTime.Now,
                Category = category.ID,
                USERID = Convert.ToInt16(Session["UserId"]),
                DOCATTRIBUTEs = attrs.Select(a => new edocs.Models.DocAttributes
                {
                    ATTRIBUTEID = a.ID,
                    VALUEOF = ""
                }).ToList()
            };

            ViewBag.StatusMessage = edocs.helper.StatusMessage.GetStatusMessage(message);
            ViewBag.CategoryId = category.ID;
            ViewBag.Category = category;
            ViewBag.AttributeNames = attrs;

            return View(document);
        }

        [HttpPost]
        public ActionResult UploadTempFiles(int categoryId)
        {
            try
            {
                if (Session["UserId"] == null)
                    return Json(new { success = false });

                var userId = Session["UserId"].ToString();
                var tempPath = Path.Combine(Server.MapPath("~/temp/"), userId);

                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                var uploadedFiles = Session["UploadedFiles"] as List<string> ?? new List<string>();

                foreach (string file in Request.Files)
                {
                    var postedFile = Request.Files[file];

                    if (postedFile != null && postedFile.ContentLength > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(postedFile.FileName);
                        var fullPath = Path.Combine(tempPath, fileName);

                        postedFile.SaveAs(fullPath);
                        uploadedFiles.Add(fileName);
                    }
                }

                Session["UploadedFiles"] = uploadedFiles;

                return Json(new { success = true, files = uploadedFiles });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public ActionResult RemoveTempFile(string fileName)
        {
            try
            {
                var userId = Session["UserId"]?.ToString();
                if (userId == null)
                    return Json(false);

                var path = Path.Combine(Server.MapPath("~/temp/"), userId, fileName);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                var files = Session["UploadedFiles"] as List<string>;
                files?.Remove(fileName);

                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        // POST: Documenti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Documenti document, string submitbutton)
        {
            try
            {
                if (Session["UserId"] == null)
                    return Redirect("~/Account/Login");

                document.USERID = int.Parse(Session["UserId"].ToString());
                document.DATEIN = DateTime.Now;

                if (!document.DATEOFDOC.HasValue)
                    document.DATEOFDOC = DateTime.Now;

                var tempFiles = Session["UploadedFiles"] as List<string>;

                var success = await _documentService.CreateDocumentWithTempFilesAsync(
                    document,
                    Server.MapPath("~"),
                    Session["UserId"].ToString(),
                    tempFiles
                );

                if (success)
                    Session["UploadedFiles"] = null;

                if (!success)
                {
                    return RedirectToAction("Index", new
                    {
                        categoryID = document.Category,
                        message = edocs.helper.StatusMessage.MessageId.AddError
                    });
                }

                if (submitbutton == "continu")
                {
                    return RedirectToAction("Create", new
                    {
                        categoryID = document.Category,
                        message = edocs.helper.StatusMessage.MessageId.AddSuccess
                    });
                }

                return RedirectToAction("Index", new
                {
                    categoryID = document.Category,
                    message = edocs.helper.StatusMessage.MessageId.AddSuccess
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Documenti/Create POST ERROR:");
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                return RedirectToAction("Index", new
                {
                    categoryID = document.Category,
                    message = edocs.helper.StatusMessage.MessageId.connectionerror
                });
            }
        }
        // GET: Documenti/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            try
            {
                if (id == null || Session["selectedcategory"] == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var document = await _documentService.GetDocumentForEditAsync(id.Value);
                if (document == null)
                {
                    return HttpNotFound();
                }

                ViewBag.CategoryId = ((Category)Session["selectedcategory"]).ID;
                return View(document);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { message = edocs.helper.StatusMessage.MessageId.connectionerror });
            }
        }

        // POST: Documenti/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Documenti document, HttpPostedFileBase[] files)
        {
            try
            {
                if (Session["UserId"] == null)
                    return Redirect("~/Account/Login");

                document.USERID = int.Parse(Session["UserId"].ToString());

                var success = await _documentService.UpdateDocumentAsync(
                    document,
                    files,
                    Server.MapPath("~"),
                    Session["pf"]?.ToString());

                var messageId = success
                    ? edocs.helper.StatusMessage.MessageId.EditeSuccess
                    : edocs.helper.StatusMessage.MessageId.connectionerror;

                return RedirectToAction("Index", new
                {
                    categoryID = document.Category,
                    message = messageId
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Documenti/Edit POST ERROR:");
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                return RedirectToAction("Index", new
                {
                    categoryID = document.Category,
                    message = edocs.helper.StatusMessage.MessageId.connectionerror
                });
            }
        }

        // GET: Documenti/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            try
            {
                if (id == null || Session["selectedcategory"] == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var document = await _documentService.GetDocumentDetailsAsync(id.Value);
                if (document == null)
                {
                    return HttpNotFound();
                }

                return View(document);
            }
            catch
            {
                return RedirectToAction("Index", new { message = edocs.helper.StatusMessage.MessageId.connectionerror });
            }
        }

        // POST: Documenti/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var categoryId = ((Category)Session["selectedcategory"])?.ID ?? 0;
                var success = _documentService.DeleteDocumentAsync(id).Result;

                var messageId = success ? edocs.helper.StatusMessage.MessageId.DeleteSuccess : edocs.helper.StatusMessage.MessageId.connectionerror;
                return RedirectToAction("Index", new { categoryID = categoryId, message = messageId });
            }
            catch
            {
                return RedirectToAction("Index", new { message = edocs.helper.StatusMessage.MessageId.connectionerror });
            }
        }

        private string BuildAdminActionColumn()
        {
            return ",{ data: null, orderable: false, render: function (data, type, row){"
                 + "var linkEdit = '<a href=\"document/Edit?ID=' + row.ID + '\">تعديل</a>';"
                 + "var linkDetails = '<a href=\"document/Details?ID=' + row.ID + '\">استعراض</a>';"
                 + "var linkDelete = '<a href=\"document/Delete?ID=' + row.ID + '\">حذف</a>';"
                 + "var linkSend = '<a href=\"docsent/Create?ID=' + row.ID + '\">إرسال</a>';"
                 + "return linkEdit + ' | ' + linkDetails + ' | ' + linkDelete + ' | ' + linkSend;"
                 + "} }";
        }

        private string BuildManagerActionColumn()
        {
            return ",{ data: null, orderable: false, render: function (data, type, row){"
                 + "var linkEdit = '<a href=\"document/Edit?ID=' + row.ID + '\">تعديل</a>';"
                 + "var linkDetails = '<a href=\"document/Details?ID=' + row.ID + '\">استعراض</a>';"
                 + "var linkDelete = '<a href=\"document/Delete?ID=' + row.ID + '\">حذف</a>';"
                 + "var linkSend = '<a href=\"docsent/Create?ID=' + row.ID + '\">إرسال</a>';"
                 + "return linkEdit + ' | ' + linkDetails + ' | ' + linkDelete + ' | ' + linkSend;"
                 + "} }";
        }
        private string BuildReadOnlyActionColumn()
        {
            return ",{ data: null, orderable: false, render: function (data, type, row){"
                 + "var linkDetails = '<a href=\"document/Details?ID=' + row.ID + '\">استعراض</a>';"
                 + "return linkDetails;"
                 + "} }";
        }

        protected override void Dispose(bool disposing)
        {
            // DbContext is managed by DI container; don't dispose here
            base.Dispose(disposing);
        }
    }
}
