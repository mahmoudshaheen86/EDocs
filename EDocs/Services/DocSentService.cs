using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace edocs.Services
{
    public class DocSentService : IDocSentService
    {
        private readonly IDocSentRepository _docSentRepository;
        private readonly IUserCategoryRepository _userCategoryRepository;
        private readonly ApplicationDbContext _context;

        public DocSentService(
            IDocSentRepository docSentRepository,
            IUserCategoryRepository userCategoryRepository,
            ApplicationDbContext context)
        {
            _docSentRepository = docSentRepository;
            _userCategoryRepository = userCategoryRepository;
            _context = context;
        }

        public async Task<IEnumerable<DocMessage>> GetUserMessagesAsync(int userId)
        {
            return await _docSentRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<DocMessage>> GetSentMessagesAsync(int userId)
        {
            var result = await _docSentRepository.GetAllIncludingAsync(d => d.DocumentiFk);

            return result
                .Where(d => d.USERSENDID == userId)
                .OrderByDescending(d => d.SENDDATE);
        }

        public async Task<IEnumerable<DocMessage>> GetReceivedMessagesAsync(int userId)
        {
            var result = await _docSentRepository.GetAllIncludingAsync(d => d.DocumentiFk);

            return result
                .Where(d => d.USERECIVEID == userId)
                .OrderByDescending(d => d.SENDDATE);
        }

        public async Task<IEnumerable<DocMessage>> GetByDocumentIdAsync(int documentId)
        {
            var result = await _docSentRepository.GetAllIncludingAsync(d => d.DocumentiFk);

            return result
                .Where(d => d.Documenti == documentId)
                .OrderByDescending(d => d.SENDDATE);
        }

        public async Task<DocMessage> GetMessageAsync(int id)
        {
            var result = await _docSentRepository.GetAllIncludingAsync(d => d.DocumentiFk);

            return result.FirstOrDefault(d => d.ID == id);
        }

        public async Task<DocMessage> CreateMessageAsync(DocMessage message)
        {
            message.SENDDATE = DateTime.Now;
            await _docSentRepository.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task UpdateMessageAsync(DocMessage message)
        {
            _docSentRepository.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMessageAsync(int id)
        {
            var message = await _docSentRepository.GetByIdAsync(id);
            if (message != null)
            {
                _docSentRepository.Delete(message);
                await _context.SaveChangesAsync();
            }
        }

        public async Task BroadcastMessageAsync(string usersCsv, string note, int documentId, int senderId)
        {
            var usernames = usersCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var allUsers = await _context.Users.ToListAsync();

            foreach (var username in usernames)
            {
                var user = allUsers.FirstOrDefault(u => u.UserName == username.Trim());
                if (user != null)
                {
                    var message = new DocMessage
                    {
                        SENDNOTES = note,
                        SENDDATE = DateTime.Now,
                        USERSENDID = senderId,
                        USERECIVEID = user.Id,
                        Documenti = documentId
                    };

                    await _docSentRepository.AddAsync(message);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<DocMessage>> SearchByDocumentNumberAsync(string number, int currentUserId)
        {
            // Get document first
            var document = await _context.Documenti.FirstOrDefaultAsync(d => d.NUMBEROF == number);
            if (document == null) return new List<DocMessage>();

            return await _docSentRepository.GetByDocumentIdAsync(document.ID);
        }

        public async Task<DocMessage> GetFirstSentMessageAsync(int userId)
        {
            var result = await _docSentRepository.GetAllIncludingAsync(d => d.DocumentiFk);

            return result.FirstOrDefault(d => d.USERSENDID == userId);
        }

        public async Task<ApplicationUser> GetUserAsync(int userId)
        {
            return _context.Users.Find(userId);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        async Task<bool> IDocSentService.DeleteMessageAsync(int id)
        {
            var message = await _docSentRepository.GetByIdAsync(id);
            if (message != null)
            {
                try
                {
                    _docSentRepository.Delete(message);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
            else { return false; }
        }
    }
}
