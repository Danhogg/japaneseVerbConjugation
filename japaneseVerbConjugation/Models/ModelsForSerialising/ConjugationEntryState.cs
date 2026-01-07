using japaneseVerbConjugation.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace japaneseVerbConjugation.Models.ModelsForSerialising
{
    public sealed class ConjugationEntryState
    {
        public ConjugationForm ConjugationForm { get; init; }
        public string? UserInput { get; set; }
        public ConjugationResult Result { get; set; } = ConjugationResult.Unchecked;
    }
}
