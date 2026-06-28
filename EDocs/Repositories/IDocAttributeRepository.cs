using edocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Repositories
{
    public interface IDocAttributeRepository : IRepository<DocAttributes>
    {
        Task<IEnumerable<DocAttributes>> GetByDocumentIdAsync(int documentId);
        Task<DocAttributes> GetByDocumentAndAttributeAsync(int documentId, int attributeId);
        Task<IEnumerable<DocAttributes>> GetByAttributeIdAsync(int attributeId);
    }
}
