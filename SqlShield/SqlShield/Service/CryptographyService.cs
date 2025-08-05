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
    internal class CryptographyService : ICryptography, IDisposable
    {
        private TripleDES _tripleDES = TripleDES.Create();
        bool _disposed = false;
        private byte[] TruncateHash(string key, int length)
        {
            SHA1 sha1 = SHA1.Create();

            // Hash the key.
            byte[] keyBytes = System.Text.Encoding.Unicode.GetBytes(key);
            byte[] hash = sha1.ComputeHash(keyBytes);
            var oldHash = hash;
            hash = new byte[length - 1 + 1];

            // Truncate or pad the hash.
            if (oldHash != null)
                Array.Copy(oldHash, hash, Math.Min(length - 1 + 1, oldHash.Length));
            return hash;
        }

        public CryptographyService(IOptions<SqlShieldSettings> settings)
        {
            string cryptoKey = settings.Value.CryptoKey;
            if (string.IsNullOrEmpty(cryptoKey))
            {
                throw new ArgumentNullException(nameof(cryptoKey), "CryptoKey cannot be null or empty. Check your appsettings.json.");
            }

            // The rest of your constructor logic is perfect.
            _tripleDES.Key = TruncateHash(cryptoKey, _tripleDES.KeySize / 8);
            _tripleDES.IV = TruncateHash("", _tripleDES.BlockSize / 8);
        }

        public string GenerateHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public bool CompareInputHash(HashAlgorithm hashAlgorithm, string input, string hash)
        {
            // Hash the input.
            var hashOfInput = GenerateHash(hashAlgorithm, input);
            // Create a StringComparer an compare the hashes.
            var comparer = StringComparer.Ordinal;
            return comparer.Compare(hashOfInput, hash) == 0;
        }

        public string EncryptData(string plaintext)
        {

            // Convert the plaintext string to a byte array.
            byte[] plaintextBytes = System.Text.Encoding.Unicode.GetBytes(plaintext);

            // Create the stream.
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            // Create the encoder to write to the stream.
            CryptoStream encStream = new CryptoStream(ms, _tripleDES.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write);

            // Use the crypto stream to write the byte array to the stream.
            encStream.Write(plaintextBytes, 0, plaintextBytes.Length);
            encStream.FlushFinalBlock();

            // Convert the encrypted stream to a printable string.
            return Convert.ToBase64String(ms.ToArray());
        }

        public string DecryptData(string encryptedtext)
        {
            if (string.IsNullOrEmpty(encryptedtext))
                return string.Empty;

            // This handles cases where the base64 string might have been URL-encoded
            // and spaces replaced plus signs.
            byte[] encryptedBytes = Convert.FromBase64String(encryptedtext.Replace(" ", "+"));

            using (var ms = new MemoryStream())
            using (var decStream = new CryptoStream(ms, _tripleDES.CreateDecryptor(), CryptoStreamMode.Write))
            {
                decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                decStream.FlushFinalBlock();
                return Encoding.Unicode.GetString(ms.ToArray());
            }
        }

        public string BuildConnString(string connString, string pass)
        {
            string decryptPass = DecryptData(pass);
            return String.Format(connString, decryptPass);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _tripleDES?.Dispose();
                _disposed = true;
            }
        }
    }
}
