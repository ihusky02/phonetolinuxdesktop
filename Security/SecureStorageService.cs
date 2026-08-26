using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhoneToLinux.Security
{
    /// <summary>
    /// Service responsible for encrypting and decrypting local sensitive data using AES-256.
    /// </summary>
    public class SecureStorageService
    {
        private readonly byte[] _masterKey;

        public SecureStorageService(byte[] masterKey)
        {
            if (masterKey == null || masterKey.Length != 32)
            {
                throw new ArgumentException("Master key must be exactly 256 bits (32 bytes).", nameof(masterKey));
            }
            _masterKey = masterKey;
        }

        /// <summary>
        /// Encrypts plain text data and writes it to the specified file path along with the generated Initialization Vector (IV).
        /// </summary>
        public void EncryptAndWrite(string targetFilePath, string plainTextData)
        {
            using var aes = Aes.Create();
            aes.Key = _masterKey;
            aes.GenerateIV();

            string? directory = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            // Write IV at the beginning of the file (unencrypted, required for decryption)
            fileStream.Write(aes.IV, 0, aes.IV.Length);

            using var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var writer = new StreamWriter(cryptoStream, Encoding.UTF8);
            writer.Write(plainTextData);
        }

        /// <summary>
        /// Reads an encrypted file, extracts the IV, and decrypts the content back to plain text.
        /// </summary>
        public string ReadAndDecrypt(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("Encrypted data file was not found.", sourceFilePath);
            }

            using var aes = Aes.Create();
            aes.Key = _masterKey;

            using var fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] iv = new byte[aes.BlockSize / 8];
            int bytesRead = fileStream.Read(iv, 0, iv.Length);
            
            if (bytesRead != iv.Length)
            {
                throw new InvalidOperationException("Invalid encrypted file structure (missing IV).");
            }

            aes.IV = iv;

            using var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}