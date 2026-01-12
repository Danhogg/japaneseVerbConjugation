using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.SharedResources.Methods
{
    public static class ConjugationFormExtensions
    {
        public static string ToDisplayLabel(this ConjugationForm form) => form switch
        {
            ConjugationForm.PresentPlain => "Present (Plain)",
            ConjugationForm.PresentPolite => "Present (Polite)",

            ConjugationForm.PastPlain => "Past (Plain)",
            ConjugationForm.PastPolite => "Past (Polite)",

            ConjugationForm.TeForm => "Te-form",

            ConjugationForm.NegativePlain => "Negative (Plain)",
            ConjugationForm.NegativePolite => "Negative (Polite)",

            ConjugationForm.VolitionalPlain => "Volitional (Plain)",
            ConjugationForm.VolitionalPolite => "Volitional (Polite)",

            ConjugationForm.ConditionalBa => "Conditional (ば)",
            ConjugationForm.ConditionalTara => "Conditional (たら)",

            ConjugationForm.PotentialPlain => "Potential (Plain)",
            ConjugationForm.PotentialPolite => "Potential (Polite)",

            ConjugationForm.PassivePlain => "Passive (Plain)",
            ConjugationForm.PassivePolite => "Passive (Polite)",

            ConjugationForm.CausativePlain => "Causative (Plain)",
            ConjugationForm.CausativePolite => "Causative (Polite)",

            ConjugationForm.CausativePassivePlain => "Causative-Passive (Plain)",
            ConjugationForm.CausativePassivePolite => "Causative-Passive (Polite)",

            ConjugationForm.Imperative => "Imperative",

            _ => form.ToString()
        };
    }
}
