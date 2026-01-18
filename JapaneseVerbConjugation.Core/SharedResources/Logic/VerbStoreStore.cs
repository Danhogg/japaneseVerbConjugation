using JapaneseVerbConjugation.Models.ModelsForSerialising;
using System.Text.Json;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class VerbStoreStore
    {
        private const string FileName = "verbs.json";
        private static string? _overridePath;

        public static void SetOverridePath(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _overridePath = null;
                return;
            }

            _overridePath = Path.GetFullPath(filePath);
        }

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

        public static void CreateBackup()
        {
            var path = GetPath();
            if (!File.Exists(path))
                return;

            var dir = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(dir);

            var backupPath = Path.Combine(dir, "verbs.backup.json");
            File.Copy(path, backupPath, overwrite: true);
        }

        private static string GetPath()
        {
            if (!string.IsNullOrWhiteSpace(_overridePath))
                return _overridePath;

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
