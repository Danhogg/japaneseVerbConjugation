namespace japaneseVerbConjugation.Models.ModelsForSerialising
{
    public sealed class ProgressStore
    {
        public int SchemaVersion { get; set; } = 1;
        public List<VerbProgress> Progress { get; set; } = new();
    }
}
