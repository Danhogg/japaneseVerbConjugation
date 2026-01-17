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
            var baseDir = AppContext.BaseDirectory;
            var baseData = Path.Combine(baseDir, "Data");
            var devData = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "JapaneseVerbConjugation.Core", "Data"));
            DataPathProvider.SetDataRootFromCandidates(baseData, devData);
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}