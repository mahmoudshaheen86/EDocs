using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using static iTextSharp.tool.xml.html.HTML;
using Category = edocs.Models.Category;

namespace edocs.Services
{
    public interface ICategoryService : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> HasChildrenAsync(int categoryId);
        Task<string> BuildCategoryTreeAsync(int? selectedCategoryId = null);
    }

    public interface IAttributeService
    {
        Task<IEnumerable<DocAttribute>> GetAttributesAsync();
        Task<IEnumerable<DocAttribute>> GetAttributesByCategoryAsync(int categoryId);
        Task<IEnumerable<DocAttribute>> GetAttributesWithListsAsync(int categoryId);
        Task<DocAttribute> CreateAttributeAsync(DocAttribute attribute);
        Task<bool> UpdateAttributeAsync(DocAttribute attribute);
        Task<bool> DeleteAttributeAsync(int id);
        Task<DocAttribute> GetAttributeAsync(int id);
        Task<bool> IsAttributeUsedAsync(int attributeId);
    }

    public interface IAttributeListService
    {
        Task<IEnumerable<AttributeList>> GetValuesByAttributeAsync(int attributeId);
        Task<AttributeList> CreateValueAsync(AttributeList value);
        Task<bool> UpdateValueAsync(AttributeList value);
        Task<bool> DeleteValueAsync(int id);
    }

    public interface IDocAttributeService
    {
        Task<IEnumerable<DocAttributes>> GetByDocumentAsync(int documentId);
        Task<DocAttributes> CreateAsync(DocAttributes docAttribute);
        Task<bool> UpdateAsync(DocAttributes docAttribute);
        Task<bool> DeleteAsync(int id);
    }

    public interface IFileService
    {
        Task<IEnumerable<DocFile>> GetFilesByDocumentAsync(int documentId);
        Task<IEnumerable<DocFile>> GetFilesByUserAsync(int userId);
        Task<DocFile> GetFileAsync(int id);
        Task<DocFile> CreateFileAsync(DocFile file, HttpPostedFileBase uploadedFile, string serverMapPath);
        Task UpdateFileAsync(DocFile file);
        Task<bool> DeleteFileAsync(int id);
    }

    public interface IDocSentService
    {
        Task<IEnumerable<DocMessage>> GetUserMessagesAsync(int userId);
        Task<IEnumerable<DocMessage>> GetSentMessagesAsync(int userId);
        Task<IEnumerable<DocMessage>> GetReceivedMessagesAsync(int userId);
        Task<IEnumerable<DocMessage>> GetByDocumentIdAsync(int documentId);
        Task<DocMessage> GetMessageAsync(int id);
        Task<DocMessage> CreateMessageAsync(DocMessage message);
        Task<bool> DeleteMessageAsync(int id);
        Task UpdateMessageAsync(DocMessage message);
        Task BroadcastMessageAsync(string usersCsv, string note, int documentId, int senderId);
        Task<IEnumerable<DocMessage>> SearchByDocumentNumberAsync(string number, int currentUserId);
        Task<DocMessage> GetFirstSentMessageAsync(int userId);
        Task<ApplicationUser> GetUserAsync(int userId);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    }

    public interface IUserCategoryService
    {
        Task<UserCategory> GetUserCategoryAsync(int userId, int categoryId);
        Task<IEnumerable<UserCategory>> GetUserCategoriesAsync(int userId);
        Task<IEnumerable<UserCategory>> GetUsersCategoriesAsync();
        Task<IEnumerable<Category>> GetUserAllowedCategoriesAsync(int userId);
        Task<List<int>> GetAllowedCategoryIdsAsync(int userId);
        Task<UserCategory> CreateAsync(UserCategory userCategory);
        Task<bool> UpdateAsync(UserCategory userCategory);
        Task<bool> DeleteAsync(int id);
        Task<string> GetUserRoleInCategoryAsync(int userId, int categoryId);
        Task<UserCategory> GetByIdAsync(int value);
        Task<ApplicationUser> GetUserAsync(int userId);
    }
}
