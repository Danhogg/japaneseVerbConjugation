using japaneseVerbConjugation.Enums;

namespace japaneseVerbConjugation.SharedMethods
{
    public static class ConjugationFormExtensions
    {
        public static string ToDisplayLabel(this ConjugationForm form) => form switch
        {
            ConjugationForm.TeForm => "Te-form",
            ConjugationForm.PastPlain => "Past (Plain)",
            ConjugationForm.PastPolite => "Past (Polite)",
            _ => form.ToString()
        };
    }
}
