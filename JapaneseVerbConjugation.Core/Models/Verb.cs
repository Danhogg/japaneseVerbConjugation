using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;

namespace JapaneseVerbConjugation.Core.Models
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
        public bool IsFavorite { get; set; }

        // Cached conjugations
        public Dictionary<ConjugationFormEnum, ConjugationAnswer> Conjugations { get; set; } = [];

        // Track if verb group has been answered correctly
        public bool VerbGroupAnsweredCorrectly { get; set; }

        // Track which verb is currently active (the one being studied)
        public bool Active { get; set; }

        public UserNote? UserNotes { get; set; }
    }
}
