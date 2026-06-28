using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Implementations.Repositories
{
    public class DocAttributeRepository : Repository<DocAttributes>, IDocAttributeRepository
    {
        public DocAttributeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocAttributes>> GetByDocumentIdAsync(int documentId)
        {
            return await _dbSet
                .Where(da => da.DOCUMENTID == documentId)
                .Include(da => da.AttributeFk)
                .OrderBy(da => da.AttributeFk.COLM_ORDER)
                .ToListAsync();
        }

        public async Task<DocAttributes> GetByDocumentAndAttributeAsync(int documentId, int attributeId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(da => da.DOCUMENTID == documentId && da.ATTRIBUTEID == attributeId);
        }

        public async Task<IEnumerable<DocAttributes>> GetByAttributeIdAsync(int attributeId)
        {
            return await _dbSet
                .Where(da => da.ATTRIBUTEID == attributeId)
                .ToListAsync();
        }
    }
}
