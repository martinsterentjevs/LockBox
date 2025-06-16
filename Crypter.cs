using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace LockBox
{
    internal class Crypter
    {
        public static async Task<string> EncryptAsync(string plaintext) //uses built in AES functions to encrypt the data (uses aes-cbc)
        {
            var key = await CredMan.GetPasswordAsync();
            if (key.Length != 16 && key.Length != 24 && key.Length != 32)
            {
                throw new CryptographicException("Specified key is not a valid size for this algorithm." + "test" + key);
            }

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using var writer = new StreamWriter(cryptoStream);
                writer.Write(plaintext);
            }

            var iv = aes.IV;
            var encrypted = ms.ToArray();

            var result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

            return Convert.ToBase64String(result); //the encrypted data is returned as a base64 string
        }

        public static async Task<string> DecryptAsync(string encryptedText) //Uses built in AES functions to decrypt the data
        {
            var key = await CredMan.GetPasswordAsync();
            var fullCipher = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = key;

            // Extract IV
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);
            return await reader.ReadToEndAsync();
        }
    }
}
