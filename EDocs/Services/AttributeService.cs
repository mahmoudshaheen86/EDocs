using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Services
{
    public class AttributeService : IAttributeService
    {
        private readonly IAttributeRepository _attributeRepository;
        private readonly IAttributeListRepository _attributeListRepository;
        private readonly ApplicationDbContext _context;

        public AttributeService(
            IAttributeRepository attributeRepository,
            IAttributeListRepository attributeListRepository,
            ApplicationDbContext context)
        {
            _attributeRepository = attributeRepository;
            _attributeListRepository = attributeListRepository;
            _context = context;
        }
        public async Task<IEnumerable<DocAttribute>> GetAttributesAsync()
        {
            return await _attributeRepository.GetAllAsync();
        }
        public async Task<IEnumerable<DocAttribute>> GetAttributesByCategoryAsync(int categoryId)
        {
            return await _attributeRepository.GetByCategoryIdOrderedAsync(categoryId);
        }

        public async Task<IEnumerable<DocAttribute>> GetAttributesWithListsAsync(int categoryId)
        {
            var attributes = await _attributeRepository.GetByCategoryIdOrderedAsync(categoryId);
            foreach (var attr in attributes)
            {
                if (attr.TYPE == "قائمة")
                {
                    attr.AttributeList = await _attributeListRepository.GetByAttributeIdAsync(attr.ID).ContinueWith(t => t.Result.ToList());
                }
            }
            return attributes;
        }

        public async Task<DocAttribute> GetAttributeAsync(int id)
        {
            return await _attributeRepository.GetByIdAsync(id);
        }

        public async Task<DocAttribute> CreateAttributeAsync(DocAttribute attribute)
        {
            await _attributeRepository.AddAsync(attribute);
            await _context.SaveChangesAsync();
            return attribute;
        }

        public async Task<bool> UpdateAttributeAsync(DocAttribute attribute)
        {
            _attributeRepository.Update(attribute);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAttributeAsync(int id)
        {
            var attribute = await _attributeRepository.GetByIdAsync(id);
            if (attribute == null) return false;

            // Delete associated list values first
            var listValues = await _attributeListRepository.GetByAttributeIdAsync(id);
            foreach (var value in listValues)
            {
                _attributeListRepository.Delete(value);
            }

            _attributeRepository.Delete(attribute);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsAttributeUsedAsync(int attributeId)
        {
            return await _context.DocAttributes
                .AnyAsync(x => x.ATTRIBUTEID == attributeId);
        }
    }
}
