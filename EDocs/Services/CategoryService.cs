using edocs.Implementations.Repositories;
using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Services
{
    public class CategoryService : Repository<Category>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserCategoryRepository _userCategoryRepository;
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context) : base(context)
        {
        }

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUserCategoryRepository userCategoryRepository,
            ApplicationDbContext context) : base(context)
        {
            _categoryRepository = categoryRepository;
            _userCategoryRepository = userCategoryRepository;
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository
                .GetAllIncludingAsync(c => c.ParentCategory);
        }

        /*public async Task<Category> GetCategoryAsync(int id)
        {
            return await _categoryRepository.FirstOrDefaultIncludingAsync(
                c => c.ID == id,
                c => c.ParentCategory,
                c => c.Attributes
            );
        }*/
        public async Task<Category> GetCategoryAsync(int id)
        {
            return await _context.Category
                .Include(c => c.ParentCategory)
                .Include(c => c.Attributes.Select(a => a.AttributeList))
                .FirstOrDefaultAsync(c => c.ID == id);
        }


        public async Task<Category> CreateCategoryAsync(Category category)
        {
            await _categoryRepository.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            _categoryRepository.Update(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            _categoryRepository.Delete(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasChildrenAsync(int categoryId)
        {
            return await _categoryRepository.HasChildrenAsync(categoryId);
        }

        public async Task<string> BuildCategoryTreeAsync(int? selectedCategoryId = null)
        {
            var allCategories = await _categoryRepository.GetAllAsync();
            var sb = new System.Text.StringBuilder();

            foreach (var category in allCategories.Where(c => c.PARENT_NAME == null))
            {
                BuildCategoryTreeRecursive(allCategories.ToList(), category, sb, 0, selectedCategoryId);
            }

            return sb.ToString();
        }

        private void BuildCategoryTreeRecursive(List<Category> allCategories, Category category, System.Text.StringBuilder sb, int level, int? selectedId)
        {
            var indent = new string('─', level * 2);
            var selected = category.ID == selectedId ? "selected" : "";
            sb.AppendLine($"<option value=\"{category.ID}\" {selected}>{indent} {category.NAME}</option>");

            var children = allCategories.Where(c => c.PARENT_NAME == category.ID);
            foreach (var child in children)
            {
                BuildCategoryTreeRecursive(allCategories, child, sb, level + 1, selectedId);
            }
        }
    }
}
