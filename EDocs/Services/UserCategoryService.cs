using edocs.Implementations.Repositories;
using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Services
{
    public class UserCategoryService : IUserCategoryService
    {
        private readonly IUserCategoryRepository _userCategoryRepository;
        private readonly ApplicationDbContext _context;

        public UserCategoryService(IUserCategoryRepository userCategoryRepository, ApplicationDbContext context)
        {
            _userCategoryRepository = userCategoryRepository;
            _context = context;
        }

        public async Task<UserCategory> GetUserCategoryAsync(int userId, int categoryId)
        {
            return await _userCategoryRepository.GetByUserAndCategoryAsync(userId, categoryId);
        }

        public async Task<IEnumerable<UserCategory>> GetUserCategoriesAsync(int userId)
        {
            return await _userCategoryRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<UserCategory>> GetUsersCategoriesAsync()
        {
            return await _userCategoryRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Category>> GetUserAllowedCategoriesAsync(int userId)
        {
            return await _userCategoryRepository.GetCategoriesByUserIdAsync(userId);
        }

        public async Task<List<int>> GetAllowedCategoryIdsAsync(int userId)
        {
            var userCategories = await _userCategoryRepository.GetByUserIdAsync(userId);
            return userCategories
                    .Where(uc => uc.CATEGORYID.HasValue)
                    .Select(uc => uc.CATEGORYID.Value)
                    .ToList();
        }

        public async Task<UserCategory> CreateAsync(UserCategory userCategory)
        {
            await _userCategoryRepository.AddAsync(userCategory);
            await _context.SaveChangesAsync();
            return userCategory;
        }

        public async Task<bool> UpdateAsync(UserCategory userCategory)
        {
            _userCategoryRepository.Update(userCategory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var userCategory = await _userCategoryRepository.GetByIdAsync(id);
            if (userCategory == null) return false;

            _userCategoryRepository.Delete(userCategory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetUserRoleInCategoryAsync(int userId, int categoryId)
        {
            var userCategory = await _userCategoryRepository.GetByUserAndCategoryAsync(userId, categoryId);
            return userCategory?.TYPE;
        }

        async Task<UserCategory> IUserCategoryService.GetByIdAsync(int id)
        {
            return await _userCategoryRepository.GetByIdAsync(id);
        }
        public async Task<ApplicationUser> GetUserAsync(int userId)
        {
            return _context.Users.Find(userId);
        }
    }
}
