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
            reading = string.Empty;
            return false;
        }

        public bool TryGetVerbGroup(string dictionaryForm, out VerbGroupEnum group)
            => _groups.TryGetValue(StringNormalization.NormalizeKey(dictionaryForm), out group);
    }
}
