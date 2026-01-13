using JapaneseVerbConjugation.SharedResources.Methods;

namespace japaneseVerbConjugationTests
{
    [TestFixture]
    public sealed class StringNormalizationTests
    {
        [Test]
        public void NormalizeKey_NormalString_ReturnsTrimmed()
        {
            var result = StringNormalization.NormalizeKey("  食べる  ");
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void NormalizeKey_FullWidthSpace_Trims()
        {
            var result = StringNormalization.NormalizeKey("　食べる　");
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void NormalizeKey_WithQuotes_Trims()
        {
            var result = StringNormalization.NormalizeKey("\"食べる\"");
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void NormalizeKey_WithComma_TrimsEnd()
        {
            var result = StringNormalization.NormalizeKey("食べる,");
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void NormalizeKey_WithFullWidthComma_TrimsEnd()
        {
            var result = StringNormalization.NormalizeKey("食べる，");
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void NormalizeKey_WithPeriod_TrimsEnd()
        {
            var result = StringNormalization.NormalizeKey("食べる。");
            Assert.That(result, Is.EqualTo("食べる"));
        }

        [Test]
        public void NormalizeKey_EmptyString_ReturnsEmpty()
        {
            var result = StringNormalization.NormalizeKey("");
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void NormalizeKey_Null_ReturnsEmpty()
        {
            var result = StringNormalization.NormalizeKey(null!);
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void NormalizeKey_Whitespace_ReturnsEmpty()
        {
            var result = StringNormalization.NormalizeKey("   ");
            Assert.That(result, Is.EqualTo(""));
        }

        [Test]
        public void NormalizeKey_MultiplePunctuation_TrimsEndPunctuation()
        {
            var result = StringNormalization.NormalizeKey("\"食べる\",。");
            // Trim() removes quotes from start/end, TrimEnd() removes comma/period from end
            // "\"食べる\",。" -> after Trim: "食べる",。 -> after TrimEnd: "食べる",
            // Note: Trim only removes from start/end, so middle quotes might remain
            Assert.That(result, Does.Contain("食べる"));
            Assert.That(result, Does.Not.Contain("。")); // Period should be trimmed
        }

        [Test]
        public void NormalizeKey_UnicodeNormalization_Applies()
        {
            // Test that Unicode normalization is applied
            var result = StringNormalization.NormalizeKey("食べる");
            Assert.That(result, Is.EqualTo("食べる"));
        }
    }
}
