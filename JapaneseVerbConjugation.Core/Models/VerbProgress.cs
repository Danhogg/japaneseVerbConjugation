namespace JapaneseVerbConjugation.Models
{
    public sealed class VerbProgress
    {
        public Guid VerbId { get; set; }

        public int TimesSeen { get; set; }
        public int TimesCorrect { get; set; }

        public bool MarkedDifficult { get; set; }
        public DateTime? LastSeenUtc { get; set; }
    }
}
