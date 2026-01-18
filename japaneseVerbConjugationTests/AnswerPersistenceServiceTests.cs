using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests;

public sealed class AnswerPersistenceServiceTests
{
    [Test]
    public void PersistAnswer_Correct_SavesCanonical()
    {
        var verb = CreateVerb();
        var store = new VerbStore { Verbs = [verb] };
        var expected = new List<string> { "書く" };

        AnswerPersistenceService.PersistAnswer(
            verb,
            ConjugationFormEnum.PresentPlain,
            "かく",
            expected,
            ConjugationResultEnum.Correct,
            new AppOptions(),
            store);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(verb.Conjugations.ContainsKey(ConjugationFormEnum.PresentPlain), Is.True);
            Assert.That(verb.Conjugations[ConjugationFormEnum.PresentPlain].Kanji, Is.EqualTo("書く"));
        }
    }

    [Test]
    public void PersistAnswer_Incorrect_SavesUserInput()
    {
        var verb = CreateVerb();
        var store = new VerbStore { Verbs = [verb] };

        AnswerPersistenceService.PersistAnswer(
            verb,
            ConjugationFormEnum.PastPlain,
            "書いた",
            ["書いた"],
            ConjugationResultEnum.Incorrect,
            new AppOptions(),
            store);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(verb.Conjugations.ContainsKey(ConjugationFormEnum.PastPlain), Is.True);
            Assert.That(verb.Conjugations[ConjugationFormEnum.PastPlain].Kanji, Is.EqualTo("書いた"));
        }
    }

    [Test]
    public void PersistAnswer_Whitespace_DoesNotSave()
    {
        var verb = CreateVerb();
        var store = new VerbStore { Verbs = [verb] };

        AnswerPersistenceService.PersistAnswer(
            verb,
            ConjugationFormEnum.TeForm,
            "   ",
            ["書いて"],
            ConjugationResultEnum.Incorrect,
            new AppOptions(),
            store);

        Assert.That(verb.Conjugations.ContainsKey(ConjugationFormEnum.TeForm), Is.False);
    }

    private static Verb CreateVerb() => new()
    {
        DictionaryForm = "書く",
        Reading = "かく",
        Group = VerbGroupEnum.Godan
    };
}
