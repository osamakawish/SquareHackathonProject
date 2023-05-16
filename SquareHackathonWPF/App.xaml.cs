using Square;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace SquareHackathonWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    // Modify code in the future to choose between sandbox and production modes.
    internal static SquareClient Client { get; } = new SquareClient.Builder()
        .Environment(Square.Environment.Sandbox)
        .AccessToken(GetSquareAccessToken())
        .Build();

    private static string GetToken(string subpath)
    {
        var doc = new XmlDocument();
        doc.Load(@"..\..\..\..\secrets.xml");

        var tokenNode = doc.SelectSingleNode($"/secrets/{subpath}");
        return tokenNode!.InnerText;
    }

    internal static string GetSquareAccessToken() => GetToken("square_access_token");

    internal static string GetOpenExchangeRatesAppId() => GetToken("open_exchange_rates_app_id");

    internal static string GetAzureKey1() => GetToken("azure_key_1");
}