using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Interfaces;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.DictionaryMethods;
using JapaneseVerbConjugation.SharedResources.Methods;
using System.Diagnostics;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class VerbImportService
    {
        private static readonly IJapaneseDictionary dictionary = JapaneseDictionaryProvider.Instance;

        public sealed class ImportResult
        {
            public int TotalRows { get; init; }
            public int AddedCount { get; init; }
            public int SkippedDuplicatesCount { get; init; }
            public int ErrorCount { get; init; }
            public List<string> SkippedDuplicates { get; init; } = [];
            public List<string> Errors { get; init; } = [];
        }

        public static ImportResult ImportFromDelimitedFile(
            string filePath,
            VerbStore store,
            Action<RowImportEvent>? onRow = null)
        {
            ArgumentNullException.ThrowIfNull(store);
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundException("Import file not found.", filePath);

            var dictionary = JapaneseDictionaryProvider.Instance;
            var result = new ImportResult();

            var lines = File.ReadAllLines(filePath)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            if (lines.Count == 0)
                return result;

            char delimiter = DetectDelimiter(lines[0]);

            // Header row (required)
            var header = SplitLine(lines[0], delimiter);
            var map = BuildHeaderMap(header);

            if (!map.ContainsKey("dictionaryform"))
            {
                return new ImportResult
                {
                    TotalRows = 0,
                    AddedCount = 0,
                    Errors = { "Missing required header: DictionaryForm" }
                };
            }

            // Prepare duplicate index (DictionaryForm-based)
            var existing = new HashSet<string>(
                store.Verbs.Select(v => StringNormalization.NormalizeKey(v.DictionaryForm ?? string.Empty)),
                StringComparer.Ordinal);

            // Calculate total verbs to attempt (excluding header)
            int totalVerbsToProcess = lines.Count - 1;
            onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, $"Attempting to import {totalVerbsToProcess} verbs from {Path.GetFileName(filePath)}"));

            int added = 0;
            int skippedDuplicates = 0;
            int skippedErrors = 0;
            int readingFailures = 0;
            int groupFailures = 0;
            int missingDictionaryForm = 0;
            int totalRows = 0;

            for (int i = 1; i < lines.Count; i++)
            {
                totalRows++;
                var cols = SplitLine(lines[i], delimiter);

                string dictRaw = GetCol(cols, map, "dictionaryform");
                string dict = StringNormalization.NormalizeKey(dictRaw);

                if (string.IsNullOrWhiteSpace(dict))
                {
                    missingDictionaryForm++;
                    skippedErrors++;
                    var msg = $"Row {i + 1}: missing DictionaryForm (skipped)";
                    result.Errors.Add(msg);
                    onRow?.Invoke(new RowImportEvent(RowStatus.Error, string.Empty, msg));
                    continue;
                }

                if (existing.Contains(dict))
                {
                    skippedDuplicates++;
                    result.SkippedDuplicates.Add(dict);
                    onRow?.Invoke(new RowImportEvent(RowStatus.Duplicate, dict, $"Duplicate skipped: {dict}"));
                    continue;
                }

                string groupRaw = GetCol(cols, map, "group");
                string meaning = GetCol(cols, map, "meaning");

                if (!dictionary.TryGetReading(dict, out var dictionaryReading))
                {
                    readingFailures++;
                    skippedErrors++;
                    var normalized = StringNormalization.NormalizeKey(dict);
                    var msg = $"Row {i + 1}: no dictionary reading found for '{dict}' (normalized: '{normalized}') (skipped)";
                    result.Errors.Add(msg);
                    Debug.WriteLine($"[IMPORT] Reading lookup failed for '{dict}' (normalized: '{normalized}')");
                    onRow?.Invoke(new RowImportEvent(RowStatus.Error, dict, msg));
                    continue;
                }

                // Verify we got a valid hiragana reading (should be non-empty and contain hiragana characters)
                if (string.IsNullOrWhiteSpace(dictionaryReading))
                {
                    readingFailures++;
                    skippedErrors++;
                    var msg = $"Row {i + 1}: empty reading returned for '{dict}' (skipped)";
                    result.Errors.Add(msg);
                    Debug.WriteLine($"[IMPORT] Empty reading for '{dict}'");
                    onRow?.Invoke(new RowImportEvent(RowStatus.Error, dict, msg));
                    continue;
                }

                // Verify the reading contains hiragana (JMdict kana field should be hiragana)
                // Hiragana range: \u3040-\u309F
                bool containsHiragana = dictionaryReading.Any(c => c >= '\u3040' && c <= '\u309F');
                bool containsKatakana = dictionaryReading.Any(c => c >= '\u30A0' && c <= '\u30FF');
                
                if (!containsHiragana && containsKatakana)
                {
                    Debug.WriteLine($"[IMPORT WARNING] '{dict}' has katakana reading '{dictionaryReading}' instead of hiragana - this is unusual");
                }
                else if (!containsHiragana && !containsKatakana)
                {
                    Debug.WriteLine($"[IMPORT WARNING] '{dict}' reading '{dictionaryReading}' contains no hiragana/katakana - may be kanji-only or invalid");
                }

                if (!dictionary.TryGetVerbGroup(dict, out var group))
                {
                    groupFailures++;
                    skippedErrors++;
                    var normalized = StringNormalization.NormalizeKey(dict);
                    var msg = $"Row {i + 1}: no dictionary verb group found for '{dict}' (normalized: '{normalized}') (skipped)";
                    result.Errors.Add(msg);
                    Debug.WriteLine($"[IMPORT] Group lookup failed for '{dict}' (normalized: '{normalized}')");
                    onRow?.Invoke(new RowImportEvent(RowStatus.Error, dict, msg));
                    continue;
                }

                var jlptLevel = filePath.ToLowerInvariant() switch
                {
                    var s when s.Contains("n5") => JLPTLevelEnum.N5,
                    var s when s.Contains("n4") => JLPTLevelEnum.N4,
                    _ => JLPTLevelEnum.NotSpecified,
                };

                var verb = new Verb
                {
                    DictionaryForm = dict,
                    Reading = dictionaryReading, // This should be hiragana (furigana) from JMdict kana field
                    Group = group, // VerbGroupEnum: Ichidan, Godan, or Irregular
                    Meaning = null,
                    JLPTLevel = jlptLevel,
                };

                // Log a sample of what we're saving for verification
                if (added < 5)
                {
                    Debug.WriteLine($"[IMPORT] Sample verb {added + 1}: '{dict}' -> reading: '{dictionaryReading}', group: {group}");
                }

                store.Verbs.Add(verb);
                existing.Add(dict);
                added++;

                onRow?.Invoke(new RowImportEvent(RowStatus.Added, dict, $"Added: {dict} ({dictionaryReading}, {group})"));
            }

            // Log summary to import window
            onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, "===== Import Summary ====="));
            onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, $"Total verbs to process: {totalVerbsToProcess}"));
            onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, $"Successfully added: {added}"));
            onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, $"Skipped (duplicates): {skippedDuplicates}"));
            onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, $"Skipped (errors): {skippedErrors}"));
            if (skippedErrors > 0)
            {
                onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, $"  - Missing DictionaryForm: {missingDictionaryForm}"));
                onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, $"  - Reading lookup failures: {readingFailures}"));
                onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, $"  - Group lookup failures: {groupFailures}"));
            }
            onRow?.Invoke(new RowImportEvent(RowStatus.Summary, string.Empty, "============================"));

            return new ImportResult
            {
                TotalRows = totalVerbsToProcess,
                AddedCount = added,
                SkippedDuplicatesCount = skippedDuplicates,
                ErrorCount = skippedErrors,
                SkippedDuplicates = result.SkippedDuplicates,
                Errors = result.Errors
            };
        }

        private static char DetectDelimiter(string headerLine)
        {
            // Prefer PSV if it looks like PSV
            if (headerLine.Contains('|')) return '|';
            return ','; // default CSV
        }

        private static Dictionary<string, int> BuildHeaderMap(string[] header)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < header.Length; i++)
            {
                var key = (header[i] ?? string.Empty).Trim().ToLowerInvariant();
                if (key.Length == 0) continue;
                map.TryAdd(key, i);
            }

            return map;
        }

        private static string GetCol(string[] cols, Dictionary<string, int> map, string key)
        {
            if (!map.TryGetValue(key, out var idx))
                return string.Empty;

            if (idx < 0 || idx >= cols.Length)
                return string.Empty;

            return cols[idx] ?? string.Empty;
        }

        // Simple splitter: supports quoted values in a basic way (enough for typical sheets)
        private static string[] SplitLine(string line, char delimiter)
        {
            var list = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Toggle quote state, handle escaped quotes ("")
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (!inQuotes && c == delimiter)
                {
                    list.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            list.Add(current.ToString());
            return [.. list];
        }

        // Note: NormalizeKey is now in StringNormalization utility class

        public enum RowStatus { Added, Duplicate, Error, Summary }

        public readonly record struct RowImportEvent(RowStatus Status, string DictionaryForm, string Message);
    }
}

