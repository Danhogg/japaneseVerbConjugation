using JapaneseVerbConjugation.Models.ModelsForSerialising;
using System.Text.Json;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class VerbStoreStore
    {
        private const string FileName = "verbs.json";

        public static VerbStore LoadOrCreateDefault()
        {
            var path = GetPath();

            try
            {
                if (!File.Exists(path))
                    return new VerbStore();

                var json = File.ReadAllText(path);
                var store = JsonSerializer.Deserialize<VerbStore>(json, JsonOptions());
                return store ?? new VerbStore();
            }
            catch
            {
                return new VerbStore();
            }
        }

        public static void Save(VerbStore store)
        {
            var path = GetPath();
            var dir = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(store, JsonOptions());
            File.WriteAllText(path, json);
        }

        private static string GetPath()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JapaneseVerbConjugation");

            return Path.Combine(dir, FileName);
        }

        private static JsonSerializerOptions JsonOptions() => new()
        {
            WriteIndented = true
        };
    }
}
