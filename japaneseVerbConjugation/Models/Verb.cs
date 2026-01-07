using japaneseVerbConjugation.Enums;

namespace japaneseVerbConjugation.Models
{
    public sealed class Verb
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        // Required
        public string DictionaryForm { get; set; } = null!; // 食べる
        public string Reading { get; set; } = null!; // たべる
        public VerbGroup Group { get; set; }

        // Optional metadata
        public string? Meaning { get; set; }

        // Cached conjugations
        public Dictionary<ConjugationForm, ConjugationAnswer> Conjugations { get; set; } = [];

        public UserNote? UserNotes { get; set; }
    }
}
