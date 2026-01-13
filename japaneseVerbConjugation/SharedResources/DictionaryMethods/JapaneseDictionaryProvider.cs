using JapaneseVerbConjugation.Interfaces;
using System.Diagnostics;

namespace JapaneseVerbConjugation.SharedResources.DictionaryMethods
{
    public sealed class JapaneseDictionaryProvider
    {
        private static readonly Lazy<IJapaneseDictionary> _instance =
            new(LoadDictionary);

        public static IJapaneseDictionary Instance => _instance.Value;

        private JapaneseDictionaryProvider() { }

        private static IJapaneseDictionary LoadDictionary()
        {
            // 🔹 This is THE load point
            var dict = DictionaryLoader.LoadOrThrow();

            RunSanityChecks(dict);

            return dict;
        }

        private static void RunSanityChecks(IJapaneseDictionary dict)
        {
            // These should NEVER fail in a healthy JMdict load
            var probes = new[] { "する", "来る", "行く", "食べる" };

            foreach (var p in probes)
            {
                if (dict.TryGetReading(p, out var reading))
                    Debug.WriteLine($"[DICT OK] {p} -> {reading}");
                else
                    Debug.WriteLine($"[DICT MISS] {p}");
            }
        }
    }
}
