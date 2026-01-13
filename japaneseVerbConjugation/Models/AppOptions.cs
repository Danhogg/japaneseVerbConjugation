using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.Models
{
    public sealed class AppOptions
    {
        public int SchemaVersion { get; set; } = 1;

        // Display
        public bool ShowFurigana { get; set; } = false;

        // Answer checking
        public bool AllowHiragana { get; set; } = false;

        // Study filters
        public bool FocusModeOnly { get; set; } = false;

        // Progress persistence
        public bool PersistUserAnswers { get; set; } = false;

        // Enabled conjugations
        // I don't like hard coding the dictionary exclusion in this way but it's simple
        // and not that big of a deal
        public HashSet<ConjugationFormEnum> EnabledConjugations { get; set; } 
            = [.. Enum.GetValues<ConjugationFormEnum>().Where(form => form.ToString() != "Dictionary")];
    }
}
