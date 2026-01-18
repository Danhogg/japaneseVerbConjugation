using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests;

public sealed class DataPathProviderTests
{
    [Test]
    public void ResolveDataFile_UsesCustomRoot()
    {
        var tempDir = CreateTempDir();

        DataPathProvider.SetDataRoot(tempDir);
        try
        {
            var path = DataPathProvider.ResolveDataFile("verbs.csv");

            Assert.That(path, Is.EqualTo(Path.Combine(tempDir, "verbs.csv")));
        }
        finally
        {
            DataPathProvider.SetDataRoot(null);
        }
    }

    [Test]
    public void SetDataRootIfExists_ReturnsExpected()
    {
        var tempDir = CreateTempDir();
        var missingDir = Path.Combine(tempDir, "missing");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(DataPathProvider.SetDataRootIfExists(missingDir), Is.False);
            Assert.That(DataPathProvider.SetDataRootIfExists(tempDir), Is.True);
        }

        DataPathProvider.SetDataRoot(null);
    }

    [Test]
    public void SetDataRootFromCandidates_UsesFirstExisting()
    {
        var tempDir = CreateTempDir();
        var altDir = CreateTempDir();

        var result = DataPathProvider.SetDataRootFromCandidates("not-real", tempDir, altDir);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.True);
            Assert.That(DataPathProvider.GetDataRoot(), Is.EqualTo(Path.GetFullPath(tempDir)));
        }

        DataPathProvider.SetDataRoot(null);
    }

    [Test]
    public void TryResolveDataFile_ReturnsNullIfMissing()
    {
        var tempDir = CreateTempDir();

        DataPathProvider.SetDataRoot(tempDir);
        try
        {
            var path = DataPathProvider.TryResolveDataFile("missing.csv");

            Assert.That(path, Is.Null);
        }
        finally
        {
            DataPathProvider.SetDataRoot(null);
        }
    }

    [Test]
    public void TryResolveDataFile_ReturnsPathIfExists()
    {
        var tempDir = CreateTempDir();
        var filePath = Path.Combine(tempDir, "exists.csv");
        File.WriteAllText(filePath, "data");

        DataPathProvider.SetDataRoot(tempDir);
        try
        {
            var path = DataPathProvider.TryResolveDataFile("exists.csv");

            Assert.That(path, Is.EqualTo(filePath));
        }
        finally
        {
            DataPathProvider.SetDataRoot(null);
        }
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "jvc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
