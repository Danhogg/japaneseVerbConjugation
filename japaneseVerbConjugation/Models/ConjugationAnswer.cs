namespace JapaneseVerbConjugation.Models
{
    public sealed class ConjugationAnswer
    {
        // Canonical answer
        public string Kanji { get; set; } = null!;

        // Optional alternates (rare but future-safe)
        public List<string> AlternateKanji { get; set; } = [];
        public List<string> AlternateReadings { get; set; } = [];

        public UserNote? UserNotes { get; set; }
    }
}
