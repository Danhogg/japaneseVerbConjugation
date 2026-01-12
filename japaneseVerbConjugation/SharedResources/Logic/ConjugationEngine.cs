using JapaneseVerbConjugation.Enums;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class ConjugationEngine
    {
        public static IReadOnlyList<string> Generate(
            string dictionaryForm,
            string reading,
            VerbGroup group,
            ConjugationForm form)
        {
            if (string.IsNullOrWhiteSpace(dictionaryForm))
                return [];

            dictionaryForm = dictionaryForm.Trim();
            reading = (reading ?? string.Empty).Trim();

            if (group == VerbGroup.Irregular)
                return GenerateIrregular(dictionaryForm, reading, form);

            var hasReading = !string.IsNullOrWhiteSpace(reading);

            return group switch
            {
                VerbGroup.Ichidan => GenerateIchidan(dictionaryForm, reading, hasReading, form),
                VerbGroup.Godan => GenerateGodan(dictionaryForm, reading, hasReading, form),
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
            ConjugationForm form)
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
                ConjugationForm.PresentPlain =>
                    Pack(dict, hasReading ? reading : null),

                ConjugationForm.PresentPolite =>
                    Pack(kanjiStem + "ます", hasReading ? kanaStem + "ます" : null),

                ConjugationForm.PastPlain =>
                    PastPlain(),

                ConjugationForm.PastPolite =>
                    Pack(kanjiStem + "ました", hasReading ? kanaStem + "ました" : null),

                ConjugationForm.NegativePlain =>
                    Pack(kanjiStem + "ない", hasReading ? kanaStem + "ない" : null),

                ConjugationForm.NegativePolite =>
                    Pack(kanjiStem + "ません", hasReading ? kanaStem + "ません" : null),

                ConjugationForm.TeForm =>
                    Pack(kanjiStem + "て", hasReading ? kanaStem + "て" : null),

                ConjugationForm.VolitionalPlain =>
                    Pack(kanjiStem + "よう", hasReading ? kanaStem + "よう" : null),

                ConjugationForm.VolitionalPolite =>
                    Pack(kanjiStem + "ましょう", hasReading ? kanaStem + "ましょう" : null),

                ConjugationForm.ConditionalBa =>
                    Pack(kanjiStem + "れば", hasReading ? kanaStem + "れば" : null),

                ConjugationForm.ConditionalTara =>
                    AppendSuffix(PastPlain(), "ら"),

                // Potential (Ichidan): standard ～られる, common alternate ～れる
                ConjugationForm.PotentialPlain =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "られる",
                        primaryKana: hasReading ? kanaStem + "られる" : null,
                        altKanji: kanjiStem + "れる",
                        altKana: hasReading ? kanaStem + "れる" : null),

                ConjugationForm.PotentialPolite =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "られます",
                        primaryKana: hasReading ? kanaStem + "られます" : null,
                        altKanji: kanjiStem + "れます",
                        altKana: hasReading ? kanaStem + "れます" : null),

                // Passive (Ichidan): same surface form as potential
                ConjugationForm.PassivePlain =>
                    Pack(kanjiStem + "られる", hasReading ? kanaStem + "られる" : null),

                ConjugationForm.PassivePolite =>
                    Pack(kanjiStem + "られます", hasReading ? kanaStem + "られます" : null),

                ConjugationForm.CausativePlain =>
                    Pack(kanjiStem + "させる", hasReading ? kanaStem + "させる" : null),

                ConjugationForm.CausativePolite =>
                    Pack(kanjiStem + "させます", hasReading ? kanaStem + "させます" : null),

                ConjugationForm.CausativePassivePlain =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "させられる",
                        primaryKana: hasReading ? kanaStem + "させられる" : null,
                        altKanji: kanjiStem + "させれる",
                        altKana: hasReading ? kanaStem + "させれる" : null),

                ConjugationForm.CausativePassivePolite =>
                    PackWithAlt(
                        primaryKanji: kanjiStem + "させられます",
                        primaryKana: hasReading ? kanaStem + "させられます" : null,
                        altKanji: kanjiStem + "させれます",
                        altKana: hasReading ? kanaStem + "させれます" : null),

                // Imperative: ～ろ is common; ～よ also exists (more formal/literary)
                ConjugationForm.Imperative =>
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
            ConjugationForm form)
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
                ConjugationForm.PresentPlain =>
                    Pack(dict, reading),

                ConjugationForm.PresentPolite =>
                    Pack(
                        kanjiStem + GodanRules.IStem(lastKana) + "ます",
                        kanaStem + GodanRules.IStem(lastKana) + "ます"
                    ),

                ConjugationForm.TeForm =>
                    Pack(
                        kanjiStem + teEnding,
                        kanaStem + teEnding
                    ),

                ConjugationForm.PastPlain =>
                    pastPlain,

                ConjugationForm.PastPolite =>
                    Pack(
                        kanjiStem + GodanRules.IStem(lastKana) + "ました",
                        kanaStem + GodanRules.IStem(lastKana) + "ました"
                    ),

                ConjugationForm.NegativePlain =>
                    Pack(
                        kanjiStem + GodanRules.AStem(lastKana) + "ない",
                        kanaStem + GodanRules.AStem(lastKana) + "ない"
                    ),

                ConjugationForm.NegativePolite =>
                    Pack(
                        kanjiStem + GodanRules.IStem(lastKana) + "ません",
                        kanaStem + GodanRules.IStem(lastKana) + "ません"
                    ),

                ConjugationForm.VolitionalPlain =>
                    Pack(kanjiStem + GodanRules.OStem(lastKana) + "う",
                         kanaStem + GodanRules.OStem(lastKana) + "う"),

                ConjugationForm.VolitionalPolite =>
                    Pack(kanjiStem + GodanRules.IStem(lastKana) + "ましょう",
                         kanaStem + GodanRules.IStem(lastKana) + "ましょう"),

                ConjugationForm.ConditionalBa =>
                    Pack(kanjiStem + GodanRules.EStem(lastKana) + "ば",
                         kanaStem + GodanRules.EStem(lastKana) + "ば"),

                ConjugationForm.ConditionalTara =>
                    AppendSuffix(pastPlain, "ら"),

                ConjugationForm.PotentialPlain =>
                    Pack(kanjiStem + GodanRules.EStem(lastKana) + "る",
                         kanaStem + GodanRules.EStem(lastKana) + "る"),

                ConjugationForm.PotentialPolite =>
                    Pack(kanjiStem + GodanRules.EStem(lastKana) + "ます",
                         kanaStem + GodanRules.EStem(lastKana) + "ます"),

                ConjugationForm.PassivePlain =>
                    Pack(kanjiStem + GodanRules.AStem(lastKana) + "れる",
                         kanaStem + GodanRules.AStem(lastKana) + "れる"),

                ConjugationForm.PassivePolite =>
                    Pack(kanjiStem + GodanRules.AStem(lastKana) + "れます",
                         kanaStem + GodanRules.AStem(lastKana) + "れます"),

                // Causative: a-stem + せる, but す => させる
                ConjugationForm.CausativePlain =>
                    Pack(kanjiStem + GodanCausativeBase(lastKana) + "る",
                         kanaStem + GodanCausativeBase(lastKana) + "る"),

                ConjugationForm.CausativePolite =>
                    Pack(kanjiStem + GodanCausativeBase(lastKana) + "ます",
                         kanaStem + GodanCausativeBase(lastKana) + "ます"),

                ConjugationForm.CausativePassivePlain =>
                    Pack(kanjiStem + GodanCausativeBase(lastKana) + "られる",
                         kanaStem + GodanCausativeBase(lastKana) + "られる"),

                ConjugationForm.CausativePassivePolite =>
                    Pack(kanjiStem + GodanCausativeBase(lastKana) + "られます",
                         kanaStem + GodanCausativeBase(lastKana) + "られます"),

                // Imperative: e-stem only (書け、読め)
                ConjugationForm.Imperative =>
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
            ConjugationForm form)
        {
            bool isSuru = dict is "する" or "為る";
            bool isKuru = dict is "来る" or "くる";

            if (!isSuru && !isKuru)
                return [];

            if (isSuru)
            {
                return form switch
                {
                    ConjugationForm.PresentPlain => new[] { "する" },
                    ConjugationForm.PresentPolite => new[] { "します" },

                    ConjugationForm.TeForm => new[] { "して" },
                    ConjugationForm.PastPlain => new[] { "した" },
                    ConjugationForm.PastPolite => new[] { "しました" },

                    ConjugationForm.NegativePlain => new[] { "しない" },
                    ConjugationForm.NegativePolite => new[] { "しません" },

                    ConjugationForm.VolitionalPlain => new[] { "しよう" },
                    ConjugationForm.VolitionalPolite => new[] { "しましょう" },

                    ConjugationForm.ConditionalBa => new[] { "すれば" },
                    ConjugationForm.ConditionalTara => new[] { "したら" },

                    ConjugationForm.PotentialPlain => new[] { "できる" },
                    ConjugationForm.PotentialPolite => new[] { "できます" },

                    ConjugationForm.PassivePlain => new[] { "される" },
                    ConjugationForm.PassivePolite => new[] { "されます" },

                    ConjugationForm.CausativePlain => new[] { "させる" },
                    ConjugationForm.CausativePolite => new[] { "させます" },

                    ConjugationForm.CausativePassivePlain => new[] { "させられる", "させれる" },
                    ConjugationForm.CausativePassivePolite => new[] { "させられます", "させれます" },

                    ConjugationForm.Imperative => new[] { "しろ", "せよ" },

                    _ => []
                };
            }

            // 来る / くる
            return form switch
            {
                ConjugationForm.PresentPlain => new[] { "くる", "来る" },
                ConjugationForm.PresentPolite => new[] { "きます", "来ます" },

                ConjugationForm.TeForm => new[] { "きて", "来て" },
                ConjugationForm.PastPlain => new[] { "きた", "来た" },
                ConjugationForm.PastPolite => new[] { "きました", "来ました" },

                ConjugationForm.NegativePlain => new[] { "こない", "来ない" },
                ConjugationForm.NegativePolite => new[] { "きません", "来ません" },

                ConjugationForm.VolitionalPlain => new[] { "こよう", "来よう" },
                ConjugationForm.VolitionalPolite => new[] { "きましょう", "来ましょう" },

                ConjugationForm.ConditionalBa => new[] { "くれば", "来れば" },
                ConjugationForm.ConditionalTara => new[] { "きたら", "来たら" },

                ConjugationForm.PotentialPlain => new[] { "こられる", "来られる" },
                ConjugationForm.PotentialPolite => new[] { "こられます", "来られます" },

                // Passive is same surface form for 来る
                ConjugationForm.PassivePlain => new[] { "こられる", "来られる" },
                ConjugationForm.PassivePolite => new[] { "こられます", "来られます" },

                ConjugationForm.CausativePlain => new[] { "こさせる", "来させる" },
                ConjugationForm.CausativePolite => new[] { "こさせます", "来させます" },

                ConjugationForm.CausativePassivePlain => new[] { "こさせられる", "来させられる" },
                ConjugationForm.CausativePassivePolite => new[] { "こさせられます", "来させられます" },

                ConjugationForm.Imperative => new[] { "こい", "来い" },

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
