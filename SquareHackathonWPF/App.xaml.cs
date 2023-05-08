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
        .AccessToken(App.GetSquareAccessToken())
        .Build();

    internal static string GetSquareAccessToken()
    {
        // Load the secrets file
        var doc = new XmlDocument();
        doc.Load(@"..\..\..\..\secrets.xml");

        // Get the access token
        var tokenNode = doc.SelectSingleNode("/secrets/square_access_token");
        var accessToken = tokenNode!.InnerText;
        
        return accessToken;
    }

    internal static string GetOpenExchangeRatesAppId()
    {
        // Load the secrets file
        var doc = new XmlDocument();
        doc.Load(@"..\..\..\..\secrets.xml");

        // Get the app id
        var tokenNode = doc.SelectSingleNode("/secrets/open_exchange_rates_app_id");
        var appId = tokenNode!.InnerText;
        
        return appId;
    }
}