using edocs.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services.Description;
using edocs.ViewModels;

namespace edocs.Services
{
    public interface IDocumentService
    {
        Task<DocumentIndexViewModel> GetIndexViewModelAsync(int categoryId, edocs.helper.StatusMessage.MessageId? message, string userRole);
        Task<DataTableResult> GetDataTableResultAsync(DataTableRequest request, int categoryId, string userRole);
        Task<Documenti> GetDocumentDetailsAsync(int id);
        Task<Documenti> CreateDocumentOnlyAsync(Documenti document);
        Task<bool> CreateDocumentWithTempFilesAsync(
            Documenti document,
            string serverMapPath,
            string userId,
            List<string> tempFiles);
        Task<bool> CreateDocumentAsync(Documenti document, HttpPostedFileBase[] files, string serverMapPath, string pf);
        Task<bool> UpdateDocumentAsync(Documenti document, HttpPostedFileBase[] files, string serverMapPath, string pf);
        Task<bool> DeleteDocumentAsync(int id);
        Task<Documenti> GetDocumentForEditAsync(int id);
        Task<string> GetDocumentFilePathAsync(Documenti document, DocFile file);
        Task DecryptFileAsync(string inputPath, string outputPath, string fileName);
        Task<IEnumerable<Documenti>> SearchDocumentsAsync(string searchTerm, int categoryId);
        Task<IEnumerable<Documenti>> GetRecentDocumentsAsync(int userId, int count);
    }

    public class DataTableRequest
    {
        public int? YearFilter { get; set; }
        public string SearchValue { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string ColumnSearch { get; set; }
        public List<int> AllowedCategoryIds { get; set; }
        public int Draw { get; set; }
    }

    public class DataTableResult
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<DocumentRowDto> Data { get; set; }
        public string Mode { get; set; }
    }
}
