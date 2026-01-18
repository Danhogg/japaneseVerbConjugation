using JapaneseVerbConjugation.SharedResources.DictionaryMethods;
using System.IO.Compression;
using System.Text;

namespace japaneseVerbConjugationTests
{
    [TestFixture]
    public sealed class DictionaryLoaderTests
    {
        [Test]
        public void LoadOrThrowFromPath_Json_LoadsReadingByKanji()
        {
            // Arrange
            var tempDir = CreateTempDir();
            try
            {
                var path = Path.Combine(tempDir, "dict.json");
                File.WriteAllText(path, SampleJson());

                // Act
                var dict = DictionaryLoader.LoadOrThrowFromPath(path);

                using (Assert.EnterMultipleScope())
                {
                    // Assert
                    Assert.That(dict.TryGetReading("食べる", out var reading1), Is.True);
                    Assert.That(reading1, Is.EqualTo("たべる"));

                    Assert.That(dict.TryGetReading("泳ぐ", out var reading2), Is.True);
                    Assert.That(reading2, Is.EqualTo("およぐ"));

                    Assert.That(dict.TryGetReading("不存在", out _), Is.False);
                }
            }
            finally
            {
                CleanupTempDir(tempDir);
            }
        }

        [Test]
        public void LoadOrThrowFromPath_GzJson_LoadsReadingByKanji()
        {
            // Arrange
            var tempDir = CreateTempDir();
            try
            {
                var path = Path.Combine(tempDir, "dict.json.gz");
                WriteGz(path, SampleJson());

                // Act
                var dict = DictionaryLoader.LoadOrThrowFromPath(path);

                using (Assert.EnterMultipleScope())
                {
                    // Assert
                    Assert.That(dict.TryGetReading("食べる", out var reading), Is.True);
                    Assert.That(reading, Is.EqualTo("たべる"));

                    Assert.That(dict.TryGetVerbGroup("食べる", out var group1), Is.True);
                    Assert.That(group1, Is.EqualTo(JapaneseVerbConjugation.Enums.VerbGroupEnum.Ichidan));
                }
            }
            finally
            {
                CleanupTempDir(tempDir);
            }
        }

        [Test]
        public void LoadOrThrowFromPath_MissingFile_Throws()
        {
            var tempDir = CreateTempDir();
            try
            {
                var missing = Path.Combine(tempDir, "nope.json");

                var ex = Assert.Throws<FileNotFoundException>(() =>
                    DictionaryLoader.LoadOrThrowFromPath(missing));

                Assert.That(ex!.Message, Does.Contain("Dictionary file missing"));
            }
            finally
            {
                CleanupTempDir(tempDir);
            }
        }

        private static string CreateTempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), "jvc-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static void CleanupTempDir(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch
            {
                // Best-effort cleanup for test temp folders.
            }
        }

        private static void WriteGz(string path, string content)
        {
            using var fs = File.Create(path);
            using var gz = new GZipStream(fs, CompressionMode.Compress);
            using var sw = new StreamWriter(gz, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            sw.Write(content);
        }

        private static string SampleJson() =>
            // Matches the real JMdict JSON structure with words array
            """
            {
              "words": [
                {
                  "id": "1234567",
                  "kanji": [
                    { "text": "食べる", "common": true }
                  ],
                  "kana": [
                    { "text": "たべる", "common": true, "appliesToKanji": ["食べる"] }
                  ],
                  "sense": [
                    {
                      "partOfSpeech": ["v1"]
                    }
                  ]
                },
                {
                  "id": "1234568",
                  "kanji": [
                    { "text": "泳ぐ", "common": true }
                  ],
                  "kana": [
                    { "text": "およぐ", "common": true, "appliesToKanji": ["泳ぐ"] }
                  ],
                  "sense": [
                    {
                      "partOfSpeech": ["v5g"]
                    }
                  ]
                }
              ]
            }
            """;
    }
}
