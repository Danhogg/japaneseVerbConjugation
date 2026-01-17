using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Interfaces;
using JapaneseVerbConjugation.SharedResources.Methods;

namespace JapaneseVerbConjugation.SharedResources.DictionaryMethods
{
    public sealed class JapaneseDictionaryIndex(
        Dictionary<string, string> readings,
        Dictionary<string, VerbGroupEnum> groups) : IJapaneseDictionary
    {
        private readonly Dictionary<string, string> _readings = readings ?? throw new ArgumentNullException(nameof(readings));
        private readonly Dictionary<string, VerbGroupEnum> _groups = groups ?? throw new ArgumentNullException(nameof(groups));

        public bool TryGetReading(string dictionaryForm, out string reading)
        {
            var normalized = StringNormalization.NormalizeKey(dictionaryForm);
            if (_readings.TryGetValue(normalized, out var value))
            {
                reading = value;
                return true;
            }

            // Pragmatic fallback for suru-verbs stored as noun headwords in JMdict.
            if (TryGetSuruFallback(normalized, out var suruReading))
            {
                reading = suruReading;
                return true;
            }

            reading = string.Empty;
            return false;
        }

        public bool TryGetVerbGroup(string dictionaryForm, out VerbGroupEnum group)
        {
            var normalized = StringNormalization.NormalizeKey(dictionaryForm);
            if (_groups.TryGetValue(normalized, out group))
                return true;

            if (TryGetSuruFallback(normalized, out _))
            {
                group = VerbGroupEnum.Irregular;
                return true;
            }

            group = default;
            return false;
        }

        private bool TryGetSuruFallback(string normalized, out string reading)
        {
            reading = string.Empty;

            if (!normalized.EndsWith("する", StringComparison.Ordinal))
                return false;

            var stem = normalized[..^2];
            if (string.IsNullOrWhiteSpace(stem))
                return false;

            if (_readings.TryGetValue(stem, out var stemReading))
            {
                reading = $"{stemReading}する";
                return true;
            }

            return false;
        }
    }
}
