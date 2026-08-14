using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace phonetolinux.ViewModels
{
    public static class DebugSecurityManager
    {
        private static readonly string CdpFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_config.cdp");
        // Sól używana do wyprowadzenia klucza szyfrującego AES (musi być typu byte[])
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("PhonetoLinuxDebugSaltKey2026");

        public static bool CdpFileExists => File.Exists(CdpFilePath);

        public static bool ValidatePasswordStrength(string password, out string errorMessage)
        {
            if (string.IsNullOrEmpty(password) || password.Length != 12)
            {
                errorMessage = "Hasło musi mieć dokładnie 12 znaków.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                errorMessage = "Hasło musi zawierać przynajmniej jedną małą literę.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                errorMessage = "Hasło musi zawierać przynajmniej jedną dużą literę.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                errorMessage = "Hasło musi zawierać przynajmniej jedną cyfrę.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public static void SavePasswordToCdp(string password)
        {
            byte[] encryptedData = EncryptPassword(password, "PhonetoLinuxSecretKey2026");
            File.WriteAllBytes(CdpFilePath, encryptedData);
        }

        public static bool VerifyPassword(string inputPassword)
        {
            try
            {
                if (!CdpFileExists) return false;
                byte[] encryptedData = File.ReadAllBytes(CdpFilePath);
                string decryptedPassword = DecryptPassword(encryptedData, "PhonetoLinuxSecretKey2026");
                return decryptedPassword == inputPassword;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] EncryptPassword(string plainText, string passphrase)
        {
            using var aes = Aes.Create();
            // Poprawiono kolejność argumentów oraz wielkość liter w SHA256
            var key = new Rfc2898DeriveBytes(passphrase, Salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.GenerateIV(); // Poprawiono nazwę metody generującej wektor inicjalizacyjny

            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }

        private static string DecryptPassword(byte[] cipherBytes, string passphrase)
        {
            using var aes = Aes.Create();
            // Poprawiono wielkość liter w SHA256
            var key = new Rfc2898DeriveBytes(passphrase, Salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);

            byte[] iv = new byte[16];
            Array.Copy(cipherBytes, 0, iv, 0, 16);
            aes.IV = iv;

            using var ms = new MemoryStream(cipherBytes, 16, cipherBytes.Length - 16);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }
}