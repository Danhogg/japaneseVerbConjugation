using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class AnswerChecker
    {
        public static ConjugationResult Check(
            string? userInput,
            IReadOnlyList<string> expectedAnswers,
            AppOptions options)
        {
            var input = (userInput ?? string.Empty).Trim();
            if (input.Length == 0)
                return ConjugationResult.Unchecked;

            // Filter expected by allowed script options
            var filtered = expectedAnswers
                .Select(a => a.Trim())
                .Where(a => a.Length > 0)
                .Where(a => IsAllowedByOptions(a, options))
                .ToList();

            if (filtered.Count == 0)
                return ConjugationResult.Incorrect;

            if (filtered.Any(a => a == input))
                return ConjugationResult.Correct;

            // "Close" heuristic for now:
            // - if kana is allowed and input is kana and differs by small edit distance (1),
            //   mark close. This is intentionally conservative and cheap.
            if (options.AllowKana && LooksLikeKana(input))
            {
                if (filtered.Any(a => LooksLikeKana(a) && LevenshteinDistanceAtMostOne(a, input)))
                    return ConjugationResult.Close;
            }

            return ConjugationResult.Incorrect;
        }

        private static bool IsAllowedByOptions(string answer, AppOptions options)
        {
            // If only Kanji allowed: reject kana-only answers.
            // If only Kana allowed: reject answers that contain kanji.
            // If both allowed: accept either (mixed across entries is automatically fine).
            bool hasKanji = ContainsKanji(answer);
            bool isKanaOnly = LooksLikeKana(answer) && !hasKanji;

            if (options.AllowKanji && options.AllowKana)
                return true;

            if (options.AllowKanji && !options.AllowKana)
                return !isKanaOnly;

            if (!options.AllowKanji && options.AllowKana)
                return !hasKanji;

            // If user disables both, nothing is valid. Treat as reject all.
            return false;
        }

        private static bool ContainsKanji(string s)
            => s.Any(c => c >= '\u4E00' && c <= '\u9FFF');

        private static bool LooksLikeKana(string s)
            => s.All(c =>
                (c >= '\u3040' && c <= '\u309F') || // hiragana
                (c >= '\u30A0' && c <= '\u30FF') || // katakana
                c == 'ー');

        // Only detect distance <= 1 to keep it cheap + predictable
        private static bool LevenshteinDistanceAtMostOne(string a, string b)
        {
            if (a == b) return true;
            if (Math.Abs(a.Length - b.Length) > 1) return false;

            // substitution case
            if (a.Length == b.Length)
            {
                int diffs = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i] && ++diffs > 1) return false;
                }
                return diffs == 1;
            }

            // insertion/deletion case
            // ensure a is shorter
            if (a.Length > b.Length)
                (a, b) = (b, a);

            int iA = 0, iB = 0, edits = 0;
            while (iA < a.Length && iB < b.Length)
            {
                if (a[iA] == b[iB])
                {
                    iA++; iB++;
                    continue;
                }

                if (++edits > 1) return false;
                iB++; // skip one char in longer string
            }

            return true;
        }
    }
}
