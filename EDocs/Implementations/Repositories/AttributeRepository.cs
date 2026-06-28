using ADO.NET.Helper;
using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Implementations.Repositories
{
    public class AttributeRepository : Repository<DocAttribute>, IAttributeRepository
    {
        public AttributeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocAttribute>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Where(a => a.Category == categoryId)
                .Include(a => a.CategoryFk)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocAttribute>> GetByCategoryIdOrderedAsync(int categoryId)
        {
            return await _dbSet
                .Where(a => a.Category == categoryId)
                .OrderBy(a => a.COLM_ORDER)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocAttribute>> GetByCategoryOrderedAsync(Category category)
        {
            return await _dbSet
                .Where(a => a.CategoryFk.ID == category.ID || a.CategoryFk.ID == category.PARENT_NAME)
                .OrderBy(a => a.COLM_ORDER)
                .ToListAsync();
        }
    }
}
