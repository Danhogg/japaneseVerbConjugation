using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.Models.ModelsForSerialising
{
    public sealed class ConjugationEntryState
    {
        public ConjugationForm ConjugationForm { get; init; }
        public string? UserInput { get; set; }
        public ConjugationResult Result { get; set; } = ConjugationResult.Unchecked;
    }
}
