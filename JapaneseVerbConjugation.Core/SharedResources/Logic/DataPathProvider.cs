using System;
using System.IO;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class DataPathProvider
    {
        private static string? _dataRoot;

        public static void SetDataRoot(string? dataRoot)
        {
            if (string.IsNullOrWhiteSpace(dataRoot))
            {
                _dataRoot = null;
                return;
            }

            _dataRoot = Path.GetFullPath(dataRoot);
        }

        public static string GetDataRoot()
        {
            return _dataRoot ?? Path.Combine(AppContext.BaseDirectory, "Data");
        }

        public static string ResolveDataFile(string fileName)
        {
            return Path.Combine(GetDataRoot(), fileName);
        }

        public static string? TryResolveDataFile(string fileName)
        {
            var path = ResolveDataFile(fileName);
            return File.Exists(path) ? path : null;
        }
    }
}
