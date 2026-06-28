using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Implementations.Repositories
{
    public class AttributeListRepository : Repository<AttributeList>, IAttributeListRepository
    {
        public AttributeListRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AttributeList>> GetByAttributeIdAsync(int attributeId)
        {
            return await _dbSet
                .Where(al => al.ATTRID == attributeId)
                .OrderBy(al => al.ID)
                .ToListAsync();
        }
    }
}
