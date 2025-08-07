using Microsoft.Extensions.Options;
using SqlShield.Model;
using System.Security.Cryptography;
using SqlShield.Service;

namespace SqlShield.Tests
{
    public class CryptographyServiceTests
    {
        // helper method to generate a random key for each run
        private string GenerateRandomKey(int size = 32)
        {
            // Generates a cryptographically secure random byte array
            var randomBytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            // Convert the bytes to a Base64 string to ensure it's a valid string key
            return Convert.ToBase64String(randomBytes);
        }

        private CryptographyService CreateService(string key, int iterations = 100000)
        {
            // We just create the service directly with the values we need for the test.
            return new CryptographyService(key, iterations);
        }

        [Fact]
        public void EncryptThenDecryptShouldReturnOriginalData()
        {
            // ARRANGE
            var service = CreateService(GenerateRandomKey());
            string originalData = "This is a secret message.";

            // ACT
            string encryptedData = service.EncryptData(originalData);
            string decryptedData = service.DecryptData(encryptedData);

            // ASSERT
            Assert.NotNull(encryptedData);
            Assert.NotEqual(originalData, encryptedData); // Ensure it's actually encrypted
            Assert.Equal(originalData, decryptedData);    // Ensure it decrypts back correctly
        }

        [Fact]
        public void DecryptWithWrongKeyShouldThrowCryptographicException()
        {
            string correctKey = GenerateRandomKey();
            string wrongKey = GenerateRandomKey();
            // To ensure the keys are never the same
            while (correctKey == wrongKey)
            {
                wrongKey = GenerateRandomKey();
            }
            // ARRANGE
            var correctKeyService = CreateService(correctKey);
            var wrongKeyService = CreateService(wrongKey);
            string originalData = "some data";

            // ACT
            string encryptedData = correctKeyService.EncryptData(originalData);

            // ASSERT
            // Verify that trying to decrypt with the wrong service/key throws the expected exception
            Assert.Throws<AuthenticationTagMismatchException>(() => wrongKeyService.DecryptData(encryptedData));
        }

        [Fact]
        public void DecryptTamperedDataShouldThrowCryptographicException()
        {
            // ARRANGE
            var service = CreateService(GenerateRandomKey());
            string originalData = "important data";
            string encryptedData = service.EncryptData(originalData);

            var tamperedChars = encryptedData.ToCharArray();
            int indexToTamper = 1;
            char originalChar = tamperedChars[indexToTamper];

            // Change the character to something else. This logic ensures it's always different.
            tamperedChars[indexToTamper] = (originalChar == 'A' ? 'B' : 'A');

            string tamperedData = new string(tamperedChars);

            // ACT & ASSERT
            // The HMAC check should fail, resulting in an exception
            Assert.Throws<AuthenticationTagMismatchException>(() => service.DecryptData(tamperedData));
        }

        [Fact]
        public void ConstructorWithNullKeyShouldThrowArgumentNullException()
        {
            // ARRANGE
            string nullKey = null;

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => new CryptographyService(nullKey, 10000));
        }
    }
}