using JapaneseVerbConjugation.SharedResources.Logic;
using System;
using System.IO;

namespace JapaneseVerbConjugation
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
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
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.SetDefaultFont(
                new Font("Yu Gothic UI", 11F)
            );
            Application.Run(new VerbConjugationForm());
        }
    }
}