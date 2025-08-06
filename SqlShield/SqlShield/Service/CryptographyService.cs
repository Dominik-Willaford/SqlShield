using Microsoft.Extensions.Options;
using SqlShield.Interface;
using SqlShield.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Service
{
    // The class no longer needs to be IDisposable as we will manage resources within each method.
    internal class CryptographyService : ICryptography
    {
        // ... (Constants and constructor are unchanged)
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int Pbkdf2Iterations = 100000;

        private readonly string _masterKey;

        public CryptographyService(IOptions<SqlShieldSettings> settings)
        {
            _masterKey = settings.Value.CryptoKey;
            if (string.IsNullOrEmpty(_masterKey))
            {
                throw new ArgumentNullException(nameof(_masterKey), "CryptoKey cannot be null or empty in appsettings.json.");
            }
        }


        public string EncryptData(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext))
                return string.Empty;

            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            using var rfc2898 = new Rfc2898DeriveBytes(_masterKey, nonce, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            byte[] key = rfc2898.GetBytes(KeySize);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using (var aes = new AesGcm(key, TagSize))
            {
                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            byte[] encryptedPayload = new byte[NonceSize + TagSize + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, encryptedPayload, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, encryptedPayload, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, encryptedPayload, NonceSize + TagSize, ciphertext.Length);

            return Convert.ToBase64String(encryptedPayload);
        }

        public string DecryptData(string encryptedPayloadBase64)
        {
            if (string.IsNullOrEmpty(encryptedPayloadBase64))
                return string.Empty;

            byte[] encryptedPayload = Convert.FromBase64String(encryptedPayloadBase64);

            if (encryptedPayload.Length < NonceSize + TagSize)
            {
                throw new CryptographicException("Invalid encrypted payload.");
            }

            Span<byte> nonce = encryptedPayload.AsSpan(0, NonceSize);
            Span<byte> tag = encryptedPayload.AsSpan(NonceSize, TagSize);
            Span<byte> ciphertext = encryptedPayload.AsSpan(NonceSize + TagSize);

            using var rfc2898 = new Rfc2898DeriveBytes(_masterKey, nonce.ToArray(), Pbkdf2Iterations, HashAlgorithmName.SHA256);
            byte[] key = rfc2898.GetBytes(KeySize);

            byte[] plaintextBytes = new byte[ciphertext.Length];

            // *** CORRECTED IMPLEMENTATION ***
            // 1. Create an INSTANCE with the derived key.
            using (var aes = new AesGcm(key, TagSize))
            {
                // 2. Call the DECRYPT method on the INSTANCE ('aes').
                aes.Decrypt(nonce, ciphertext, tag, plaintextBytes);
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        // --- Other methods remain unchanged ---
        // ... (GenerateHash, CompareInputHash, BuildConnString)
        #region Other Methods
        public string GenerateHash(HashAlgorithm hashAlgorithm, string input)
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public bool CompareInputHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            var hashOfInput = GenerateHash(hashAlgorithm, input);
            return StringComparer.OrdinalIgnoreCase.Equals(hashOfInput, hash);
        }

        public string BuildConnString(string connString, string pass)
        {
            string decryptPass = DecryptData(pass);
            return $"{connString};Password={decryptPass}";
        }
        #endregion
    }
}
