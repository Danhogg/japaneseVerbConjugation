using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.SharedResources.Constants;
using System.Text.Json;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class AppOptionsStore
    {
        private const string FileName = "appoptions.json";

        public static AppOptions LoadOrCreateDefault()
        {
            var path = GetOptionsPath();

            try
            {
                if (!File.Exists(path))
                    return new AppOptions();

                var json = File.ReadAllText(path);
                var options = JsonSerializer.Deserialize<AppOptions>(json, JsonOptions());

                return options ?? new AppOptions
                {
                    PersistUserAnswers = true,
                    ShowFurigana = true,
                    AllowHiragana = false,
                    FocusModeOnly = false,
                    EnabledConjugations = [.. Enum.GetValues<ConjugationFormEnum>()
                        .Where(form => form.ToString() != ConjugationNameConstants.DictionaryFormConst)]
                }; ;
            }
            catch
            {
                // If file is corrupt or schema mismatched, fall back safely
                return new AppOptions
                {
                    PersistUserAnswers = true,
                    ShowFurigana = true,
                    AllowHiragana = false,
                    FocusModeOnly = false,
                    EnabledConjugations = [.. Enum.GetValues<ConjugationFormEnum>()
                        .Where(form => form.ToString() != ConjugationNameConstants.DictionaryFormConst)]
                };
            }
        }

        public static void Save(AppOptions options)
        {
            var path = GetOptionsPath();
            var dir = Path.GetDirectoryName(path)!;

            Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(options, JsonOptions());
            File.WriteAllText(path, json);
        }

        private static string GetOptionsPath()
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

        /// <summary>
        /// Compares two AppOptions instances for equality.
        /// </summary>
        public static bool AreEqual(AppOptions a, AppOptions b)
        {
            if (a.ShowFurigana != b.ShowFurigana) return false;
            if (a.AllowHiragana != b.AllowHiragana) return false;
            if (a.FocusModeOnly != b.FocusModeOnly) return false;
            if (a.PersistUserAnswers != b.PersistUserAnswers) return false;

            // HashSet comparison
            return a.EnabledConjugations.SetEquals(b.EnabledConjugations);
        }
    }
}
