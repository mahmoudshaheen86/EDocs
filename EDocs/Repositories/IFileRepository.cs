using edocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Repositories
{
    public interface IFileRepository : IRepository<DocFile>
    {
        Task<IEnumerable<DocFile>> GetByDocumentIdAsync(int documentId);
        Task<IEnumerable<DocFile>> GetByUserIdAsync(int userId);
        Task<DocFile> GetFileAsync(int id);
    }
}
