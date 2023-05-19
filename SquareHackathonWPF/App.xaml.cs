using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Square;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace SquareHackathonWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private static string GetSecret(string secretName)
    {
        var keyVaultUrl = "https://square-hackathon-ok.vault.azure.net/";
        var client = new SecretClient(vaultUri: new(keyVaultUrl), credential: new DefaultAzureCredential());


    }

    private static string GetToken(string subpath)
    {
        var doc = new XmlDocument();
        doc.Load(@"..\..\..\..\secrets.xml");

        var tokenNode = doc.SelectSingleNode($"/secrets/{subpath}");
        return tokenNode!.InnerText;
    }

    internal static string SquareAccessToken { get; } = GetToken("square_access_token");

    internal static string OpenExchangeRatesAppId { get; } = GetToken("open_exchange_rates_app_id");

    internal static string AzureKey1 { get; } = GetToken("azure_key_1");
   
    // Modify code in the future to choose between sandbox and production modes.
    internal static SquareClient Client { get; } = new SquareClient.Builder()
        .Environment(Square.Environment.Sandbox)
        .AccessToken(SquareAccessToken)
        .Build();
}