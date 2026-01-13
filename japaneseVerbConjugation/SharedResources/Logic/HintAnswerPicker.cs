namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class HintAnswerPicker
    {
        public static string PickBestForHint(IReadOnlyList<string> expectedAnswers)
        {
            if (expectedAnswers is null || expectedAnswers.Count == 0)
                return string.Empty;

            // Prefer any answer containing kanji
            foreach (var a in expectedAnswers)
            {
                if (!string.IsNullOrWhiteSpace(a) && ContainsKanji(a))
                    return a;
            }

            // Otherwise first non-empty (e.g., する, きて etc.)
            foreach (var a in expectedAnswers)
            {
                if (!string.IsNullOrWhiteSpace(a))
                    return a;
            }

            return string.Empty;
        }

        private static bool ContainsKanji(string s)
            => s.Any(c => c >= '\u4E00' && c <= '\u9FFF');
    }
}
