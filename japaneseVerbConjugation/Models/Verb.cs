using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.Models
{
    public sealed class Verb
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        // Required
        public string DictionaryForm { get; set; } = null!; // 食べる
        public string Reading { get; set; } = null!; // たべる
        public VerbGroupEnum Group { get; set; }

        public JLPTLevelEnum JLPTLevel { get; set; }

        // Optional metadata
        public string? Meaning { get; set; }

        // Cached conjugations
        public Dictionary<ConjugationFormEnum, ConjugationAnswer> Conjugations { get; set; } = [];

        public UserNote? UserNotes { get; set; }
    }
}
