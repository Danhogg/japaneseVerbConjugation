using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.Models.ModelsForSerialising
{
    public sealed class ConjugationEntryState
    {
        public ConjugationFormEnum ConjugationForm { get; init; }
        public string? UserInput { get; set; }
        public ConjugationResultEnum Result { get; set; } = ConjugationResultEnum.Unchecked;
    }
}
