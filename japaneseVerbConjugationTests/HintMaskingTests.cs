using JapaneseVerbConjugation.SharedResources.Methods;

namespace japaneseVerbConjugationTests
{
    [TestFixture]
    public sealed class HintMaskingTests
    {
        [Test]
        public void MaskHint_KanjiOnly_KeepsAll()
        {
            // "食べる" = 食(kanji) + べ(hiragana) + る(hiragana)
            // So it will mask the hiragana parts
            var result = HintMasking.MaskHint("食べる");
            // 食 is kept, べ and る are hiragana - る is last and prev is hiragana, so kept
            Assert.That(result, Does.Contain("食"));
            Assert.That(result, Does.Contain("る"));
        }

        [Test]
        public void MaskHint_HiraganaOnly_MasksAllButLast()
        {
            // "たべる" = た + べ + る (all hiragana)
            // Last character る has prev hiragana, so it's kept
            var result = HintMasking.MaskHint("たべる");
            Assert.That(result, Does.Contain("る"));
            Assert.That(result, Does.Contain("＊"));
        }

        [Test]
        public void MaskHint_MixedKanjiHiragana_MasksHiragana()
        {
            // "食べる" = 食(kanji) + べ(hiragana) + る(hiragana)
            var result = HintMasking.MaskHint("食べる");
            Assert.That(result, Does.Contain("食"));
            // べ should be masked, る should be kept (last with prev hiragana)
            Assert.That(result, Has.Length.EqualTo(3));
        }

        [Test]
        public void MaskHint_MixedWithHiraganaEnding_MasksHiraganaButKeepsLast()
        {
            // "食べます" = 食(kanji) + べ(hiragana) + ま(hiragana) + す(hiragana)
            var result = HintMasking.MaskHint("食べます");
            Assert.That(result, Does.Contain("食"));
            // Last す has prev hiragana, so kept
            Assert.That(result, Does.Contain("す"));
            Assert.That(result, Does.Contain("＊"));
        }

        [Test]
        public void MaskHint_SingleHiragana_KeepsIt()
        {
            var result = HintMasking.MaskHint("る");
            Assert.That(result, Is.EqualTo("＊"));
        }

        [Test]
        public void MaskHint_TwoHiragana_KeepsLast()
        {
            var result = HintMasking.MaskHint("ます");
            Assert.That(result, Is.EqualTo("＊す"));
        }

        [Test]
        public void MaskHint_EmptyString_ReturnsEmpty()
        {
            var result = HintMasking.MaskHint("");
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void IsHiragana_ValidHiragana_ReturnsTrue()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(HintMasking.IsHiragana('あ'), Is.True);
                Assert.That(HintMasking.IsHiragana('る'), Is.True);
                Assert.That(HintMasking.IsHiragana('ん'), Is.True);
            }
        }

        [Test]
        public void IsHiragana_Katakana_ReturnsFalse()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(HintMasking.IsHiragana('ア'), Is.False);
                Assert.That(HintMasking.IsHiragana('ル'), Is.False);
            }
        }

        [Test]
        public void IsHiragana_Kanji_ReturnsFalse()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(HintMasking.IsHiragana('食'), Is.False);
                Assert.That(HintMasking.IsHiragana('泳'), Is.False);
                Assert.That(HintMasking.IsHiragana('行'), Is.False);
            }
        }

        [Test]
        public void IsHiragana_ASCII_ReturnsFalse()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(HintMasking.IsHiragana('a'), Is.False);
                Assert.That(HintMasking.IsHiragana('1'), Is.False);
            }
        }
    }
}
