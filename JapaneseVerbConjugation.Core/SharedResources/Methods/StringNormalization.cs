using System.Text;

namespace JapaneseVerbConjugation.SharedResources.Methods
{
    /// <summary>
    /// Shared utility for normalizing Japanese dictionary keys to ensure consistent lookups.
    /// </summary>
    public static class StringNormalization
    {
        /// <summary>
        /// Normalizes a string for use as a dictionary key.
        /// Handles full-width spaces, Unicode normalization, and trims common punctuation.
        /// </summary>
        public static string NormalizeKey(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;

            s = s.Trim()
                 .Trim('\u3000') // full-width space
                 .Normalize(NormalizationForm.FormKC);

            s = s.Trim('\"', '\'', ' ', '\t');
            s = s.TrimEnd(',', '，', '。');

            // Remove internal whitespace and common separator dots to improve lookups
            s = new string(s.Where(c => !char.IsWhiteSpace(c) && c != '・' && c != '･').ToArray());

            return s;
        }
    }
}

