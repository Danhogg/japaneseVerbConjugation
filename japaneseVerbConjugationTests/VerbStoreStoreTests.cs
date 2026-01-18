using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests;

public sealed class VerbStoreStoreTests
{
    [Test]
    public void CreateBackup_WhenStoreExists_CopiesFile()
    {
        using var scope = new TestStoreScope();
        var store = new VerbStore
        {
            Verbs =
            [
                new Verb
                {
                    DictionaryForm = "書く",
                    Reading = "かく",
                    Group = JapaneseVerbConjugation.Enums.VerbGroupEnum.Godan
                }
            ]
        };

        VerbStoreStore.Save(store);
        VerbStoreStore.CreateBackup();

        var dir = Path.GetDirectoryName(scope.StorePath)!;
        var backupPath = Path.Combine(dir, "verbs.backup.json");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(File.Exists(backupPath), Is.True);
            Assert.That(File.ReadAllText(backupPath), Is.EqualTo(File.ReadAllText(scope.StorePath)));
        }
    }

    [Test]
    public void CreateBackup_WhenStoreMissing_DoesNothing()
    {
        using var scope = new TestStoreScope();
        var dir = Path.GetDirectoryName(scope.StorePath)!;
        var backupPath = Path.Combine(dir, "verbs.backup.json");

        VerbStoreStore.CreateBackup();

        Assert.That(File.Exists(backupPath), Is.False);
    }
}
