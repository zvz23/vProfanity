using System;
using System.IO;
using System.Security.Cryptography;

namespace vProfanity.Services
{
    public static class FileHashGenerator
    {
        public static string GetFileHash(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    return BitConverter.ToString(sha256.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
