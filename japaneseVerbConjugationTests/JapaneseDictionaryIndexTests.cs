using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.SharedResources.DictionaryMethods;

namespace japaneseVerbConjugationTests;

public sealed class JapaneseDictionaryIndexTests
{
    [Test]
    public void TryGetReading_ExactMatch_ReturnsReading()
    {
        var dict = CreateDictionary();

        var result = dict.TryGetReading("食べる", out var reading);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(reading, Is.EqualTo("たべる"));
        }
    }

    [Test]
    public void TryGetVerbGroup_ExactMatch_ReturnsGroup()
    {
        var dict = CreateDictionary();

        var result = dict.TryGetVerbGroup("食べる", out var group);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(group, Is.EqualTo(VerbGroupEnum.Ichidan));
        }
    }

    [Test]
    public void TryGetReading_SuruFallback_ReturnsReading()
    {
        var dict = CreateDictionary();

        var result = dict.TryGetReading("安心する", out var reading);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(reading, Is.EqualTo("あんしんする"));
        }
    }

    [Test]
    public void TryGetVerbGroup_SuruFallback_ReturnsIrregular()
    {
        var dict = CreateDictionary();

        var result = dict.TryGetVerbGroup("安心する", out var group);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(group, Is.EqualTo(VerbGroupEnum.Irregular));
        }
    }

    private static JapaneseDictionaryIndex CreateDictionary()
    {
        var readings = new Dictionary<string, string>
        {
            ["食べる"] = "たべる",
            ["安心"] = "あんしん"
        };

        var groups = new Dictionary<string, VerbGroupEnum>
        {
            ["食べる"] = VerbGroupEnum.Ichidan
        };

        return new JapaneseDictionaryIndex(readings, groups);
    }
}
