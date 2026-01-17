using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class ConjugationEngine
    {
        public static IReadOnlyList<string> Generate(
            string dictionaryForm,
            string reading,
            VerbGroupEnum group,
            ConjugationFormEnum form)
        {
            if (string.IsNullOrWhiteSpace(dictionaryForm))
                return [];

            dictionaryForm = dictionaryForm.Trim();
            reading = (reading ?? string.Empty).Trim();

            if (group == VerbGroupEnum.Irregular)
                return GenerateIrregular(dictionaryForm, reading, form);

            var hasReading = !string.IsNullOrWhiteSpace(reading);

            return group switch
            {
                VerbGroupEnum.Ichidan => GenerateIchidan(dictionaryForm, reading, hasReading, form),
                VerbGroupEnum.Godan => GenerateGodan(dictionaryForm, reading, hasReading, form),
                _ => []
            };
        }

        // ----------------------
        // ICHIDAN
        // ----------------------
        private static IReadOnlyList<string> GenerateIchidan(
            string dict,
            string reading,
            bool hasReading,
            ConjugationFormEnum form)
        {
            if (!dict.EndsWith('る'))
                return [];

            string kanjiStem = dict[..^1];
            string kanaStem = hasReading && reading.EndsWith('る') ? reading[..^1] : string.Empty;

            // Helpers for tara based on past-plain
            IReadOnlyList<string> PastPlain() =>
                Pack(kanjiStem + "た", hasReading ? kanaStem + "た" : null);

            return form switch
            {
                ConjugationFormEnum.PresentPlain =>
                    Pack(dict, hasReading ? reading : null),

                ConjugationFormEnum.PresentPolite =>
                    Pack(kanjiStem + "ます", hasReading ? kanaStem + "ます" : null),

                ConjugationFormEnum.PastPlain =>
                    PastPlain(),

                ConjugationFormEnum.PastPolite =>
                    Pack(kanjiStem + "ました", hasReading ? kanaStem + "ました" : null),

                ConjugationFormEnum.NegativePlain =>
                    Pack(kanjiStem + "ない", hasReading ? kanaStem + "ない" : null),

                ConjugationFormEnum.NegativePolite =>
                    Pack(kanjiStem + "ません", hasReading ? kanaStem + "ません" : null),

                ConjugationFormEnum.TeForm =>
                    Pack(kanjiStem + "て", hasReading ? kanaStem + "て" : null),

                ConjugationFormEnum.VolitionalPlain =>
                    Pack(kanjiStem + "よう", hasReading ? kanaStem + "よう" : null),

                ConjugationFormEnum.VolitionalPolite =>
                    Pack(kanjiStem + "ましょう", hasReading ? kanaStem + "ましょう" : null),

                ConjugationFormEnum.ConditionalBa =>
                    Pack(kanjiStem + "れば", hasReading ? kanaStem + "れば" : null),

                ConjugationFormEnum.ConditionalTara =>
                    AppendSuffix(PastPlain(), "ら"),

                // Potential (Ichidan): standard ～られる, common alternate ～れる
                ConjugationFormEnum.PotentialPlain =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "られる",
                        primaryKana: hasReading ? kanaStem + "られる" : null,
                        altKanji: kanjiStem + "れる",
                        altKana: hasReading ? kanaStem + "れる" : null),

                ConjugationFormEnum.PotentialPolite =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "られます",
                        primaryKana: hasReading ? kanaStem + "られます" : null,
                        altKanji: kanjiStem + "れます",
                        altKana: hasReading ? kanaStem + "れます" : null),

                // Passive (Ichidan): same surface form as potential
                ConjugationFormEnum.PassivePlain =>
                    Pack(kanjiStem + "られる", hasReading ? kanaStem + "られる" : null),

                ConjugationFormEnum.PassivePolite =>
                    Pack(kanjiStem + "られます", hasReading ? kanaStem + "られます" : null),

                ConjugationFormEnum.CausativePlain =>
                    Pack(kanjiStem + "させる", hasReading ? kanaStem + "させる" : null),

                ConjugationFormEnum.CausativePolite =>
                    Pack(kanjiStem + "させます", hasReading ? kanaStem + "させます" : null),

                ConjugationFormEnum.CausativePassivePlain =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "させられる",
                        primaryKana: hasReading ? kanaStem + "させられる" : null,
                        altKanji: kanjiStem + "させれる",
                        altKana: hasReading ? kanaStem + "させれる" : null),

                ConjugationFormEnum.CausativePassivePolite =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "させられます",
                        primaryKana: hasReading ? kanaStem + "させられます" : null,
                        altKanji: kanjiStem + "させれます",
                        altKana: hasReading ? kanaStem + "させれます" : null),

                // Imperative: ～ろ is common; ～よ also exists (more formal/literary)
                ConjugationFormEnum.Imperative =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "ろ",
                        primaryKana: hasReading ? kanaStem + "ろ" : null,
                        altKanji: kanjiStem + "よ",
                        altKana: hasReading ? kanaStem + "よ" : null),

                _ => []
            };
        }

        // ----------------------
        // GODAN
        // ----------------------
        private static IReadOnlyList<string> GenerateGodan(
            string dict,
            string reading,
            bool hasReading,
            ConjugationFormEnum form)
        {
            if (!hasReading)
                return [];

            if (reading.Length < 1)
                return [];

            char lastKana = reading[^1];
            string kanaStem = reading[..^1];
            string kanjiStem = dict[..^1];

            // ✅ Special-case: 行く / いく family
            // Applies to all "iku" verbs (行く / 逝く / 往く / いく)
            bool isIkuVerb = reading == "いく" && lastKana == 'く';

            // Precompute endings once
            string teEnding = isIkuVerb
                ? "って"
                : GodanRules.TeEnding(lastKana);

            string pastEnding = isIkuVerb
                ? "った"
                : GodanRules.PastEnding(lastKana);

            var pastPlain = Pack(
                kanjiStem + pastEnding,
                kanaStem + pastEnding);

            return form switch
            {
                ConjugationFormEnum.PresentPlain =>
                    Pack(dict, reading),

                ConjugationFormEnum.PresentPolite =>
                    Pack(
                        kanjiStem + GodanRules.IStem(lastKana) + "ます",
                        kanaStem + GodanRules.IStem(lastKana) + "ます"
                    ),

                ConjugationFormEnum.TeForm =>
                    Pack(
                        kanjiStem + teEnding,
                        kanaStem + teEnding
                    ),

                ConjugationFormEnum.PastPlain =>
                    pastPlain,

                ConjugationFormEnum.PastPolite =>
                    Pack(
                        kanjiStem + GodanRules.IStem(lastKana) + "ました",
                        kanaStem + GodanRules.IStem(lastKana) + "ました"
                    ),

                ConjugationFormEnum.NegativePlain =>
                    Pack(
                        kanjiStem + GodanRules.AStem(lastKana) + "ない",
                        kanaStem + GodanRules.AStem(lastKana) + "ない"
                    ),

                ConjugationFormEnum.NegativePolite =>
                    Pack(
                        kanjiStem + GodanRules.IStem(lastKana) + "ません",
                        kanaStem + GodanRules.IStem(lastKana) + "ません"
                    ),

                ConjugationFormEnum.VolitionalPlain =>
                    Pack(kanjiStem + GodanRules.OStem(lastKana) + "う",
                         kanaStem + GodanRules.OStem(lastKana) + "う"),

                ConjugationFormEnum.VolitionalPolite =>
                    Pack(kanjiStem + GodanRules.IStem(lastKana) + "ましょう",
                         kanaStem + GodanRules.IStem(lastKana) + "ましょう"),

                ConjugationFormEnum.ConditionalBa =>
                    Pack(kanjiStem + GodanRules.EStem(lastKana) + "ば",
                         kanaStem + GodanRules.EStem(lastKana) + "ば"),

                ConjugationFormEnum.ConditionalTara =>
                    AppendSuffix(pastPlain, "ら"),

                ConjugationFormEnum.PotentialPlain =>
                    Pack(kanjiStem + GodanRules.EStem(lastKana) + "る",
                         kanaStem + GodanRules.EStem(lastKana) + "る"),

                ConjugationFormEnum.PotentialPolite =>
                    Pack(kanjiStem + GodanRules.EStem(lastKana) + "ます",
                         kanaStem + GodanRules.EStem(lastKana) + "ます"),

                ConjugationFormEnum.PassivePlain =>
                    Pack(kanjiStem + GodanRules.AStem(lastKana) + "れる",
                         kanaStem + GodanRules.AStem(lastKana) + "れる"),

                ConjugationFormEnum.PassivePolite =>
                    Pack(kanjiStem + GodanRules.AStem(lastKana) + "れます",
                         kanaStem + GodanRules.AStem(lastKana) + "れます"),

                // Causative: a-stem + せる, but す => させる
                ConjugationFormEnum.CausativePlain =>
                    Pack(kanjiStem + GodanCausativeBase(lastKana) + "る",
                         kanaStem + GodanCausativeBase(lastKana) + "る"),

                ConjugationFormEnum.CausativePolite =>
                    Pack(kanjiStem + GodanCausativeBase(lastKana) + "ます",
                         kanaStem + GodanCausativeBase(lastKana) + "ます"),

                ConjugationFormEnum.CausativePassivePlain =>
                    Pack(kanjiStem + GodanCausativeBase(lastKana) + "られる",
                         kanaStem + GodanCausativeBase(lastKana) + "られる"),

                ConjugationFormEnum.CausativePassivePolite =>
                    Pack(kanjiStem + GodanCausativeBase(lastKana) + "られます",
                         kanaStem + GodanCausativeBase(lastKana) + "られます"),

                // Imperative: e-stem only (書け、読め)
                ConjugationFormEnum.Imperative =>
                    Pack(kanjiStem + GodanRules.EStem(lastKana),
                         kanaStem + GodanRules.EStem(lastKana)),

                _ => []
            };
        }

        private static string GodanCausativeBase(char lastKana)
        {
            // Standard: a-stem + せ (then + る/ます/られる/られます)
            // For す: させ
            return lastKana == 'す'
                ? "させ"
                : GodanRules.AStem(lastKana) + "せ";
        }

        // ----------------------
        // IRREGULAR (minimum: する / 来る)
        // ----------------------
        private static IReadOnlyList<string> GenerateIrregular(
            string dict,
            string reading,
            ConjugationFormEnum form)
        {
            bool isSuru = dict is "する" or "為る";
            bool isKuru = dict is "来る" or "くる";

            if (!isSuru && !isKuru)
                return [];

            if (isSuru)
            {
                return form switch
                {
                    ConjugationFormEnum.PresentPlain => new[] { "する" },
                    ConjugationFormEnum.PresentPolite => new[] { "します" },

                    ConjugationFormEnum.TeForm => new[] { "して" },
                    ConjugationFormEnum.PastPlain => new[] { "した" },
                    ConjugationFormEnum.PastPolite => new[] { "しました" },

                    ConjugationFormEnum.NegativePlain => new[] { "しない" },
                    ConjugationFormEnum.NegativePolite => new[] { "しません" },

                    ConjugationFormEnum.VolitionalPlain => new[] { "しよう" },
                    ConjugationFormEnum.VolitionalPolite => new[] { "しましょう" },

                    ConjugationFormEnum.ConditionalBa => new[] { "すれば" },
                    ConjugationFormEnum.ConditionalTara => new[] { "したら" },

                    ConjugationFormEnum.PotentialPlain => new[] { "できる" },
                    ConjugationFormEnum.PotentialPolite => new[] { "できます" },

                    ConjugationFormEnum.PassivePlain => new[] { "される" },
                    ConjugationFormEnum.PassivePolite => new[] { "されます" },

                    ConjugationFormEnum.CausativePlain => new[] { "させる" },
                    ConjugationFormEnum.CausativePolite => new[] { "させます" },

                    ConjugationFormEnum.CausativePassivePlain => new[] { "させられる", "させれる" },
                    ConjugationFormEnum.CausativePassivePolite => new[] { "させられます", "させれます" },

                    ConjugationFormEnum.Imperative => new[] { "しろ", "せよ" },

                    _ => []
                };
            }

            // 来る / くる
            return form switch
            {
                ConjugationFormEnum.PresentPlain => new[] { "くる", "来る" },
                ConjugationFormEnum.PresentPolite => new[] { "きます", "来ます" },

                ConjugationFormEnum.TeForm => new[] { "きて", "来て" },
                ConjugationFormEnum.PastPlain => new[] { "きた", "来た" },
                ConjugationFormEnum.PastPolite => new[] { "きました", "来ました" },

                ConjugationFormEnum.NegativePlain => new[] { "こない", "来ない" },
                ConjugationFormEnum.NegativePolite => new[] { "きません", "来ません" },

                ConjugationFormEnum.VolitionalPlain => new[] { "こよう", "来よう" },
                ConjugationFormEnum.VolitionalPolite => new[] { "きましょう", "来ましょう" },

                ConjugationFormEnum.ConditionalBa => new[] { "くれば", "来れば" },
                ConjugationFormEnum.ConditionalTara => new[] { "きたら", "来たら" },

                ConjugationFormEnum.PotentialPlain => new[] { "こられる", "来られる" },
                ConjugationFormEnum.PotentialPolite => new[] { "こられます", "来られます" },

                // Passive is same surface form for 来る
                ConjugationFormEnum.PassivePlain => new[] { "こられる", "来られる" },
                ConjugationFormEnum.PassivePolite => new[] { "こられます", "来られます" },

                ConjugationFormEnum.CausativePlain => new[] { "こさせる", "来させる" },
                ConjugationFormEnum.CausativePolite => new[] { "こさせます", "来させます" },

                ConjugationFormEnum.CausativePassivePlain => new[] { "こさせられる", "来させられる" },
                ConjugationFormEnum.CausativePassivePolite => new[] { "こさせられます", "来させられます" },

                ConjugationFormEnum.Imperative => new[] { "こい", "来い" },

                _ => []
            };
        }

        // ----------------------
        // Shared helpers
        // ----------------------
        private static IReadOnlyList<string> AppendSuffix(IReadOnlyList<string> bases, string suffix)
        {
            if (bases.Count == 0) return [];

            var list = new List<string>(bases.Count);
            foreach (var b in bases)
            {
                var v = b + suffix;
                if (!list.Contains(v)) list.Add(v);
            }
            return list;
        }

        private static IReadOnlyList<string> Pack(string? kanji, string? kana = null)
        {
            var list = new List<string>(2);
            if (!string.IsNullOrWhiteSpace(kanji)) list.Add(kanji);
            if (!string.IsNullOrWhiteSpace(kana) && kana != kanji) list.Add(kana);
            return list;
        }

        private static IReadOnlyList<string> PackWithAlt(
            string? primaryKanji,
            string? primaryKana,
            string? altKanji,
            string? altKana)
        {
            var list = new List<string>(4);

            void Add(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return;
                if (!list.Contains(s)) list.Add(s);
            }

            Add(primaryKanji);
            Add(primaryKana);
            Add(altKanji);
            Add(altKana);

            return list;
        }
    }
}
