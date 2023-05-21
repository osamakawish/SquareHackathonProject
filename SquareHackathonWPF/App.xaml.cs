using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Square;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using static System.Net.WebRequestMethods;
using Environment = System.Environment;

namespace SquareHackathonWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    static App()
    {
        void SetEnvironmentVariable(string key, string value)
        {
            if (Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User) == null)
                Environment.SetEnvironmentVariable(key, value, EnvironmentVariableTarget.User);
        }
        SetEnvironmentVariable("ok_app_0", "8iyMdUgH+ql0SNkIJx8ARg==");
        SetEnvironmentVariable("ok_app_1",
            "PCtt6gWUboOgYWUpJhS9wJprRr5w/kvSGTRmTaTmIaVmY8Cp9b7a0lzqNXptK0yMqLvNdHkt0XaZvkHALgh4" +
            "fnO2cBhUbPG6IfXH/I9Fbi3HVsDWlIYBtZujQl+OEtYl");
        SetEnvironmentVariable("ok_app_2", "RJYtiBxAZYcTMqXD3VUiJYuZcFLF7XizlTwcyRUnt5uzZ9sc0LvVv" +
                                           "VIVvQW54xHbsOpgI7c6D9YtD/lyU91Nhw==");
        SetEnvironmentVariable("ok_app_3", "TE4jC4Uxnmx0XVLwF46sdy+5L/8hQLt/beDo5EdreUkV2OeCGcWB9" +
                                           "Pev3M/OkyzcHn2U5clmczZvf9N4XHMnGQ==");
        
        SquareAccessToken = Environment.GetEnvironmentVariable("ok_app_1", EnvironmentVariableTarget.User)!;
        SquareAccessToken = Decrypt(SquareAccessToken, Environment.GetEnvironmentVariable("ok_app_0", EnvironmentVariableTarget.User)!);

        OpenExchangeRatesAppId = Environment.GetEnvironmentVariable("ok_app_2", EnvironmentVariableTarget.User)!;
        OpenExchangeRatesAppId = Decrypt(OpenExchangeRatesAppId, Environment.GetEnvironmentVariable("ok_app_0", EnvironmentVariableTarget.User)!);

        AzureKey1 = Environment.GetEnvironmentVariable("ok_app_3", EnvironmentVariableTarget.User)!;
        AzureKey1 = Decrypt(AzureKey1, Environment.GetEnvironmentVariable("ok_app_0", EnvironmentVariableTarget.User)!);
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

    private static string GetToken(string subpath)
    {
        var doc = new XmlDocument();
        doc.Load(@"..\..\..\..\secrets.xml");

        var tokenNode = doc.SelectSingleNode($"/secrets/{subpath}");
        return tokenNode!.InnerText;
    }

    internal static string SquareAccessToken { get; }

    internal static string OpenExchangeRatesAppId { get; }

    internal static string AzureKey1 { get; }
   
    // Modify code in the future to choose between sandbox and production modes.
    internal static SquareClient Client { get; } = new SquareClient.Builder()
        .Environment(Square.Environment.Sandbox)
        .AccessToken(SquareAccessToken)
        .Build();
}