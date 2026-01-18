using System.Reflection;
using System.Text.Json;
using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests;

[NonParallelizable]
public sealed class VerbStudySessionTests
{
    [Test]
    public void SetFavorite_UpdatesVerbAndPersists()
    {
        using var scope = new TestStoreScope();
        {
            var session = CreateSessionWithVerb(out var verb);

            var result = session.SetFavorite(true);

            var persisted = LoadPersistedStore(scope.StorePath);
            var savedVerb = persisted.Verbs.Single(v => v.Id == verb.Id);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(verb.IsFavorite, Is.True);
                Assert.That(savedVerb.IsFavorite, Is.True);
            }
        }
    }

    [Test]
    public void SetNotes_WhitespaceClearsExistingAndPersists()
    {
        using var scope = new TestStoreScope();
        {
            var session = CreateSessionWithVerb(out var verb);
            session.SetNotes("Existing note");

            var result = session.SetNotes("   ");

            var persisted = LoadPersistedStore(scope.StorePath);
            var savedVerb = persisted.Verbs.Single(v => v.Id == verb.Id);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);
                Assert.That(verb.UserNotes, Is.Null);
                Assert.That(savedVerb.UserNotes, Is.Null);
            }
        }
    }

    [Test]
    public void SetNotes_NewText_SavesTrimmedAndReturnsTimestamp()
    {
        using var scope = new TestStoreScope();
        {
            var session = CreateSessionWithVerb(out var verb);

            var result = session.SetNotes("  New note  ");

            var persisted = LoadPersistedStore(scope.StorePath);
            var savedVerb = persisted.Verbs.Single(v => v.Id == verb.Id);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(verb.UserNotes?.Text, Is.EqualTo("New note"));
                Assert.That(savedVerb.UserNotes?.Text, Is.EqualTo("New note"));
            }
        }
    }

    private static VerbStudySession CreateSessionWithVerb(out Verb verb)
    {
        verb = new Verb
        {
            DictionaryForm = "書く",
            Reading = "かく",
            Group = VerbGroupEnum.Godan
        };

        var options = new AppOptions();
        var store = new VerbStore { Verbs = [verb] };

        var session = CreateSession(options, store);

        session.LoadVerb(verb, new List<ConjugationEntryState>
        {
            new() { ConjugationForm = ConjugationFormEnum.PresentPlain }
        });

        return session;
    }

    private static VerbStudySession CreateSession(AppOptions options, VerbStore store)
    {
        var ctor = typeof(VerbStudySession).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(AppOptions), typeof(VerbStore)],
            modifiers: null);

        return (VerbStudySession)ctor!.Invoke([options, store]);
    }

    private static VerbStore LoadPersistedStore(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<VerbStore>(json)!;
    }
}
