using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests;

public sealed class NoteSavePolicyTests
{
    [Test]
    public void Evaluate_NoExisting_NoInput_ReturnsNone()
    {
        var decision = NoteSavePolicy.Evaluate(null, null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(decision.Action, Is.EqualTo(NoteSaveAction.None));
            Assert.That(decision.NormalizedText, Is.Null);
        }
    }

    [Test]
    public void Evaluate_NoExisting_Whitespace_ReturnsNone()
    {
        var decision = NoteSavePolicy.Evaluate(null, "   ");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(decision.Action, Is.EqualTo(NoteSaveAction.None));
            Assert.That(decision.NormalizedText, Is.Null);
        }
    }

    [Test]
    public void Evaluate_HasExisting_Whitespace_ReturnsClear()
    {
        var decision = NoteSavePolicy.Evaluate("hello", "   ");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(decision.Action, Is.EqualTo(NoteSaveAction.Clear));
            Assert.That(decision.NormalizedText, Is.Null);
        }
    }

    [Test]
    public void Evaluate_HasExisting_Empty_ReturnsClear()
    {
        var decision = NoteSavePolicy.Evaluate("hello", "");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(decision.Action, Is.EqualTo(NoteSaveAction.Clear));
            Assert.That(decision.NormalizedText, Is.Null);
        }
    }

    [Test]
    public void Evaluate_NewInput_TrimsAndSaves()
    {
        var decision = NoteSavePolicy.Evaluate(null, "  new note  ");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(decision.Action, Is.EqualTo(NoteSaveAction.Save));
            Assert.That(decision.NormalizedText, Is.EqualTo("new note"));
        }
    }

    [Test]
    public void Evaluate_HasExisting_NewInput_Saves()
    {
        var decision = NoteSavePolicy.Evaluate("old", "updated");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(decision.Action, Is.EqualTo(NoteSaveAction.Save));
            Assert.That(decision.NormalizedText, Is.EqualTo("updated"));
        }
    }
}
