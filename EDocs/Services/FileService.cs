using edocs.helper;
using edocs.Models;
using edocs.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace edocs.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;
        private readonly ApplicationDbContext _context;

        public FileService(IFileRepository fileRepository, ApplicationDbContext context)
        {
            _fileRepository = fileRepository;
            _context = context;
        }

        public async Task<IEnumerable<DocFile>> GetFilesByDocumentAsync(int documentId)
        {
            return await _fileRepository.GetByDocumentIdAsync(documentId);
        }

        public async Task<IEnumerable<DocFile>> GetFilesByUserAsync(int userId)
        {
            return await _fileRepository.GetByUserIdAsync(userId);
        }

        public async Task<DocFile> GetFileAsync(int id)
        {
            return await _fileRepository.GetFileAsync(id);
        }

        public async Task<DocFile> CreateFileAsync(DocFile file, HttpPostedFileBase uploadedFile, string serverMapPath)
        {
            if (uploadedFile != null && uploadedFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(uploadedFile.FileName);
                var safeFileName = PathSecurityHelper.SanitizePathComponent(fileName);
                
                //save
                var uploadPath = serverMapPath;
                Directory.CreateDirectory(uploadPath);
                var fullPath = Path.Combine(uploadPath, safeFileName);
                uploadedFile.SaveAs(fullPath);

                file.NAME = safeFileName;
                file.DATEOF = DateTime.Now;
                file.STATUS = 1;

                await _fileRepository.AddAsync(file);
                await _context.SaveChangesAsync();
            }

            return file;
        }

        public async Task UpdateFileAsync(DocFile file)
        {
            _fileRepository.Update(file);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteFileAsync(int id)
        {
            var file = await _fileRepository.GetByIdAsync(id);
            if (file == null) return false;

            _fileRepository.Delete(file);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
