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
public partial class App : Application
{
    internal static string GetSquareAccessToken()
    {
        // Load the secrets file
        var doc = new XmlDocument();
        doc.Load(@"..\..\..\..\secrets.xml");

        // Get the access token value
        var tokenNode = doc.SelectSingleNode("/secrets/square_access_token");
        var accessToken = tokenNode!.InnerText;

        return accessToken;
    }
}