namespace GatewayService.BLL.Security;

public interface IPasswordDecryptor
{
    string Decrypt(string encryptedPassword);
}
