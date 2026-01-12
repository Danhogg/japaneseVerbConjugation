using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class ConjugationEngine
    {
        // Returns one or more acceptable answers in kana/kanji forms depending on what you pass in.
        // For now, we generate KANA only from the provided reading, and KANJI by applying the same suffix change to dictionaryForm.
        // This is deterministic and keeps you moving without needing full dictionary integration yet.
        public static IReadOnlyList<string> Generate(
            string dictionaryForm,
            string reading,
            VerbGroup group,
            ConjugationForm form)
        {
            if (string.IsNullOrWhiteSpace(dictionaryForm))
                return [];

            dictionaryForm = dictionaryForm.Trim();
            reading = reading.Trim();

            // Irregular core support
            if (group == VerbGroup.Irregular)
                return GenerateIrregular(dictionaryForm, reading, form);

            // If we don't have reading yet, we can still conjugate by kanji suffixing,
            // but we can't do kana correctness well. We'll still return kanji-form answer.
            var hasReading = !string.IsNullOrWhiteSpace(reading);

            return group switch
            {
                VerbGroup.Ichidan => GenerateIchidan(dictionaryForm, reading, hasReading, form),
                VerbGroup.Godan => GenerateGodan(dictionaryForm, reading, hasReading, form),
                _ => []
            };
        }

        private static IReadOnlyList<string> GenerateIchidan(
            string dict,
            string reading,
            bool hasReading,
            ConjugationForm form)
        {
            // Ichidan: remove る
            if (!dict.EndsWith("る"))
                return []; // defensive: bad input/group mismatch

            string kanjiStem = dict[..^1];
            string kanaStem = hasReading && reading.EndsWith("る") ? reading[..^1] : string.Empty;

            return form switch
            {
                ConjugationForm.TeForm =>
                    Pack(kanjiStem + "て", hasReading ? kanaStem + "て" : null),

                ConjugationForm.PastPlain =>
                    Pack(kanjiStem + "た", hasReading ? kanaStem + "た" : null),

                ConjugationForm.PastPolite =>
                    Pack(kanjiStem + "ました", hasReading ? kanaStem + "ました" : null),

                ConjugationForm.NegativePlain =>
                    Pack(kanjiStem + "ない", hasReading ? kanaStem + "ない" : null),

                ConjugationForm.NegativePolite =>
                    Pack(kanjiStem + "ません", hasReading ? kanaStem + "ません" : null),

                _ => []
            };
        }

        private static IReadOnlyList<string> GenerateGodan(
            string dict,
            string reading,
            bool hasReading,
            ConjugationForm form)
        {
            // Godan: last kana determines transformations
            // We need the last *kana sound*, not just the last kanji char.
            // So we use reading for rules; if missing, we can only attempt kanji suffixing and will be incomplete.
            if (!hasReading)
                return []; // keep it honest; you can decide later if you want partial behavior

            if (reading.Length < 1)
                return [];

            char kanaLast = reading[^1];
            string kanaStem = reading[..^1];

            // For kanji, we apply the same "remove last kana" concept by removing 1 char from dict.
            // This is correct when dict ends in kana (e.g., 書く, ends with く). It is also common.
            // If dict ends with kanji (rare in dictionary forms), you'll need dictionary mapping anyway.
            string kanjiStem = dict.Length >= 1 ? dict[..^1] : string.Empty;

            return form switch
            {
                ConjugationForm.TeForm => Pack(
                    kanjiStem + GodanTeEnding(kanaLast),
                    kanaStem + GodanTeEnding(kanaLast),
                    extraKana: GodanTeExtraKana(reading)),

                ConjugationForm.PastPlain => Pack(
                    kanjiStem + GodanPastEnding(kanaLast),
                    kanaStem + GodanPastEnding(kanaLast),
                    extraKana: GodanPastExtraKana(reading)),

                ConjugationForm.PastPolite => Pack(
                    kanjiStem + GodanIStem(kanaLast) + "ました",
                    kanaStem + GodanIStem(kanaLast) + "ました"),

                ConjugationForm.NegativePlain => Pack(
                    kanjiStem + GodanAStemForNegative(kanaLast) + "ない",
                    kanaStem + GodanAStemForNegative(kanaLast) + "ない"),

                ConjugationForm.NegativePolite => Pack(
                    kanjiStem + GodanIStem(kanaLast) + "ません",
                    kanaStem + GodanIStem(kanaLast) + "ません"),

                _ => []
            };
        }

        private static IReadOnlyList<string> GenerateIrregular(
            string dict,
            string reading,
            ConjugationForm form)
        {
            // Minimum required: する / 来る
            // Accept common kanji/kana variants
            // dict can be する, 為る (rare), 来る, くる
            bool isSuru = dict is "する" or "為る";
            bool isKuru = dict is "来る" || dict is "くる";

            if (!isSuru && !isKuru)
                return []; // later you can expand irregular list

            if (isSuru)
            {
                return form switch
                {
                    ConjugationForm.TeForm => new[] { "して" },
                    ConjugationForm.PastPlain => new[] { "した" },
                    ConjugationForm.PastPolite => new[] { "しました" },
                    ConjugationForm.NegativePlain => new[] { "しない" },
                    ConjugationForm.NegativePolite => new[] { "しません" },
                    _ => []
                };
            }

            // kuru
            return form switch
            {
                ConjugationForm.TeForm => new[] { "きて", "来て" },
                ConjugationForm.PastPlain => new[] { "きた", "来た" },
                ConjugationForm.PastPolite => new[] { "きました", "来ました" },
                ConjugationForm.NegativePlain => new[] { "こない", "来ない" },
                ConjugationForm.NegativePolite => new[] { "きません", "来ません" },
                _ => []
            };
        }

        // Helpers

        private static string GodanTeEnding(char lastKana) => lastKana switch
        {
            'う' or 'つ' or 'る' => "って",
            'む' or 'ぶ' or 'ぬ' => "んで",
            'く' => "いて",
            'ぐ' => "いで",
            'す' => "して",
            _ => ""
        };

        // Special-case: 行く te-form is いって, not いて
        private static string? GodanTeExtraKana(string reading)
        {
            if (reading == "いく")
                return "いって";
            return null;
        }

        private static string GodanPastEnding(char lastKana) => lastKana switch
        {
            'う' or 'つ' or 'る' => "った",
            'む' or 'ぶ' or 'ぬ' => "んだ",
            'く' => "いた",
            'ぐ' => "いだ",
            'す' => "した",
            _ => ""
        };

        private static string? GodanPastExtraKana(string reading)
        {
            if (reading == "いく")
                return "いった";
            return null;
        }

        private static string GodanIStem(char lastKana) => lastKana switch
        {
            'う' => "い",
            'つ' => "ち",
            'る' => "り",
            'む' => "み",
            'ぶ' => "び",
            'ぬ' => "に",
            'く' => "き",
            'ぐ' => "ぎ",
            'す' => "し",
            _ => ""
        };

        private static string GodanAStemForNegative(char lastKana) => lastKana switch
        {
            'う' => "わ", // crucial: u -> wa for negative
            'つ' => "た",
            'る' => "ら",
            'む' => "ま",
            'ぶ' => "ば",
            'ぬ' => "な",
            'く' => "か",
            'ぐ' => "が",
            'す' => "さ",
            _ => ""
        };

        private static IReadOnlyList<string> Pack(string? kanji, string? kana = null, string? extraKana = null)
        {
            var list = new List<string>(3);
            if (!string.IsNullOrWhiteSpace(kanji)) list.Add(kanji);
            if (!string.IsNullOrWhiteSpace(kana) && kana != kanji) list.Add(kana);
            if (!string.IsNullOrWhiteSpace(extraKana) && !list.Contains(extraKana)) list.Add(extraKana);
            return list;
        }
    }
}
