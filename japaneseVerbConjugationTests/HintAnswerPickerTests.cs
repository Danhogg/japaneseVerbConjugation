using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests
{
    [TestFixture]
    public sealed class HintAnswerPickerTests
    {
        [Test]
        public void PickBestForHint_WithKanji_PrefersKanji()
        {
            var result = HintAnswerPicker.PickBestForHint(["食べる", "たべる"]);
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void PickBestForHint_OnlyHiragana_ReturnsFirst()
        {
            var result = HintAnswerPicker.PickBestForHint(["たべる", "たべます"]);
            Assert.That(result, Is.EqualTo("たべる"));
        }

        [Test]
        public void PickBestForHint_KanjiNotFirst_PicksKanji()
        {
            var result = HintAnswerPicker.PickBestForHint(["たべる", "食べる"]);
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void PickBestForHint_EmptyList_ReturnsEmpty()
        {
            var result = HintAnswerPicker.PickBestForHint([]);
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void PickBestForHint_Null_ReturnsEmpty()
        {
            var result = HintAnswerPicker.PickBestForHint(null!);
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void PickBestForHint_WithEmptyStrings_SkipsThem()
        {
            var result = HintAnswerPicker.PickBestForHint(["", "   ", "食べる"]);
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void PickBestForHint_AllEmpty_ReturnsEmpty()
        {
            var result = HintAnswerPicker.PickBestForHint(["", "   "]);
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void PickBestForHint_SingleItem_ReturnsThat()
        {
            var result = HintAnswerPicker.PickBestForHint(["する"]);
            Assert.That(result, Is.EqualTo("する"));
        }
    }
}
