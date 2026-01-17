using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JapaneseVerbConjugation.SharedResources.Logic;
using System;
using System.IO;

namespace JapaneseVerbConjugation.AvaloniaUI;

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
            var baseData = Path.Combine(AppContext.BaseDirectory, "Data");
            if (Directory.Exists(baseData))
            {
                DataPathProvider.SetDataRoot(baseData);
            }
            else
            {
                var devData = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "JapaneseVerbConjugation.Core", "Data"));
                DataPathProvider.SetDataRoot(devData);
            }
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}