using edocs.Models;
using edocs.Repositories;
using edocs.helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using edocs.ViewModels;
using Newtonsoft.Json;
using System.Globalization;

namespace edocs.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAttributeRepository _attributeRepository;
        private readonly IDocAttributeRepository _docAttributeRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IUserCategoryRepository _userCategoryRepository;
        private readonly ApplicationDbContext _context;
        private static readonly DateTime MinDate = new DateTime(1900, 4, 30);
        private static readonly DateTime MaxDate = new DateTime(2077, 11, 16);

        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif", ".txt", ".rtf", ".tif", ".tiff" };

        public DocumentService(
            IDocumentRepository documentRepository,
            ICategoryRepository categoryRepository,
            IAttributeRepository attributeRepository,
            IDocAttributeRepository docAttributeRepository,
            IFileRepository fileRepository,
            IUserCategoryRepository userCategoryRepository,
            ApplicationDbContext context)
        {
            _documentRepository = documentRepository;
            _categoryRepository = categoryRepository;
            _attributeRepository = attributeRepository;
            _docAttributeRepository = docAttributeRepository;
            _fileRepository = fileRepository;
            _userCategoryRepository = userCategoryRepository;
            _context = context;
        }

        public async Task<DocumentIndexViewModel> GetIndexViewModelAsync(int categoryId, edocs.helper.StatusMessage.MessageId? message, string userRole)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null) return null;

            var attributes = await _attributeRepository.GetByCategoryIdOrderedAsync(categoryId);

            // Also include attributes from parent category
            if (category.PARENT_NAME.HasValue)
            {
                var parentAttributes = await _attributeRepository.GetByCategoryIdOrderedAsync(category.PARENT_NAME.Value);
                attributes = attributes.Concat(parentAttributes).OrderBy(a => a.COLM_ORDER).ToList();
            }

            var columns = new List<DataTableColumn>();

            // Fixed columns
            columns.Add(new DataTableColumn { data = "NUMBEROF", @class = "red_design" });
            columns.Add(new DataTableColumn { data = "DATEOFDOC" });

            // Dynamic attributes
            foreach (var attr in attributes)
            {
                columns.Add(new DataTableColumn
                {
                    data = ToDataTablesAttributePath(attr.NAME),
                    defaultContent = ""
                });
            }

            // Attachments
            columns.Add(new DataTableColumn
            {
                data = "ATTACHMENTS",
                defaultContent = "0"
            });

            // Actions
            columns.Add(new DataTableColumn
            {
                data = "ACTIONS",
                orderable = false,
                searchable = false,
                defaultContent = "",
                @class = "actions-column no-filter"
            });


            string statusMessage = edocs.helper.StatusMessage.GetStatusMessage(message);

            return new DocumentIndexViewModel
            {
                CategoryId = categoryId,
                Category = category,
                Attributes = attributes,
                StatusMessage = statusMessage,
                HasPermission = true,
                Columns = JsonConvert.SerializeObject(columns)
            };
        }

        public async Task<DataTableResult> GetDataTableResultAsync(
    DataTableRequest request,
    int categoryId,
    string userRole)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                return new DataTableResult
                {
                    Draw = request.Draw,
                    RecordsTotal = 0,
                    RecordsFiltered = 0,
                    Data = new List<DocumentRowDto>()
                };
            }

            var allowedCategoryIds = request.AllowedCategoryIds ?? new List<int>();

            var documents = await _documentRepository.GetFilteredDocumentsAsync(
                request.SearchValue,
                categoryId,
                allowedCategoryIds,
                request.YearFilter);

            // sorting (simple safe fallback)
            documents = ApplySorting(documents.AsQueryable(),
                request.SortColumn,
                request.SortDirection).ToList();

            var recordsTotal = documents.Count;

            var page = documents
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            // attributes
            var attributes = await _attributeRepository.GetByCategoryIdOrderedAsync(categoryId);

            if (category.PARENT_NAME.HasValue)
            {
                var parentAttrs = await _attributeRepository
                    .GetByCategoryIdOrderedAsync(category.PARENT_NAME.Value);

                attributes = attributes
                    .Concat(parentAttrs)
                    .OrderBy(a => a.COLM_ORDER)
                    .ToList();
            }

            var data = page.Select(doc => new DocumentRowDto
            {
                ID = doc.ID,
                NUMBEROF = doc.NUMBEROF ?? "",
                DATEOFDOC = doc.DATEOFDOC.HasValue &&
                    doc.DATEOFDOC.Value >= MinDate &&
                    doc.DATEOFDOC.Value <= MaxDate
                    ? doc.DATEOFDOC.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : "",

                DocAttribute = attributes
                .GroupBy(a => a.NAME)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var attr = g.FirstOrDefault();
                        if (attr == null) return "";

                        return doc.DOCATTRIBUTEs?
                            .FirstOrDefault(x => x.ATTRIBUTEID == attr.ID)?
                            .VALUEOF ?? "";
                    }
                ),

                ATTACHMENTS = doc.DocFile?.Count ?? 0,

                ACTIONS = BuildDocumentActionsHtml(doc.ID, userRole)

            }).ToList();

            return new DataTableResult
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsTotal,
                Data = data
            };
        }

        public async Task<Documenti> GetDocumentDetailsAsync(int id)
        {
            return await _documentRepository.GetDocumentWithAllDetailsAsync(id);
        }

        public async Task<Documenti> CreateDocumentOnlyAsync(Documenti document)
        {
            try
            {
                document.DATEIN = DateTime.Now;
                _context.Documenti.Add(document);
                await _context.SaveChangesAsync();

                return document;
            }
            catch
            {
                return document;
            }
        }

        public async Task<bool> CreateDocumentWithTempFilesAsync(
    Documenti document,
    string serverMapPath,
    string userId,
    List<string> tempFiles)
        {
            try
            {
                if (document == null)
                    return false;

                if (!document.Category.HasValue)
                    return false;

                if (!document.USERID.HasValue)
                    return false;

                document.DATEIN = DateTime.Now;

                if (!document.DATEOFDOC.HasValue)
                    document.DATEOFDOC = DateTime.Now;

                _context.Documenti.Add(document);
                await _context.SaveChangesAsync();

                if (tempFiles != null && tempFiles.Any())
                {
                    var category = await _categoryRepository.GetByIdAsync(document.Category.Value);
                    if (category == null)
                        return false;

                    var folderName = PathSecurityHelper.SanitizePathComponent(category.FOLDER_NAME ?? "default");

                    var fileDate = DateTime.Now;

                    var finalPath = Path.Combine(
                        serverMapPath,
                        "uploads",
                        folderName,
                        fileDate.Year.ToString("X"),
                        fileDate.Month.ToString("X"),
                        fileDate.Day.ToString("X")
                    );

                    Directory.CreateDirectory(finalPath);

                    var tempPath = Path.Combine(serverMapPath, "temp", userId);

                    foreach (var tempFile in tempFiles)
                    {
                        if (string.IsNullOrWhiteSpace(tempFile))
                            continue;

                        var extension = Path.GetExtension(tempFile).ToLowerInvariant();

                        if (!AllowedExtensions.Contains(extension))
                            continue;

                        var sourcePath = Path.Combine(tempPath, tempFile);

                        if (!File.Exists(sourcePath))
                            continue;

                        var safeFileName = PathSecurityHelper.SanitizePathComponent(tempFile);
                        var destinationPath = Path.Combine(finalPath, safeFileName);

                        if (File.Exists(destinationPath))
                        {
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(safeFileName);
                            var ext = Path.GetExtension(safeFileName);
                            safeFileName = nameWithoutExt + "_" + Guid.NewGuid().ToString("N") + ext;
                            destinationPath = Path.Combine(finalPath, safeFileName);
                        }
                        Encryption.EncryptFile(sourcePath, destinationPath);
                        File.Delete(sourcePath);

                        var fileEntity = new DocFile
                        {
                            NAME = safeFileName,
                            DATEOF = fileDate,
                            STATUS = 1,
                            DOCUMENTID = document.ID,
                            USERID = document.USERID.Value
                        };

                        _context.DocFile.Add(fileEntity);
                    }

                    await _context.SaveChangesAsync();

                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CreateDocumentWithTempFilesAsync ERROR:");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> CreateDocumentAsync(Documenti document, HttpPostedFileBase[] files, string serverMapPath, string pf)
        {
            try
            {
                document.DATEIN = DateTime.Now;
                _context.Documenti.Add(document);
                await _context.SaveChangesAsync();

                if (files != null && files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(file.FileName).ToLower();
                            if (AllowedExtensions.Contains(extension))
                            {
                                var category = await _categoryRepository.GetByIdAsync(document.Category.Value);
                                var fileDate = DateTime.Now;
                                var uploadPath = Path.Combine(serverMapPath, "uploads", category.FOLDER_NAME,
                                    fileDate.Year.ToString("X"),
                                    fileDate.Month.ToString("X"),
                                    fileDate.Day.ToString("X"));

                                Directory.CreateDirectory(uploadPath);
                                var fileName = Path.GetFileName(file.FileName);
                                var safeFileName = PathSecurityHelper.SanitizePathComponent(fileName);
                                var fullPath = Path.Combine(uploadPath, safeFileName);

                                var tempFilePath = Path.GetTempFileName();
                                file.SaveAs(tempFilePath);
                                Encryption.EncryptFile(tempFilePath, fullPath);
                                File.Delete(tempFilePath);

                                var fileEntity = new DocFile
                                {
                                    NAME = safeFileName,
                                    DATEOF = fileDate,
                                    STATUS = 1,
                                    DOCUMENTID = document.ID,
                                    USERID = document.USERID.Value
                                };

                                _context.DocFile.Add(fileEntity);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateDocumentAsync(
    Documenti document,
    HttpPostedFileBase[] files,
    string serverMapPath,
    string pf)
        {
            try
            {
                if (document == null)
                    return false;

                var existingDoc = await _context.Documenti
                    .Include(d => d.DOCATTRIBUTEs)
                    .Include(d => d.DocFile)
                    .FirstOrDefaultAsync(d => d.ID == document.ID);

                if (existingDoc == null)
                    return false;

                existingDoc.NUMBEROF = document.NUMBEROF;
                existingDoc.DATEOFDOC = document.DATEOFDOC;
                existingDoc.Category = document.Category;
                existingDoc.USERID = document.USERID;
                existingDoc.DATEIN = document.DATEIN == default(DateTime)
                    ? existingDoc.DATEIN
                    : document.DATEIN;

                if (document.DOCATTRIBUTEs != null)
                {
                    foreach (var postedAttr in document.DOCATTRIBUTEs)
                    {
                        if (!postedAttr.ATTRIBUTEID.HasValue)
                            continue;

                        var existingAttr = existingDoc.DOCATTRIBUTEs
                            .FirstOrDefault(x => x.ID == postedAttr.ID);

                        if (existingAttr == null)
                        {
                            existingAttr = existingDoc.DOCATTRIBUTEs
                                .FirstOrDefault(x => x.ATTRIBUTEID == postedAttr.ATTRIBUTEID);
                        }

                        if (existingAttr != null)
                        {
                            existingAttr.VALUEOF = postedAttr.VALUEOF ?? "";
                        }
                        else
                        {
                            _context.DocAttributes.Add(new DocAttributes
                            {
                                DOCUMENTID = existingDoc.ID,
                                ATTRIBUTEID = postedAttr.ATTRIBUTEID,
                                VALUEOF = postedAttr.VALUEOF ?? ""
                            });
                        }
                    }
                }

                if (files != null && files.Any(f => f != null && f.ContentLength > 0))
                {
                    var categoryId = existingDoc.Category ?? document.Category;

                    if (!categoryId.HasValue)
                        return false;

                    var category = await _categoryRepository.GetByIdAsync(categoryId.Value);

                    if (category == null)
                        return false;

                    var folderName = PathSecurityHelper.SanitizePathComponent(
                        category.FOLDER_NAME ?? "default");

                    var fileDate = DateTime.Now;

                    var uploadPath = Path.Combine(
                        serverMapPath,
                        "uploads",
                        folderName,
                        fileDate.Year.ToString("X"),
                        fileDate.Month.ToString("X"),
                        fileDate.Day.ToString("X")
                    );

                    Directory.CreateDirectory(uploadPath);

                    foreach (var file in files)
                    {
                        if (file == null || file.ContentLength <= 0)
                            continue;

                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                        if (!AllowedExtensions.Contains(extension))
                            continue;

                        var originalFileName = Path.GetFileName(file.FileName);
                        var safeFileName = PathSecurityHelper.SanitizePathComponent(originalFileName);
                        var fullPath = Path.Combine(uploadPath, safeFileName);

                        if (File.Exists(fullPath))
                        {
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(safeFileName);
                            var ext = Path.GetExtension(safeFileName);
                            safeFileName = nameWithoutExt + "_" + Guid.NewGuid().ToString("N") + ext;
                            fullPath = Path.Combine(uploadPath, safeFileName);
                        }

                        var tempFilePath = Path.GetTempFileName();
                        file.SaveAs(tempFilePath);
                        Encryption.EncryptFile(tempFilePath, fullPath);
                        File.Delete(tempFilePath);

                        _context.DocFile.Add(new DocFile
                        {
                            NAME = safeFileName,
                            DATEOF = fileDate,
                            STATUS = 1,
                            DOCUMENTID = existingDoc.ID,
                            USERID = existingDoc.USERID ?? document.USERID ?? 0
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateDocumentAsync ERROR:");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            try
            {
                var document = await _documentRepository.GetByIdAsync(id);
                if (document == null) return false;

                // Delete associated files from disk
                if (document.DocFile != null)
                {
                    foreach (var file in document.DocFile)
                    {
                        try
                        {
                            var category = await _categoryRepository.GetByIdAsync(document.Category.Value);
                            if (category != null)
                            {
                                var filePath = Path.Combine("uploads", category.FOLDER_NAME,
                                    file.DATEOF.Year.ToString("X"),
                                    file.DATEOF.Month.ToString("X"),
                                    file.DATEOF.Day.ToString("X"),
                                    file.NAME);
                                var fullPath = Path.Combine(HttpContext.Current.Server.MapPath("~"), filePath);
                                if (File.Exists(fullPath))
                                {
                                    File.Delete(fullPath);
                                }
                            }
                        }
                        catch
                        {
                            // Log error but continue
                        }
                    }
                }

                _documentRepository.Delete(document);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Documenti> GetDocumentForEditAsync(int id)
        {
            return await _documentRepository.GetDocumentWithAllDetailsAsync(id);
        }

        public async Task<string> GetDocumentFilePathAsync(Documenti document, DocFile file)
        {
            var category = await _categoryRepository.GetByIdAsync(document.Category.Value);
            var path = Path.Combine("uploads", category.FOLDER_NAME,
                file.DATEOF.Year.ToString("X"),
                file.DATEOF.Month.ToString("X"),
                file.DATEOF.Day.ToString("X"),
                file.NAME);
            return HttpContext.Current.Server.MapPath("~/" + path.Replace("\\", "/"));
        }

        public async Task DecryptFileAsync(string inputPath, string outputPath, string fileName)
        {
            // Placeholder - actual decryption logic will stay in controller or helper
            // This method can be extended based on your encryption implementation
        }

        public async Task<IEnumerable<Documenti>> SearchDocumentsAsync(string searchTerm, int categoryId)
        {
            return await _documentRepository.GetFilteredDocumentsAsync(searchTerm, categoryId, new List<int>());
        }

        public async Task<IEnumerable<Documenti>> GetRecentDocumentsAsync(int userId, int count)
        {
            return await _documentRepository
                .FindAsync(d => d.USERID == userId)
                .ContinueWith(t => t.Result.OrderByDescending(d => d.DATEIN).Take(count));
        }

        private IQueryable<Documenti> ApplySorting(IQueryable<Documenti> query, string sortColumn, string sortDirection)
        {
            if (string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortDirection))
            {
                return query.OrderBy(d => d.ID);
            }

            var allowedSortColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ID", "DATEIN", "DATEOFDOC", "NUMBEROF"
            };

            if (!allowedSortColumns.Contains(sortColumn))
            {
                sortColumn = "ID";
            }

            sortDirection = sortDirection?.ToLower() == "desc" ? "desc" : "asc";

            switch (sortColumn)
            {
                case "ID":
                    return sortDirection == "asc" ? query.OrderBy(d => d.ID) : query.OrderByDescending(d => d.ID);
                case "DATEIN":
                    return sortDirection == "asc" ? query.OrderBy(d => d.DATEIN) : query.OrderByDescending(d => d.DATEIN);
                case "DATEOFDOC":
                    return sortDirection == "asc"
                        ? query.OrderBy(d => d.DATEOFDOC ?? MinDate)
                        : query.OrderByDescending(d => d.DATEOFDOC ?? MinDate);
                case "NUMBEROF":
                    return sortDirection == "asc"
                        ? query.OrderBy(d => d.NUMBEROF.Length).ThenBy(d => d.NUMBEROF)
                        : query.OrderByDescending(d => d.NUMBEROF.Length).ThenByDescending(d => d.NUMBEROF);
                default:
                    return query.OrderBy(d => d.ID);
            }
        }

        //helpers
        private static string ToDataTablesAttributePath(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName))
                return "";

            return "DocAttribute." + attributeName
                .Replace("\\", "\\\\")
                .Replace(".", "\\.");
        }

        private static string BuildDocumentActionsHtml(int id, string userRole)
        {
            var role = (userRole ?? "").ToLowerInvariant();

            var linkDetails = $"<a href=\"{VirtualPathUtility.ToAbsolute("~/Documenti/Details")}?ID={id}\">استعراض</a>";

            if (role == "admin" || role == "manager")
            {
                var linkEdit = $"<a href=\"{VirtualPathUtility.ToAbsolute("~/Documenti/Edit")}?ID={id}\">تعديل</a>";
                var linkDelete = $"<a href=\"{VirtualPathUtility.ToAbsolute("~/Documenti/Delete")}?ID={id}\">حذف</a>";
                var linkSend = $"<a href=\"{VirtualPathUtility.ToAbsolute("~/Messaging/Create")}?ID={id}\">إرسال</a>";

                return linkEdit + " | " + linkDetails + " | " + linkDelete + " | " + linkSend;
            }

            if (role == "reader")
            {
                return linkDetails;
            }

            return "";
        }
    }
}
