using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YourNamespace.Helpers
{
    public static class PinEncryptionHelper
    {
        private static byte[] Key;
        private static byte[] IV;
        private static bool Initialized = false;

        /// <summary>
        /// Call this once at app startup
        /// </summary>
        public static void Init(IConfiguration configuration)
        {
            string encryptionKey = configuration["Security:PinEncryptionKey"] ?? "DEFAULT_KEY_123";

            // Derive Key and IV exactly like your Web App
            using var pdb = new Rfc2898DeriveBytes(encryptionKey, Encoding.ASCII.GetBytes("SALT1234"));
            Key = pdb.GetBytes(32); // 256-bit key
            IV = pdb.GetBytes(16);  // 128-bit IV

            Initialized = true;
        }

        /// <summary>
        /// Encrypt PIN (optional, for testing)
        /// </summary>
        public static string EncryptPin(string pinNumber)
        {
            if (string.IsNullOrWhiteSpace(pinNumber))
                return pinNumber;

            if (!Initialized)
                throw new Exception("PinEncryptionHelper.Init() was not called!");

            byte[] clearBytes = Encoding.UTF8.GetBytes(pinNumber);

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearBytes, 0, clearBytes.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Decrypt PIN from Web App DB
        /// </summary>
        public static string DecryptPin(string encryptedPin)
        {
            if (string.IsNullOrWhiteSpace(encryptedPin))
                return encryptedPin;

            if (!Initialized)
                throw new Exception("PinEncryptionHelper.Init() was not called!");

            byte[] cipherBytes = Convert.FromBase64String(encryptedPin);

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
