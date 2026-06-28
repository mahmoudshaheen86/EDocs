using edocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Repositories
{
    public interface IAttributeRepository : IRepository<DocAttribute>
    {
        Task<IEnumerable<DocAttribute>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<DocAttribute>> GetByCategoryIdOrderedAsync(int categoryId);
        Task<IEnumerable<DocAttribute>> GetByCategoryOrderedAsync(Category category);
    }
}
