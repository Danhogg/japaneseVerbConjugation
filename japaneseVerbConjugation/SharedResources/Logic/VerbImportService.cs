using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public static class VerbImportService
    {
        public sealed class ImportResult
        {
            public int TotalRows { get; init; }
            public int AddedCount { get; init; }
            public List<string> SkippedDuplicates { get; init; } = [];
            public List<string> Errors { get; init; } = [];
        }

        public static ImportResult ImportFromDelimitedFile(string filePath, VerbStore store)
        {
            if (store is null) throw new ArgumentNullException(nameof(store));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundException("Import file not found.", filePath);

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
                store.Verbs.Select(v => (v.DictionaryForm ?? string.Empty).Trim()),
                StringComparer.Ordinal);

            int added = 0;
            int totalRows = 0;

            for (int i = 1; i < lines.Count; i++)
            {
                totalRows++;
                var cols = SplitLine(lines[i], delimiter);

                string dict = GetCol(cols, map, "dictionaryform").Trim();
                if (string.IsNullOrWhiteSpace(dict))
                {
                    // skip blank required
                    continue;
                }

                if (existing.Contains(dict))
                {
                    result.SkippedDuplicates.Add(dict);
                    continue;
                }

                string reading = GetCol(cols, map, "reading").Trim();
                string groupRaw = GetCol(cols, map, "group").Trim();
                string meaning = GetCol(cols, map, "meaning").Trim();

                // Group is recommended. If missing, default to Godan (you can choose to error instead).
                VerbGroupEnum group;
                try
                {
                    group = ParseVerbGroup(groupRaw, defaultIfMissing: VerbGroupEnum.Godan);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {i + 1}: invalid Group '{groupRaw}' ({ex.Message})");
                    continue;
                }

                // You said you'll ensure kana-only is sensible, so we won't do ambiguity checks now.

                var jlptLevel = filePath.ToLowerInvariant() switch
                {
                    var s when s.Contains("n5") => JLPTLevelEnum.N5,
                    var s when s.Contains("n4") => JLPTLevelEnum.N4,
                    var s when s.Contains("n3") => JLPTLevelEnum.N3,
                    var s when s.Contains("n2") => JLPTLevelEnum.N2,
                    var s when s.Contains("n1") => JLPTLevelEnum.N1,
                    _ => JLPTLevelEnum.NotSpecified,
                };
                var verb = new Verb
                {
                    DictionaryForm = dict,
                    Reading = string.IsNullOrWhiteSpace(reading) ? dict : reading, // safe fallback (better: require)
                    Group = group,
                    Meaning = string.IsNullOrWhiteSpace(meaning) ? null : meaning,
                    JLPTLevel = jlptLevel,
                };

                store.Verbs.Add(verb);
                existing.Add(dict);
                added++;
            }

            return new ImportResult
            {
                TotalRows = totalRows,
                AddedCount = added,
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
                if (!map.ContainsKey(key))
                    map.Add(key, i);
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
            return list.ToArray();
        }

        private static VerbGroupEnum ParseVerbGroup(string raw, VerbGroupEnum defaultIfMissing)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return defaultIfMissing;

            var s = raw.Trim().ToLowerInvariant();

            // Accept your UI inputs + english + numbers
            if (s is "godan" or "5" or "group 5" or "group5" or "五段") return VerbGroupEnum.Godan;
            if (s is "ichidan" or "1" or "group 1" or "group1" or "一段") return VerbGroupEnum.Ichidan;
            if (s is "irregular" or "3" or "group 3" or "group3" or "不規則") return VerbGroupEnum.Irregular;

            throw new FormatException("Expected Godan/Ichidan/Irregular (or 5/1/3).");
        }
    }
}

