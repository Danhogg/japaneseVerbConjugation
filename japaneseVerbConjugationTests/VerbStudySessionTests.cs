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
        WithStoreBackup(() =>
        {
            var session = CreateSessionWithVerb(out var verb);

            var result = session.SetFavorite(true);

            var persisted = LoadPersistedStore();
            var savedVerb = persisted.Verbs.Single(v => v.Id == verb.Id);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(verb.IsFavorite, Is.True);
                Assert.That(savedVerb.IsFavorite, Is.True);
            }
        });
    }

    [Test]
    public void SetNotes_WhitespaceClearsExistingAndPersists()
    {
        WithStoreBackup(() =>
        {
            var session = CreateSessionWithVerb(out var verb);
            session.SetNotes("Existing note");

            var result = session.SetNotes("   ");

            var persisted = LoadPersistedStore();
            var savedVerb = persisted.Verbs.Single(v => v.Id == verb.Id);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Null);
                Assert.That(verb.UserNotes, Is.Null);
                Assert.That(savedVerb.UserNotes, Is.Null);
            }
        });
    }

    [Test]
    public void SetNotes_NewText_SavesTrimmedAndReturnsTimestamp()
    {
        WithStoreBackup(() =>
        {
            var session = CreateSessionWithVerb(out var verb);

            var result = session.SetNotes("  New note  ");

            var persisted = LoadPersistedStore();
            var savedVerb = persisted.Verbs.Single(v => v.Id == verb.Id);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(verb.UserNotes?.Text, Is.EqualTo("New note"));
                Assert.That(savedVerb.UserNotes?.Text, Is.EqualTo("New note"));
            }
        });
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

    private static void WithStoreBackup(Action action)
    {
        var path = GetStorePath();
        var existed = File.Exists(path);
        var backup = existed ? File.ReadAllText(path) : null;

        try
        {
            action();
        }
        finally
        {
            if (existed)
            {
                File.WriteAllText(path, backup!);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static VerbStore LoadPersistedStore()
    {
        var path = GetStorePath();
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<VerbStore>(json)!;
    }

    private static string GetStorePath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "JapaneseVerbConjugation");
        return Path.Combine(dir, "verbs.json");
    }
}
