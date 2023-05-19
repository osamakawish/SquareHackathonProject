using System.Security.Cryptography;
using System.Text;

namespace Tester;

public class Program
{
    public static string Encrypt(string plainText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.GenerateIV();
        byte[] encryptedBytes;

        using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
        {
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt)) swEncrypt.Write(plainText);
            encryptedBytes = msEncrypt.ToArray();
        }

        var combinedBytes = new byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combinedBytes, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, combinedBytes, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(combinedBytes);
    }

    public static string Decrypt(string combinedText, string key)
    {
        var combinedBytes = Convert.FromBase64String(combinedText);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);

        var iv = new byte[aes.BlockSize / 8]; // BlockSize is in bits, we need it in bytes here
        var encryptedBytes = new byte[combinedBytes.Length - iv.Length];

        Buffer.BlockCopy(src: combinedBytes, srcOffset: 0, dst: iv, dstOffset: 0, count: iv.Length);
        Buffer.BlockCopy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(encryptedBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }


    public static void Main()
    {
        var key = new byte[16];
        RandomNumberGenerator.Fill(key);
        var keyAsString = Convert.ToBase64String(key);
        Console.WriteLine($"Random key: {keyAsString}");

        var encrypted = Encrypt("\"Hello, World!\"", keyAsString);
        Console.WriteLine($"\"Hello, World!\" encrypted: {encrypted}");
        Console.WriteLine($"\"Hello, World!\" decrypted: {Decrypt(encrypted, keyAsString)}");

        Console.ReadKey();
    }
}