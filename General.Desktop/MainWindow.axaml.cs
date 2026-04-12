using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.IO;

namespace General.Desktop;

public partial class MainWindow : Window
{
    private readonly string _startUrl;

    public MainWindow()
    {
        InitializeComponent();
        _startUrl = ResolveStartUrl();
        WebViewHost.Source = new Uri(_startUrl);
    }

    private static string ResolveStartUrl()
    {
        var overrideUrl = Environment.GetEnvironmentVariable("GENERAL_DESKTOP_START_URL");
        if (!string.IsNullOrWhiteSpace(overrideUrl))
        {
            return overrideUrl;
        }

        var overrideDistDir = Environment.GetEnvironmentVariable("GENERAL_DESKTOP_DIST_DIR");
        var distUrl = ResolveDistUrl(overrideDistDir)
            ?? ResolveDistUrl(Path.Combine(AppContext.BaseDirectory, "wwwroot"));

        return distUrl ?? "http://localhost:5677/workspace";
    }

    private static string? ResolveDistUrl(string? distDirectory)
    {
        if (string.IsNullOrWhiteSpace(distDirectory))
        {
            return null;
        }

        var indexFile = Path.Combine(distDirectory, "index.html");
        if (!File.Exists(indexFile))
        {
            return null;
        }

        return $"{LocalWebRootServer.Start(distDirectory)}#/workspace";
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Q && e.KeyModifiers.HasFlag(KeyModifiers.Meta))
        {
            e.Handled = true;
            Close();
            return;
        }

        base.OnKeyDown(e);
    }
}
