using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Services
{
    public class AttributeListService : IAttributeListService
    {
        private readonly IAttributeListRepository _attributeListRepository;
        private readonly ApplicationDbContext _context;

        public AttributeListService(IAttributeListRepository attributeListRepository, ApplicationDbContext context)
        {
            _attributeListRepository = attributeListRepository;
            _context = context;
        }

        public async Task<IEnumerable<AttributeList>> GetValuesByAttributeAsync(int attributeId)
        {
            return await _attributeListRepository.GetByAttributeIdAsync(attributeId);
        }

        public async Task<AttributeList> CreateValueAsync(AttributeList value)
        {
            await _attributeListRepository.AddAsync(value);
            await _context.SaveChangesAsync();
            return value;
        }

        public async Task<bool> UpdateValueAsync(AttributeList value)
        {
            _attributeListRepository.Update(value);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteValueAsync(int id)
        {
            var value = await _attributeListRepository.GetByIdAsync(id);
            if (value == null) return false;

            _attributeListRepository.Delete(value);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
