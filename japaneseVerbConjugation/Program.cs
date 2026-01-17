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
            var baseDir = AppContext.BaseDirectory;
            var baseData = Path.Combine(baseDir, "Data");
            var devData = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "JapaneseVerbConjugation.Core", "Data"));
            DataPathProvider.SetDataRootFromCandidates(baseData, devData);
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