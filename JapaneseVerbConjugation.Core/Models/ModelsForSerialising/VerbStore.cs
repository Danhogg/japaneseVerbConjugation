using JapaneseVerbConjugation.Core.Models;

namespace JapaneseVerbConjugation.Models.ModelsForSerialising
{
    public sealed class VerbStore
    {
        public int SchemaVersion { get; set; } = 1;
        public List<Verb> Verbs { get; set; } = new();
    }
}
