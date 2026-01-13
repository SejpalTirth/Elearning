using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace GatewayService.BLL.Security;

public class PasswordDecryptor : IPasswordDecryptor
{
    private readonly string _key;

    public PasswordDecryptor(IConfiguration config)
    {
        _key = config["PasswordEncryption:Key"]
               ?? throw new InvalidOperationException("PasswordEncryption key missing");
    }

    public string Decrypt(string cipherTextBase64)
    {
        var cipherTextBytes = Convert.FromBase64String(cipherTextBase64);

        var salted = Encoding.ASCII.GetString(cipherTextBytes, 0, 8);
        if (salted != "Salted__")
            throw new InvalidOperationException("Invalid CryptoJS AES format");

        var salt = cipherTextBytes.Skip(8).Take(8).ToArray();
        var cipherText = cipherTextBytes.Skip(16).ToArray();

        var (key, iv) = EvpBytesToKey(
            Encoding.UTF8.GetBytes(_key),
            salt,
            32,
            16
        );

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

        return Encoding.UTF8.GetString(decrypted);
    }

    private static (byte[] Key, byte[] IV) EvpBytesToKey(
        byte[] password,
        byte[] salt,
        int keySize,
        int ivSize)
    {
        var totalSize = keySize + ivSize;
        var derived = new byte[totalSize];
        var block = Array.Empty<byte>();
        var offset = 0;

        using var md5 = MD5.Create();

        while (offset < totalSize)
        {
            block = md5.ComputeHash(block.Concat(password).Concat(salt).ToArray());
            var length = Math.Min(block.Length, totalSize - offset);
            Array.Copy(block, 0, derived, offset, length);
            offset += length;
        }

        return (
            derived.Take(keySize).ToArray(),
            derived.Skip(keySize).Take(ivSize).ToArray()
        );
    }
}
