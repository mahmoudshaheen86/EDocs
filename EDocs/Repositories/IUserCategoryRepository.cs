using edocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Repositories
{
    public interface IUserCategoryRepository : IRepository<UserCategory>
    {
        Task<UserCategory> GetByUserAndCategoryAsync(int userId, int categoryId);
        Task<UserCategory> GetByIdAsync(int id);
        
        Task<IEnumerable<UserCategory>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Category>> GetCategoriesByUserIdAsync(int userId);
    }
}
