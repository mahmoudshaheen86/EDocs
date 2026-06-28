using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Services
{
    public class DocAttributeService : IDocAttributeService
    {
        private readonly IDocAttributeRepository _docAttributeRepository;
        private readonly ApplicationDbContext _context;

        public DocAttributeService(IDocAttributeRepository docAttributeRepository, ApplicationDbContext context)
        {
            _docAttributeRepository = docAttributeRepository;
            _context = context;
        }

        public async Task<IEnumerable<DocAttributes>> GetByDocumentAsync(int documentId)
        {
            return await _docAttributeRepository.GetByDocumentIdAsync(documentId);
        }

        public async Task<DocAttributes> CreateAsync(DocAttributes docAttribute)
        {
            await _docAttributeRepository.AddAsync(docAttribute);
            await _context.SaveChangesAsync();
            return docAttribute;
        }

        public async Task<bool> UpdateAsync(DocAttributes docAttribute)
        {
            _docAttributeRepository.Update(docAttribute);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var docAttr = await _docAttributeRepository.GetByIdAsync(id);
            if (docAttr == null) return false;

            _docAttributeRepository.Delete(docAttr);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
