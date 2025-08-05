using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SqlShield.Interface
{
    public interface ICryptography
    {
        string GenerateHash(HashAlgorithm hashAlgorithm, string input);
        bool CompareInputHash(HashAlgorithm hashAlgorithm, string input, string hash);
        string EncryptData(string plaintext);
        string DecryptData(string encryptedtext);
        string BuildConnString(string connString, string pass);
    }
}
