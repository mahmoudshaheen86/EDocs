using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Implementations.Repositories
{
    public class DocSentRepository : Repository<DocMessage>, IDocSentRepository
    {
        public DocSentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DocMessage>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(ds => ds.USERSENDID == userId || ds.USERECIVEID == userId)
                .Include(ds => ds.DocumentiFk)
                .OrderByDescending(ds => ds.SENDDATE)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocMessage>> GetSentByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(ds => ds.USERSENDID == userId)
                .Include(ds => ds.DocumentiFk)
                .OrderByDescending(ds => ds.SENDDATE)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocMessage>> GetReceivedByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(ds => ds.USERECIVEID == userId)
                .Include(ds => ds.DocumentiFk)
                .OrderByDescending(ds => ds.SENDDATE)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocMessage>> GetByDocumentIdAsync(int documentId)
        {
            return await _dbSet
                .Where(ds => ds.Documenti == documentId)
                .Include(ds => ds.DocumentiFk)
                .OrderByDescending(ds => ds.SENDDATE)
                .ToListAsync();
        }
    }
}
