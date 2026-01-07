using japaneseVerbConjugation.Enums;

namespace japaneseVerbConjugation.Models
{
    public sealed class AppOptions
    {
        public int SchemaVersion { get; set; } = 1;

        // Display
        public bool ShowFurigana { get; set; } = false;

        // Answer checking
        public bool AllowKanji { get; set; } = true;
        public bool AllowKana { get; set; } = false;

        // Study filters
        public bool FocusModeOnly { get; set; } = false;

        // Progress persistence
        public bool PersistUserAnswers { get; set; } = false;

        // Enabled conjugations
        public HashSet<ConjugationForm> EnabledConjugations { get; set; } = [];
    }
}
