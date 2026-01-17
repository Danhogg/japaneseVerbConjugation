namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class CustomCsvStore
    {
        private const string FileName = "custom.csv";

        public static string EnsureExists()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JapaneseVerbConjugation");

            Directory.CreateDirectory(dir);

            var path = Path.Combine(dir, FileName);

            if (!File.Exists(path))
            {
                // Write a template header that matches your importer
                File.WriteAllText(path, "DictionaryForm");
            }

            return path;
        }
    }
}
