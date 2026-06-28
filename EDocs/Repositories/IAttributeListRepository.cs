using edocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Repositories
{
    public interface IAttributeListRepository : IRepository<AttributeList>
    {
        Task<IEnumerable<AttributeList>> GetByAttributeIdAsync(int attributeId);
    }
}
