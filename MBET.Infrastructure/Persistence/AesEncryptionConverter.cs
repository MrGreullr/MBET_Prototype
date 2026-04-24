using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MBET.Infrastructure.Persistence
{
    public class AesEncryptionConverter : ValueConverter<string, string>
    {
        public AesEncryptionConverter(string key)
            : base(
                v => Encrypt(v, key),
                v => Decrypt(v, key),
                new ConverterMappingHints(size: 256))
        {
        }

        private static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using var aes = Aes.Create();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            // Ensure key is 32 bytes (256 bits)
            Array.Resize(ref keyBytes, 32);
            aes.Key = keyBytes;

            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            using var ms = new MemoryStream();
            // Prepend IV to the stream so we can read it back for decryption
            ms.Write(iv, 0, iv.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                var keyBytes = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref keyBytes, 32);
                aes.Key = keyBytes;

                // Extract IV (first 16 bytes)
                var iv = new byte[16];
                Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // The rest is the actual cipher
                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(fullCipher, 16, fullCipher.Length - 16);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
            catch (FormatException)
            {
                // Not Base64? Return as-is (Legacy Plain Text support)
                return cipherText;
            }
            catch (CryptographicException)
            {
                // Decryption failed? Return raw value (Safety fallback)
                return cipherText;
            }
        }
    }
}