using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Implementations.Repositories
{
    public class FileRepository : Repository<DocFile>, IFileRepository
    {
        public FileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocFile>> GetByDocumentIdAsync(int documentId)
        {
            return await _dbSet
                .Where(f => f.DOCUMENTID == documentId)
                .OrderBy(f => f.ID)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocFile>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(f => f.USERID == userId)
                .Include(f => f.DOCFk)
                .OrderByDescending(f => f.DATEOF)
                .ToListAsync();
        }

        public async Task<DocFile> GetFileAsync(int id)
        {
            return await _dbSet
                .Include(f => f.DOCFk)
                .Include(f => f.USERFk)
                .FirstOrDefaultAsync(f => f.ID == id);
        }
    }
}
