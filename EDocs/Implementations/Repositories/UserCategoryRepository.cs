using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Implementations.Repositories
{
    public class UserCategoryRepository : Repository<UserCategory>, IUserCategoryRepository
    {
        public UserCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<UserCategory> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(uc => uc.CategoryFK)
                .FirstOrDefaultAsync(uc => uc.ID == id);
        }

        public async Task<UserCategory> GetByUserAndCategoryAsync(int userId, int categoryId)
        {
            return await _dbSet
                .Include(uc => uc.CategoryFK)
                .FirstOrDefaultAsync(uc => uc.USERID == userId && uc.CATEGORYID == categoryId);
        }

        public async Task<IEnumerable<UserCategory>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(uc => uc.USERID == userId)
                .Include(uc => uc.CategoryFK)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(uc => uc.USERID == userId)
                .Select(uc => uc.CategoryFK)
                .Distinct()
                .OrderBy(c => c.NAME)
                .ToListAsync();
        }

        async Task<UserCategory> IUserCategoryRepository.GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(uc => uc.CategoryFK)
                .FirstOrDefaultAsync(uc => uc.ID == id);
        }
    }
}
