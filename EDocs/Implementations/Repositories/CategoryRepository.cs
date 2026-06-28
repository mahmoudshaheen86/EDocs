using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Implementations.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.PARENT_NAME == null)
                .OrderBy(c => c.NAME)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId)
        {
            return await _dbSet
                .Where(c => c.PARENT_NAME == parentId)
                .OrderBy(c => c.NAME)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryWithAttributesAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Attributes)
                .FirstOrDefaultAsync(c => c.ID == id);
        }

        public async Task<bool> HasChildrenAsync(int categoryId)
        {
            return await _dbSet.AnyAsync(c => c.PARENT_NAME == categoryId);
        }
    }
}
