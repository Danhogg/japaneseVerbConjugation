using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests
{
    [TestFixture]
    public sealed class ConjugationEngineTests
    {
        private static IReadOnlyList<string> Gen(string dict, string reading, VerbGroupEnum group, ConjugationFormEnum form)
            => ConjugationEngine.Generate(dict, reading, group, form);

        private static void AssertContainsOneOf(IReadOnlyList<string> actual, params string[] expectedAny)
        {
            foreach (var e in expectedAny)
            {
                if (actual.Contains(e))
                    return;
            }

            Assert.Fail($"Expected one of: [{string.Join(", ", expectedAny)}] but got: [{string.Join(", ", actual)}]");
        }

        // --------------------
        // Ichidan (食べる)
        // --------------------
        [Test]
        public void Ichidan_Taberu_CoreForms()
        {
            const string dict = "食べる";
            const string reading = "たべる";

            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.PresentPlain), "食べる", "たべる");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.PresentPolite), "食べます", "たべます");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.TeForm), "食べて", "たべて");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.PastPlain), "食べた", "たべた");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.NegativePlain), "食べない", "たべない");

            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.VolitionalPlain), "食べよう", "たべよう");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.ConditionalBa), "食べれば", "たべれば");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.ConditionalTara), "食べたら", "たべたら");

            // Potential accepts standard + common alternate
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.PotentialPlain),
                "食べられる", "たべられる", "食べれる", "たべれる");

            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.PassivePlain), "食べられる", "たべられる");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.CausativePlain), "食べさせる", "たべさせる");

            // Causative-passive includes common alternate
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.CausativePassivePlain),
                "食べさせられる", "たべさせられる", "食べさせれる", "たべさせれる");

            // Imperative accepts ろ and よ
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Ichidan, ConjugationFormEnum.Imperative),
                "食べろ", "たべろ", "食べよ", "たべよ");
        }

        // --------------------
        // Irregular (する / 来る)
        // --------------------
        [Test]
        public void Irregular_Suru_AllKeyForms()
        {
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.PresentPlain), "する");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.PresentPolite), "します");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.TeForm), "して");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.PastPlain), "した");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.NegativePlain), "しない");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.VolitionalPlain), "しよう");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.ConditionalBa), "すれば");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.ConditionalTara), "したら");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.PotentialPlain), "できる");
            AssertContainsOneOf(Gen("する", "する", VerbGroupEnum.Irregular, ConjugationFormEnum.Imperative), "しろ", "せよ");
        }

        [Test]
        public void Irregular_Kuru_AllKeyForms()
        {
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.PresentPlain), "来る", "くる");
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.PresentPolite), "来ます", "きます");
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.TeForm), "来て", "きて");
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.PastPlain), "来た", "きた");
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.NegativePlain), "来ない", "こない");
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.VolitionalPlain), "来よう", "こよう");
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.ConditionalBa), "来れば", "くれば");
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.ConditionalTara), "来たら", "きたら");
            AssertContainsOneOf(Gen("来る", "くる", VerbGroupEnum.Irregular, ConjugationFormEnum.Imperative), "来い", "こい");
        }

        // --------------------
        // Special case: いく => いって / いった
        // --------------------
        [Test]
        public void Godan_Iku_SpecialCase_TeAndPast()
        {
            const string dict = "行く";
            const string reading = "いく";

            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.TeForm), "行って", "いって");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.PastPlain), "行った", "いった");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.ConditionalTara), "行ったら", "いったら");
        }

        // --------------------
        // Godan endings coverage
        // --------------------
        [TestCase("買う", "かう", "買って", "買った", "買わない", "買います", "買えば", "買おう", "買える", "買え", "買わせる")]
        [TestCase("待つ", "まつ", "待って", "待った", "待たない", "待ちます", "待てば", "待とう", "待てる", "待て", "待たせる")]
        [TestCase("取る", "とる", "取って", "取った", "取らない", "取ります", "取れば", "取ろう", "取れる", "取れ", "取らせる")]
        [TestCase("読む", "よむ", "読んで", "読んだ", "読まない", "読みます", "読めば", "読もう", "読める", "読め", "読ませる")]
        [TestCase("遊ぶ", "あそぶ", "遊んで", "遊んだ", "遊ばない", "遊びます", "遊べば", "遊ぼう", "遊べる", "遊べ", "遊ばせる")]
        [TestCase("死ぬ", "しぬ", "死んで", "死んだ", "死なない", "死にます", "死ねば", "死のう", "死ねる", "死ね", "死なせる")]
        [TestCase("書く", "かく", "書いて", "書いた", "書かない", "書きます", "書けば", "書こう", "書ける", "書け", "書かせる")]
        [TestCase("泳ぐ", "およぐ", "泳いで", "泳いだ", "泳がない", "泳ぎます", "泳げば", "泳ごう", "泳げる", "泳げ", "泳がせる")]
        [TestCase("話す", "はなす", "話して", "話した", "話さない", "話します", "話せば", "話そう", "話せる", "話せ", "話させる")]
        public void Godan_Endings_AreCorrect(
            string dict,
            string reading,
            string te,
            string past,
            string neg,
            string polite,
            string ba,
            string volitional,
            string potential,
            string imperative,
            string causative)
        {
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.TeForm), te);
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.PastPlain), past);
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.NegativePlain), neg);
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.PresentPolite), polite);
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.ConditionalBa), ba);
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.VolitionalPlain), volitional);
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.PotentialPlain), potential);
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.Imperative), imperative);
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.CausativePlain), causative);
        }

        [Test]
        public void Godan_Oyogu_PassiveAndCausativePassive()
        {
            const string dict = "泳ぐ";
            const string reading = "およぐ";

            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.PassivePlain), "泳がれる", "およがれる");
            AssertContainsOneOf(Gen(dict, reading, VerbGroupEnum.Godan, ConjugationFormEnum.CausativePassivePlain), "泳がせられる", "およがせられる");
        }
    }
}