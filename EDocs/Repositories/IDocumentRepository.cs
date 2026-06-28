using edocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Repositories
{
    public interface IDocumentRepository : IRepository<Documenti>
    {
        Task<IEnumerable<Documenti>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Documenti>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Documenti>> GetDocumentsWithDetailsAsync(int categoryId, List<int> allowedCategoryIds);
        Task<Documenti> GetDocumentWithAllDetailsAsync(int id);
        Task<List<Documenti>> GetFilteredDocumentsAsync(string searchValue, int categoryId, List<int> allowedCategoryIds, int? year = null);
    }
}
