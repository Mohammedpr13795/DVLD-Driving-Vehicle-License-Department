using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DVLD_Buisness
{
    public class clsCryptoHelper
    {
        // Constants for Symmetric Encryption
        private const int _SymmetricKeySize = 16; // 128 bits
        private const int _SymmetricBlockSize = 16; // 128 bits (AES block size)

        // Hashing Method
        public static string ComputeHashPassword(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input), "Input cannot be null or empty");

            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                    StringBuilder stringBuilder = new StringBuilder();

                    foreach (byte b in hashBytes)
                    {
                        stringBuilder.Append(b.ToString("x2"));
                    }

                    return stringBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                clsEventLogger.LogEvent($"Error computing hash: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        // Symmetric Encryption Methods
        public static string SymmetricEncrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText), "Plain Text cannot be null or empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Key cannot be null or empty");

            byte[] keyBytes = _PrepareSymmetricKey(key);
            byte[] iv = _GenerateRandomIV();

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 128;
                    aesAlg.Key = keyBytes;
                    aesAlg.IV = iv;

                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(iv, 0, iv.Length); // Prepend IV
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                clsEventLogger.LogEvent($"Symmetric Encryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (CryptographicException ex)
            {
                clsEventLogger.LogEvent($"Symmetric Encryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (Exception ex)
            {
                clsEventLogger.LogEvent($"Unexpected error in SymmetricEncrypt: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public static string SymmetricDecrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText), "Cipher Text cannot be null or empty");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Key cannot be null or empty");

            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            if (cipherBytes.Length < _SymmetricBlockSize)
                throw new ArgumentException("Cipher Text is too short to contain IV", nameof(cipherText));

            byte[] iv = new byte[_SymmetricBlockSize];
            Array.Copy(cipherBytes, 0, iv, 0, _SymmetricBlockSize);
            byte[] encryptedData = new byte[cipherBytes.Length - _SymmetricBlockSize];
            Array.Copy(cipherBytes, _SymmetricBlockSize, encryptedData, 0, encryptedData.Length);
            byte[] keyBytes = _PrepareSymmetricKey(key);

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.KeySize = 128;
                    aesAlg.Key = keyBytes;
                    aesAlg.IV = iv;

                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                clsEventLogger.LogEvent($"Symmetric Decryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (FormatException ex)
            {
                clsEventLogger.LogEvent($"Invalid Base64 format in Cipher Text: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (CryptographicException ex)
            {
                clsEventLogger.LogEvent($"Symmetric Decryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (ArgumentException ex)
            {
                clsEventLogger.LogEvent($"Symmetric Decryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (Exception ex)
            {
                clsEventLogger.LogEvent($"Unexpected error in SymmetricDecrypt: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        private static byte[] _PrepareSymmetricKey(string key)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                Array.Resize(ref keyBytes, _SymmetricKeySize); // Ensure key is exactly 16 bytes
                return keyBytes;
            }
            catch (Exception ex)
            {
                clsEventLogger.LogEvent($"Error preparing Symmetric Key: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        private static byte[] _GenerateRandomIV()
        {
            try
            {
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    byte[] iv = new byte[_SymmetricBlockSize];
                    rng.GetBytes(iv);
                    return iv;
                }
            }
            catch (Exception ex)
            {
                clsEventLogger.LogEvent($"Error generating IV: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        // Asymmetric Encryption Methods
        public static (string PublicKey, string PrivateKey) GenerateAsymmetricKeyPair()
        {
            try
            {
                using (RSA rsa = RSA.Create(2048)) // 2048-bit key for enhanced security
                {
                    string publicKey = rsa.ToXmlString(false); // Only public parameters
                    string privateKey = rsa.ToXmlString(true); // Public + private parameters
                    return (publicKey, privateKey);
                }
            }
            catch (CryptographicException ex)
            {
                clsEventLogger.LogEvent($"Failed to generate Asymmetric Key Pair: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (Exception ex)
            {
                clsEventLogger.LogEvent($"Unexpected error in GenerateAsymmetricKeyPair: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public static string AsymmetricEncrypt(string plainText, string publicKey)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText), "Plain Text cannot be null or empty");
            if (string.IsNullOrEmpty(publicKey))
                throw new ArgumentNullException(nameof(publicKey), "Public Key cannot be null or empty");

            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(publicKey);
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.Pkcs1);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
            catch (ArgumentNullException ex)
            {
                clsEventLogger.LogEvent($"Asymmetric Encryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (CryptographicException ex)
            {
                clsEventLogger.LogEvent($"Asymmetric Encryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (Exception ex)
            {
                clsEventLogger.LogEvent($"Unexpected error in AsymmetricEncrypt: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public static string AsymmetricDecrypt(string cipherText, string privateKey)
        {
            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException(nameof(cipherText), "Cipher Text cannot be null or empty");
            if (string.IsNullOrEmpty(privateKey))
                throw new ArgumentNullException(nameof(privateKey), "Private Key cannot be null or empty");

            try
            {
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(privateKey);
                    byte[] cipherBytes = Convert.FromBase64String(cipherText);
                    byte[] decryptedBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
            catch (ArgumentNullException ex)
            {
                clsEventLogger.LogEvent($"Asymmetric Decryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (FormatException ex)
            {
                clsEventLogger.LogEvent($"Invalid Base64 format in Cipher Text: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (CryptographicException ex)
            {
                clsEventLogger.LogEvent($"Asymmetric Decryption failed: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
            catch (Exception ex)
            {
                clsEventLogger.LogEvent($"Unexpected error in AsymmetricDecrypt: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }
    }
}