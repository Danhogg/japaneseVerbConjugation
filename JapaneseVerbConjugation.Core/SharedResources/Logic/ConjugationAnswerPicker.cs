using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class ConjugationAnswerPicker
    {
        // Prefer kanji if present, otherwise fall back to first engine answer
        public static string PickCanonical(IReadOnlyList<string> expected)
        {
            if (expected == null || expected.Count == 0)
                return string.Empty;

            foreach (var s in expected)
            {
                if (ContainsKanji(s))
                    return s;
            }

            return expected[0];
        }

        private static bool ContainsKanji(string s)
        {
            foreach (var c in s)
            {
                if ((c >= '\u4E00' && c <= '\u9FFF') ||
                    (c >= '\u3400' && c <= '\u4DBF'))
                    return true;
            }
            return false;
        }
    }
}
