using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using IOCore.Libs;

namespace IOApp.Features
{
    public class CryptographyUtils
    {
        public static readonly byte[] SALT = Encoding.UTF8.GetBytes("275010a649c4d5690f10dc49b9418456");
        public static readonly int ITERATIONS = 2048;

        private static Aes GetAes(string password, byte[] salt, int iterations)
        {
            var key = new Rfc2898DeriveBytes(password, salt, iterations);

            var aes = Aes.Create();
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = key.GetBytes(aes.BlockSize / 8);
            aes.Mode = CipherMode.CBC;

            return aes;
        }

        public static string Encrypt(string data, string password, byte[] salt, int iterations)
        {
            try
            {
                return Convert.ToBase64String(EncryptToBytes(data, password, salt, iterations));
            }
            catch
            {
                throw;
            }
        }

        public static byte[] EncryptToBytes(string data, string password, byte[] salt, int iterations)
        {
            try
            {
                var aes = GetAes(password, salt, iterations);
                var transform = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, transform, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(data);
                }

                return memoryStream.ToArray();
            }
            catch
            {
                throw;
            }
        }

        public static string Decrypt(string data, string password, byte[] salt, int iterations)
        {
            try
            {
                return Decrypt(Convert.FromBase64String(data), password, salt, iterations);
            }
            catch
            {
                throw;
            }
        }

        public static string Decrypt(byte[] buffer, string password, byte[] salt, int iterations)
        {
            try
            {
                var aes = GetAes(password, salt, iterations);
                var transform = aes.CreateDecryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new(buffer);
                using CryptoStream cryptoStream = new(memoryStream, transform, CryptoStreamMode.Read);
                using StreamReader streamReader = new(cryptoStream);
                return streamReader.ReadToEnd();
            }
            catch (CryptographicException ex)
            {
                if (ex.Message == "Padding is invalid and cannot be removed.")
                    throw new ApplicationException("Universal Microsoft Cryptographic Exception (Not to be believed!)", ex);
                else
                    throw;
            }
            catch
            {
                throw;
            }
        }

        public static void EncryptFile(string inputFilePath, string outputFilePath, string password, byte[] salt, int iterations)
        {
            var aes = GetAes(password, salt, iterations);
            var transform = aes.CreateEncryptor(aes.Key, aes.IV);

            try
            {
                using var outputFileStream = new FileStream(outputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                using var cryptoStream = new CryptoStream(outputFileStream, transform, CryptoStreamMode.Write);
                using var inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                inputFileStream.CopyTo(cryptoStream);
            }
            catch
            {
                Utils.DeleteFileOrDirectory(outputFilePath);
                throw;
            }
        }

        public static void DecryptFile(string inputFilePath, string outputFilePath, string password, byte[] salt, int iterations)
        {
            var aes = GetAes(password, salt, iterations);
            var transform = aes.CreateDecryptor(aes.Key, aes.IV);

            try
            {
                using var outputFileStream = new FileStream(outputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                using var cryptoStream = new CryptoStream(outputFileStream, transform, CryptoStreamMode.Write);
                using var inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                inputFileStream.CopyTo(cryptoStream);
            }
            catch (CryptographicException ex)
            {
                Utils.DeleteFileOrDirectory(outputFilePath);

                if (ex.Message == "Padding is invalid and cannot be removed.")
                    throw new ApplicationException("Universal Microsoft Cryptographic Exception (Not to be believed!)", ex);
                else
                    throw;
            }
            catch
            {
                Utils.DeleteFileOrDirectory(outputFilePath);
                throw;
            }
        }
    }
}