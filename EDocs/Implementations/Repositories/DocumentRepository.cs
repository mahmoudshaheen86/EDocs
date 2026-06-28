using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Implementations.Repositories
{
    public class DocumentRepository : Repository<Documenti>, IDocumentRepository
    {
        private static readonly DateTime MinDate = new DateTime(1900, 4, 30);
        private static readonly DateTime MaxDate = new DateTime(2077, 11, 16);
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Documenti>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet.Where(d => d.Category == categoryId)
                .Include(d => d.CategoryFk)
                .Include(d => d.USERFk)
                .ToListAsync();
        }

        public async Task<IEnumerable<Documenti>> GetByUserIdAsync(int userId)
        {
            return await _dbSet.Where(d => d.USERID == userId)
                .Include(d => d.CategoryFk)
                .Include(d => d.USERFk)
                .ToListAsync();
        }

        public async Task<IEnumerable<Documenti>> GetDocumentsWithDetailsAsync(int categoryId, List<int> allowedCategoryIds)
        {
            var query = _dbSet.Include(d => d.CategoryFk)
                .Include(d => d.USERFk)
                .Include(d => d.DOCATTRIBUTEs.Select(da => da.AttributeFk))
                .Include(d => d.DocFile);

            if (allowedCategoryIds != null && allowedCategoryIds.Count > 0)
            {
                query = query.Where(d => d.Category == categoryId || 
                    (d.CategoryFk.PARENT_NAME == categoryId && allowedCategoryIds.Contains(d.CategoryFk.ID)));
            }
            else
            {
                query = query.Where(d => d.Category == categoryId);
            }

            return await query.ToListAsync();
        }

        public async Task<Documenti> GetDocumentWithAllDetailsAsync(int id)
        {
            return await _dbSet
                .Include(d => d.CategoryFk)
                .Include(d => d.CategoryFk.ParentCategory)
                .Include(d => d.USERFk)
                .Include(d => d.DOCATTRIBUTEs.Select(da => da.AttributeFk))
                .Include(d => d.DocFile)
                .Include(d => d.DOCSENTS)
                .FirstOrDefaultAsync(d => d.ID == id);
        }

        public async Task<List<Documenti>> GetFilteredDocumentsAsync(
    string searchValue,
    int categoryId,
    List<int> allowedCategoryIds,
    int? year = null)
        {
            var query = _dbSet
                .Include(d => d.CategoryFk)
                .Include(d => d.USERFk)
                .Include(d => d.DOCATTRIBUTEs.Select(da => da.AttributeFk))
                .Include(d => d.DocFile);

            // Category filter
            if (allowedCategoryIds != null && allowedCategoryIds.Count > 0)
            {
                query = query.Where(d =>
                    d.Category == categoryId ||
                    (d.CategoryFk.PARENT_NAME == categoryId &&
                     allowedCategoryIds.Contains(d.CategoryFk.ID)));
            }
            else
            {
                query = query.Where(d => d.Category == categoryId);
            }

            // Year filter
            query = query.Where(d =>
                !d.DATEOFDOC.HasValue ||
                (d.DATEOFDOC.Value >= MinDate &&
                 d.DATEOFDOC.Value <= MaxDate));

            if (year.HasValue)
            {
                query = query.Where(d =>
                    d.DATEOFDOC.HasValue &&
                    d.DATEOFDOC.Value.Year == year.Value);
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                searchValue = searchValue.ToLower();

                query = query.Where(d =>
                    (d.NUMBEROF != null && d.NUMBEROF.ToLower().Contains(searchValue)) ||
                    d.DOCATTRIBUTEs.Any(da =>
                        da.VALUEOF != null &&
                        da.VALUEOF.ToLower().Contains(searchValue)));
            }

            var documents = await query
            .AsNoTracking()
            .ToListAsync();

            return documents;
        }

    }
}
