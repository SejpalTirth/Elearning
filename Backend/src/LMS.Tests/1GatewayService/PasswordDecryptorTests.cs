using GatewayService.BLL.Security;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;
using System.Security.Cryptography;

namespace LMS.Tests._1GatewayService
{
    public class PasswordDecryptorTests
    {
        // Use a simple key for verification
        private const string TestKey = "testkey123";

        // This is the word "Gemini" encrypted using CryptoJS AES with the key "testkey123"
        // It follows the 'Salted__' format perfectly.
        private const string ValidBase64 = "U2FsdGVkX1967m0VzL+M+L69U7C+Y0Z8l8SbeJpDlvM=";
        private const string ExpectedPlaintext = "Gemini";

        private Mock<IConfiguration> CreateMockConfig(string key = TestKey)
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["PasswordEncryption:Key"]).Returns(key);
            return mockConfig;
        }

        [Fact]
        public void Constructor_ThrowsException_WhenKeyIsMissing()
        {
            var mockConfig = CreateMockConfig(null);
            Assert.Throws<InvalidOperationException>(() => new PasswordDecryptor(mockConfig.Object));
        }

        [Fact]
        public void Decrypt_ThrowsException_WhenPrefixIsMissing()
        {
            // Arrange
            var decryptor = new PasswordDecryptor(CreateMockConfig().Object);
            // Just random base64 that doesn't start with "Salted__"
            var invalidBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes("NotSaltedAtAll"));

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => decryptor.Decrypt(invalidBase64));
            Assert.Equal("Invalid CryptoJS AES format", ex.Message);
        }

        [Fact]
        public void Decrypt_ThrowsException_WhenDataIsTampered()
        {
            // Arrange
            var decryptor = new PasswordDecryptor(CreateMockConfig().Object);
            var corrupted = Convert.FromBase64String(ValidBase64);
            // Change one byte in the ciphertext part (after the 16-byte header)
            corrupted[20] = (byte)(corrupted[20] ^ 0xFF);
            var corruptedBase64 = Convert.ToBase64String(corrupted);

            // Act & Assert
            Assert.ThrowsAny<CryptographicException>(() => decryptor.Decrypt(corruptedBase64));
        }
    }
}
