using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class AnswerChecker
    {
        public static ConjugationResult Check(string? userInput, IReadOnlyList<string> expectedAnswers, AppOptions options)
        {
            var input = (userInput ?? string.Empty).Trim();
            if (input.Length == 0)
                return ConjugationResult.Unchecked;

            if (expectedAnswers is null || expectedAnswers.Count == 0)
                return ConjugationResult.Incorrect;

            // Filter expected answers based on options
            IReadOnlyList<string> filtered = FilterExpected(expectedAnswers, options);
            if (filtered.Count == 0)
                filtered = expectedAnswers; // fallback: don't fail silently

            // Exact match wins immediately
            foreach (var exp in filtered)
            {
                if (string.Equals(input, exp, StringComparison.Ordinal))
                    return ConjugationResult.Correct;
            }

            // Close match (small typo / missing char etc.)
            foreach (var exp in filtered)
            {
                if (IsClose(input, exp))
                    return ConjugationResult.Close;
            }

            return ConjugationResult.Incorrect;
        }

        private static List<string> FilterExpected(IReadOnlyList<string> expected, AppOptions options)
        {
            var list = new List<string>(expected.Count);

            foreach (var e in expected)
            {
                if (string.IsNullOrWhiteSpace(e))
                    continue;

                bool hasKanji = ContainsKanji(e);
                bool looksKana = LooksLikeKana(e);

                // If both allowed, take everything
                if (options.AllowKanji && options.AllowKana)
                {
                    list.Add(e);
                    continue;
                }

                if (options.AllowKanji && !options.AllowKana)
                {
                    // Prefer kanji answers; but if the engine only produced kana, don't block learning
                    if (hasKanji || !looksKana)
                        list.Add(e);
                    continue;
                }

                if (!options.AllowKanji && options.AllowKana)
                {
                    if (looksKana)
                        list.Add(e);
                    continue;
                }

                // If both are false (shouldn't happen), fallback to accept all
                list.Add(e);
            }

            return list;
        }

        private static bool IsClose(string input, string expected)
        {
            // Quick win: if one is a prefix of the other and the remaining difference is tiny
            // Example: 泳ぎませ vs 泳ぎません (missing ん)
            if (expected.StartsWith(input, StringComparison.Ordinal))
            {
                var diff = expected.Length - input.Length;
                if (diff is 1 or 2)
                    return true;
            }

            if (input.StartsWith(expected, StringComparison.Ordinal))
            {
                var diff = input.Length - expected.Length;
                if (diff is 1 or 2)
                    return true;
            }

            // Levenshtein distance threshold (simple + predictable)
            // Allow up to 1 edit for short strings, 2 edits for longer ones.
            int threshold = expected.Length <= 6 ? 1 : 2;

            return LevenshteinDistance(input, expected, threshold) <= threshold;
        }

        // Levenshtein with early-exit when exceeding maxDistance
        private static int LevenshteinDistance(string a, string b, int maxDistance)
        {
            if (a == b) return 0;

            int n = a.Length;
            int m = b.Length;

            if (Math.Abs(n - m) > maxDistance)
                return maxDistance + 1;

            // Ensure a is the shorter one for less memory
            if (n > m)
            {
                (a, b) = (b, a);
                (n, m) = (m, n);
            }

            var prev = new int[n + 1];
            var curr = new int[n + 1];

            for (int i = 0; i <= n; i++)
                prev[i] = i;

            for (int j = 1; j <= m; j++)
            {
                curr[0] = j;
                int minInRow = curr[0];

                char bj = b[j - 1];

                for (int i = 1; i <= n; i++)
                {
                    int cost = a[i - 1] == bj ? 0 : 1;

                    int del = prev[i] + 1;
                    int ins = curr[i - 1] + 1;
                    int sub = prev[i - 1] + cost;

                    int val = del < ins ? del : ins;
                    if (sub < val) val = sub;

                    curr[i] = val;
                    if (val < minInRow) minInRow = val;
                }

                if (minInRow > maxDistance)
                    return maxDistance + 1;

                (prev, curr) = (curr, prev);
            }

            return prev[n];
        }

        private static bool ContainsKanji(string s)
            => s.Any(c => c >= '\u4E00' && c <= '\u9FFF');

        private static bool LooksLikeKana(string s)
            => s.All(c =>
                (c >= '\u3040' && c <= '\u309F') || // hiragana
                (c >= '\u30A0' && c <= '\u30FF') || // katakana
                c == 'ー');
    }
}
