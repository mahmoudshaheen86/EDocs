using edocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Repositories
{
    public interface IDocSentRepository : IRepository<DocMessage>
    {
        Task<IEnumerable<DocMessage>> GetByUserIdAsync(int userId);
        Task<IEnumerable<DocMessage>> GetSentByUserIdAsync(int userId);
        Task<IEnumerable<DocMessage>> GetReceivedByUserIdAsync(int userId);
        Task<IEnumerable<DocMessage>> GetByDocumentIdAsync(int documentId);
    }
}
