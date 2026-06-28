using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using edocs.Models;

namespace edocs.helper
{
    public static class Encryption
    {
        private const int IvSize = 16;

        private static byte[] GetEncryptionKey()
        {
            string base64Key = ConfigurationManager.AppSettings["EncryptionKey"];

            if (string.IsNullOrWhiteSpace(base64Key))
                throw new InvalidOperationException("EncryptionKey is missing from Web.config appSettings.");

            byte[] key = Convert.FromBase64String(base64Key);

            if (key.Length != 32)
                throw new InvalidOperationException("EncryptionKey must be a Base64 encoded 32-byte key for AES-256.");

            return key;
        }

        public static void EncryptFile(string inputFile, string outputFile)
        {
            byte[] key = GetEncryptionKey();

            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    // Save IV at the beginning of the encrypted file
                    outputStream.Write(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(
                        outputStream,
                        aes.CreateEncryptor(),
                        CryptoStreamMode.Write))
                    {
                        inputStream.CopyTo(cryptoStream);
                    }
                }
            }
        }

        public static void DecryptFile(DocFile file, Documenti document, string serverMapPath)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.CategoryFk == null)
                throw new InvalidOperationException("Document category is not loaded.");

            var category = document.CategoryFk;

            var uploadPath = Path.Combine(
                serverMapPath,
                "uploads",
                category.FOLDER_NAME,
                file.DATEOF.Year.ToString("X"),
                file.DATEOF.Month.ToString("X"),
                file.DATEOF.Day.ToString("X")
            );

            var inputPath = Path.Combine(uploadPath, file.NAME);

            var outputPath = Path.Combine(serverMapPath, "temp");
            Directory.CreateDirectory(outputPath);

            var outputFilePath = Path.Combine(outputPath, file.NAME);

            DecryptFile(inputPath, outputFilePath);
        }

        public static void DecryptFile(string inputFile, string outputFile)
        {
            byte[] key = GetEncryptionKey();

            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

            using (var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                byte[] iv = new byte[IvSize];

                int read = inputStream.Read(iv, 0, iv.Length);
                if (read != IvSize)
                    throw new InvalidOperationException("Invalid encrypted file. IV is missing.");

                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    using (var cryptoStream = new CryptoStream(
                        inputStream,
                        aes.CreateDecryptor(),
                        CryptoStreamMode.Read))
                    {
                        cryptoStream.CopyTo(outputStream);
                    }
                }
            }
        }
    }
}