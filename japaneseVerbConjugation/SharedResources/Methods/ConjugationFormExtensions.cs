using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.SharedResources.Methods
{
    public static class ConjugationFormExtensions
    {
        public static string ToDisplayLabel(this ConjugationFormEnum form) => form switch
        {
            ConjugationFormEnum.PresentPlain => "Present (Plain)",
            ConjugationFormEnum.PresentPolite => "Present (Polite)",

            ConjugationFormEnum.PastPlain => "Past (Plain)",
            ConjugationFormEnum.PastPolite => "Past (Polite)",

            ConjugationFormEnum.TeForm => "Te-form",

            ConjugationFormEnum.NegativePlain => "Negative (Plain)",
            ConjugationFormEnum.NegativePolite => "Negative (Polite)",

            ConjugationFormEnum.VolitionalPlain => "Volitional (Plain)",
            ConjugationFormEnum.VolitionalPolite => "Volitional (Polite)",

            ConjugationFormEnum.ConditionalBa => "Conditional (ば)",
            ConjugationFormEnum.ConditionalTara => "Conditional (たら)",

            ConjugationFormEnum.PotentialPlain => "Potential (Plain)",
            ConjugationFormEnum.PotentialPolite => "Potential (Polite)",

            ConjugationFormEnum.PassivePlain => "Passive (Plain)",
            ConjugationFormEnum.PassivePolite => "Passive (Polite)",

            ConjugationFormEnum.CausativePlain => "Causative (Plain)",
            ConjugationFormEnum.CausativePolite => "Causative (Polite)",

            ConjugationFormEnum.CausativePassivePlain => "Causative-Passive (Plain)",
            ConjugationFormEnum.CausativePassivePolite => "Causative-Passive (Polite)",

            ConjugationFormEnum.Imperative => "Imperative",

            _ => form.ToString()
        };
    }
}
