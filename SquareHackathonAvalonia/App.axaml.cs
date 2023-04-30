using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SquareHackathonAvalonia.ViewModels;
using SquareHackathonAvalonia.Views;
using System.Xml;

namespace SquareHackathonAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    internal static string GetSquareAccessToken()
    {
        // Load the secrets file
        var doc = new XmlDocument();
        doc.Load(@"..\..\..\secrets.xml");

        // Get the access token value
        var tokenNode = doc.SelectSingleNode("/secrets/square_access_token");
        var accessToken = tokenNode!.InnerText;

        return accessToken;
    }
}