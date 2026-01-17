namespace JapaneseVerbConjugation.SharedResources.Methods
{
    /// <summary>
    /// Utility methods for masking hints in Japanese text.
    /// </summary>
    public static class HintMasking
    {
        /// <summary>
        /// Masks hiragana characters in the answer, keeping only the final hiragana if there are multiple.
        /// </summary>
        public static string MaskHint(string answer)
        {
            return new string([.. answer
                .Select((c, i) =>
                {
                    if (!IsHiragana(c))
                        return c;

                    bool isLast = i == answer.Length - 1;
                    bool prevIsHiragana = i > 0 && IsHiragana(answer[i - 1]);

                    // Keep final hiragana only if there is more than one
                    if (isLast && prevIsHiragana)
                        return c;

                    return '＊';
                })]);
        }

        /// <summary>
        /// Checks if a character is hiragana (Unicode range \u3040-\u309F).
        /// </summary>
        public static bool IsHiragana(char c)
            => c >= '\u3040' && c <= '\u309F';
    }
}
