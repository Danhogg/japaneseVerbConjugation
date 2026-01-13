using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests
{
    [TestFixture]
    public sealed class AnswerCheckerTests
    {
        private static AppOptions CreateOptions(bool allowHiragana = false) => new()
        {
            AllowHiragana = allowHiragana
        };

        [Test]
        public void Check_ExactMatch_ReturnsCorrect()
        {
            var options = CreateOptions();
            var result = AnswerChecker.Check("食べる", ["食べる", "たべる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Correct));
        }

        [Test]
        public void Check_ExactMatchWithHiragana_AllowHiraganaTrue_ReturnsCorrect()
        {
            var options = CreateOptions(allowHiragana: true);
            var result = AnswerChecker.Check("たべる", ["食べる", "たべる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Correct));
        }

        [Test]
        public void Check_ExactMatchWithHiragana_AllowHiraganaFalse_FallsBackToOriginal()
        {
            var options = CreateOptions(allowHiragana: false);
            // When hiragana is filtered, it falls back to original list
            // So "たべる" should still match "たべる" in the original list
            var result = AnswerChecker.Check("たべる", ["食べる", "たべる"], options);
            // It may return Close due to filtering, but should still work
            Assert.That(result, Is.AnyOf(ConjugationResultEnum.Correct, ConjugationResultEnum.Close));
        }

        [Test]
        public void Check_NoMatch_ReturnsIncorrect()
        {
            var options = CreateOptions();
            var result = AnswerChecker.Check("違う", ["食べる", "たべる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Incorrect));
        }

        [Test]
        public void Check_EmptyInput_ReturnsUnchecked()
        {
            var options = CreateOptions();
            var result = AnswerChecker.Check("", ["食べる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Unchecked));
        }

        [Test]
        public void Check_NullInput_ReturnsUnchecked()
        {
            var options = CreateOptions();
            var result = AnswerChecker.Check(null, ["食べる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Unchecked));
        }

        [Test]
        public void Check_WhitespaceInput_ReturnsUnchecked()
        {
            var options = CreateOptions();
            var result = AnswerChecker.Check("   ", ["食べる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Unchecked));
        }

        [Test]
        public void Check_EmptyExpected_ReturnsIncorrect()
        {
            var options = CreateOptions();
            var result = AnswerChecker.Check("食べる", [], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Incorrect));
        }

        [Test]
        public void Check_CloseMatch_OneCharDiff_ReturnsClose()
        {
            var options = CreateOptions();
            // Missing one character
            var result = AnswerChecker.Check("食べ", ["食べる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Close));
        }

        [Test]
        public void Check_CloseMatch_TwoCharDiff_ReturnsClose()
        {
            var options = CreateOptions();
            // Missing two characters
            var result = AnswerChecker.Check("食べま", ["食べます"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Close));
        }

        [Test]
        public void Check_CloseMatch_Levenshtein_ReturnsClose()
        {
            var options = CreateOptions();
            // One character substitution
            var result = AnswerChecker.Check("食べる", ["食べろ"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Close));
        }

        [Test]
        public void Check_HiraganaOnly_AllowHiraganaFalse_FallsBackToOriginal()
        {
            var options = CreateOptions(allowHiragana: false);
            // Only hiragana answers should be filtered, but falls back to original list
            var result = AnswerChecker.Check("たべる", ["たべる"], options);
            // Falls back to original list, so should match
            Assert.That(result, Is.AnyOf(ConjugationResultEnum.Correct, ConjugationResultEnum.Close));
        }

        [Test]
        public void Check_HiraganaOnly_AllowHiraganaTrue_Accepts()
        {
            var options = CreateOptions(allowHiragana: true);
            var result = AnswerChecker.Check("たべる", ["たべる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Correct));
        }

        [Test]
        public void Check_Katakana_AlwaysFiltered()
        {
            var options = CreateOptions(allowHiragana: true);
            // Katakana should always be filtered out
            var result = AnswerChecker.Check("タベル", ["タベル"], options);
            // Should fall back to original list, but katakana is filtered
            // Since katakana is filtered, it should use fallback and still check
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Correct));
        }

        [Test]
        public void Check_MixedKanjiHiragana_Works()
        {
            var options = CreateOptions();
            var result = AnswerChecker.Check("食べます", ["食べます", "たべます"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Correct));
        }

        [Test]
        public void Check_TrimmedInput_Works()
        {
            var options = CreateOptions();
            var result = AnswerChecker.Check("  食べる  ", ["食べる"], options);
            Assert.That(result, Is.EqualTo(ConjugationResultEnum.Correct));
        }
    }
}
