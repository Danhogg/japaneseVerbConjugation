using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests;

public sealed class VerbStateRestorerTests
{
    [Test]
    public void RestoreSavedAnswers_PopulatesEntryStates()
    {
        var verb = new Verb
        {
            DictionaryForm = "食べる",
            Reading = "たべる",
            Group = VerbGroupEnum.Ichidan
        };

        verb.Conjugations[ConjugationFormEnum.PresentPlain] = new ConjugationAnswer
        {
            Kanji = "食べる"
        };

        var entryStates = new List<ConjugationEntryState>
        {
            new()
            {
                ConjugationForm = ConjugationFormEnum.PresentPlain,
                Result = ConjugationResultEnum.Unchecked
            },
            new()
            {
                ConjugationForm = ConjugationFormEnum.PastPlain,
                Result = ConjugationResultEnum.Unchecked
            }
        };

        var expected = new Dictionary<ConjugationFormEnum, IReadOnlyList<string>>
        {
            [ConjugationFormEnum.PresentPlain] = ["食べる"],
            [ConjugationFormEnum.PastPlain] = ["食べた"]
        };

        var options = new AppOptions();

        VerbStateRestorer.RestoreSavedAnswers(verb, entryStates, expected, options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entryStates[0].UserInput, Is.EqualTo("食べる"));
            Assert.That(entryStates[0].Result, Is.EqualTo(ConjugationResultEnum.Correct));
            Assert.That(entryStates[1].UserInput, Is.Null.Or.Empty);
            Assert.That(entryStates[1].Result, Is.EqualTo(ConjugationResultEnum.Unchecked));
        }
    }

    [Test]
    public void RestoreVerbGroup_SetsExpectedState()
    {
        var verb = new Verb
        {
            DictionaryForm = "食べる",
            Reading = "たべる",
            Group = VerbGroupEnum.Ichidan,
            VerbGroupAnsweredCorrectly = true
        };

        VerbGroupEnum? selected = null;
        bool? isCorrect = null;
        bool locked = false;

        VerbStateRestorer.RestoreVerbGroup(
            verb,
            new AppOptions(),
            (sel, correct, lockSelection) =>
            {
                selected = sel;
                isCorrect = correct;
                locked = lockSelection;
            });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(selected, Is.EqualTo(VerbGroupEnum.Ichidan));
            Assert.That(isCorrect, Is.True);
            Assert.That(locked, Is.True);
        }
    }
}
