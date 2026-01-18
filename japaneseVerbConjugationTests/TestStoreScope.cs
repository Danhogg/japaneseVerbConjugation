using JapaneseVerbConjugation.SharedResources.Logic;

namespace japaneseVerbConjugationTests;

public sealed class TestStoreScope : IDisposable
{
    private readonly string _tempDir;
    public string StorePath { get; }

    public TestStoreScope()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "jvc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        StorePath = Path.Combine(_tempDir, "verbs.json");

        VerbStoreStore.SetOverridePath(StorePath);
    }

    public void Dispose()
    {
        VerbStoreStore.SetOverridePath(null);

        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Best-effort cleanup for test temp folders.
        }
    }
}
