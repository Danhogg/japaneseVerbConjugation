using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests
{
    [TestFixture]
    public sealed class ConjugationAnswerPickerTests
    {
        [Test]
        public void PickCanonical_WithKanji_PrefersKanji()
        {
            var result = ConjugationAnswerPicker.PickCanonical(["食べる", "たべる"]);
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void PickCanonical_OnlyHiragana_ReturnsFirst()
        {
            var result = ConjugationAnswerPicker.PickCanonical(["たべる", "たべます"]);
            Assert.That(result, Is.EqualTo("たべる"));
        }

        [Test]
        public void PickCanonical_MultipleKanji_PicksFirstKanji()
        {
            var result = ConjugationAnswerPicker.PickCanonical(["食べる", "食べます", "たべる"]);
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void PickCanonical_EmptyList_ReturnsEmpty()
        {
            var result = ConjugationAnswerPicker.PickCanonical([]);
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void PickCanonical_Null_ReturnsEmpty()
        {
            var result = ConjugationAnswerPicker.PickCanonical(null!);
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void PickCanonical_SingleItem_ReturnsThat()
        {
            var result = ConjugationAnswerPicker.PickCanonical(["する"]);
            Assert.That(result, Is.EqualTo("する"));
        }

        [Test]
        public void PickCanonical_KanjiNotFirst_PicksKanji()
        {
            var result = ConjugationAnswerPicker.PickCanonical(["たべる", "食べる"]);
            Assert.That(result, Is.EqualTo("食べる"));
        }
    }
}
