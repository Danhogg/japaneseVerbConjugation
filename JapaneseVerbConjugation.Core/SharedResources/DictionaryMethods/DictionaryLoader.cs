using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Interfaces;
using JapaneseVerbConjugation.SharedResources.Constants;
using JapaneseVerbConjugation.SharedResources.Logic;
using JapaneseVerbConjugation.SharedResources.Methods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace JapaneseVerbConjugation.SharedResources.DictionaryMethods
{
    public static class DictionaryLoader
    {
        public static IJapaneseDictionary LoadOrThrow()
        {
            var path = DataPathProvider.ResolveDataFile(DataFileConstants.DictionaryFileName);
            return LoadOrThrowFromPath(path);
        }

        public static IJapaneseDictionary LoadOrThrowFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"Dictionary file missing: {path}", path);

            // Read entire file into memory (decompress if needed)
            string jsonText;
            try
            {
                if (path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                {
                    using var fs = File.OpenRead(path);
                    using var gz = new GZipStream(fs, CompressionMode.Decompress);
                    using var reader = new StreamReader(gz);
                    jsonText = reader.ReadToEnd();
                }
                else
                {
                    jsonText = File.ReadAllText(path);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read dictionary file: {ex.Message}", ex);
            }

            // Parse entire JSON document with options to handle trailing commas and comments
            JsonDocument doc;
            try
            {
                var options = new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };
                doc = JsonDocument.Parse(jsonText, options);
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Failed to parse JSON dictionary file: {ex.Message}", ex);
            }

            using (doc)
            {
                var stats = BuildIndexesFromDocument(doc.RootElement, out var readings, out var groups);
                return new JapaneseDictionaryIndex(readings, groups);
            }
        }

        // ----------------------------
        // Core: parse "words" array from document, build indexes
        // ----------------------------
        private sealed class LoadStats
        {
            public int EntriesProcessed { get; set; }
            public int EntriesIndexed { get; set; }
            public int Collisions { get; set; }
        }

        private static LoadStats BuildIndexesFromDocument(
            JsonElement root,
            out Dictionary<string, string> readings,
            out Dictionary<string, VerbGroupEnum> groups)
        {
            readings = new Dictionary<string, string>(StringComparer.Ordinal);
            groups = new Dictionary<string, VerbGroupEnum>(StringComparer.Ordinal);
            var stats = new LoadStats();

            // Navigate to the "words" array
            if (!root.TryGetProperty("words", out var wordsArray))
            {
                throw new JsonException("JSON document does not contain a 'words' property.");
            }

            if (wordsArray.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException($"'words' property is not an array (found: {wordsArray.ValueKind}).");
            }

            // Process each entry in the words array
            foreach (var entry in wordsArray.EnumerateArray())
            {
                stats.EntriesProcessed++;
                if (IndexEntryIntoMaps(entry, readings, groups, stats))
                {
                    stats.EntriesIndexed++;
                }
            }

            return stats;
        }

        private static bool IndexEntryIntoMaps(
            JsonElement entry,
            Dictionary<string, string> readings,
            Dictionary<string, VerbGroupEnum> groups,
            LoadStats stats)
        {
            if (entry.ValueKind != JsonValueKind.Object)
                return false;

            // Less strict: id is optional (some entries might not have it)
            // We still want to process entries that have kana/kanji even without id
            bool hasId = entry.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.String;

            // Must have kana array to be processable
            if (!entry.TryGetProperty("kana", out var kanaArr) || kanaArr.ValueKind != JsonValueKind.Array)
                return false;

            var kanjiSpellings = GetTextsFromArrayOfObjects(entry, "kanji");
            var kanaCandidates = GetKanaCandidates(entry);

            // If no kana candidates, skip (but this should be rare)
            if (kanaCandidates.Count == 0)
                return false;

            var group = TryGetVerbGroupFromEntry(entry);

            bool indexed = false;

            // Kanji -> best kana
            foreach (var rawKanji in kanjiSpellings)
            {
                var kanji = StringNormalization.NormalizeKey(rawKanji);
                if (string.IsNullOrWhiteSpace(kanji))
                    continue;

                var bestKana = PickBestKanaForKanji(rawKanji, kanaCandidates);
                // Always use first available kana if best selection fails
                if (string.IsNullOrWhiteSpace(bestKana) && kanaCandidates.Count > 0)
                {
                    bestKana = kanaCandidates[0].Text;
                }

                var normalizedKana = StringNormalization.NormalizeKey(bestKana);
                if (!string.IsNullOrWhiteSpace(normalizedKana))
                {
                    if (!readings.TryAdd(kanji, normalizedKana))
                    {
                        stats.Collisions++;
                    }
                    indexed = true;
                }

                if (group.HasValue)
                {
                    if (!groups.TryAdd(kanji, group.Value))
                    {
                        stats.Collisions++;
                    }
                }
            }

            // Kana -> itself (also indexes kana-only headwords)
            foreach (var c in kanaCandidates)
            {
                var kana = StringNormalization.NormalizeKey(c.Text);
                if (string.IsNullOrWhiteSpace(kana))
                    continue;

                if (!readings.TryAdd(kana, kana))
                {
                    stats.Collisions++;
                }
                indexed = true;

                if (group.HasValue)
                {
                    if (!groups.TryAdd(kana, group.Value))
                    {
                        stats.Collisions++;
                    }
                }
            }

            return indexed;
        }

        // ----------------------------
        // Extract kanji/kana
        // ----------------------------
        private static List<string> GetTextsFromArrayOfObjects(JsonElement entry, string propName)
        {
            var list = new List<string>();

            if (!entry.TryGetProperty(propName, out var arr) || arr.ValueKind != JsonValueKind.Array)
                return list;

            // prefer common spellings first (stable indexing order)
            foreach (var obj in arr.EnumerateArray().OrderByDescending(o =>
                o.ValueKind == JsonValueKind.Object &&
                o.TryGetProperty("common", out var c) &&
                (c.ValueKind == JsonValueKind.True || c.ValueKind == JsonValueKind.False) &&
                c.GetBoolean()))
            {
                if (obj.ValueKind != JsonValueKind.Object)
                    continue;

                // STRICT: only accept the "text" property on these spelling objects
                if (!obj.TryGetProperty("text", out var textEl) || textEl.ValueKind != JsonValueKind.String)
                    continue;

                var textValue = textEl.GetString();
                if (string.IsNullOrWhiteSpace(textValue))
                    continue;

                var text = StringNormalization.NormalizeKey(textValue);
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                list.Add(text);
            }

            return list;
        }

        private sealed record KanaCandidate(string Text, bool Common, List<string> AppliesToKanji);

        private static List<KanaCandidate> GetKanaCandidates(JsonElement entry)
        {
            var list = new List<KanaCandidate>();

            if (!entry.TryGetProperty("kana", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return list;

            foreach (var obj in arr.EnumerateArray())
            {
                if (obj.ValueKind != JsonValueKind.Object)
                    continue;

                var text = obj.TryGetProperty("text", out var textEl) && textEl.ValueKind == JsonValueKind.String
                    ? textEl.GetString() ?? string.Empty
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var common = obj.TryGetProperty("common", out var commonEl) &&
                             (commonEl.ValueKind == JsonValueKind.True || commonEl.ValueKind == JsonValueKind.False) &&
                             commonEl.GetBoolean();

                var applies = new List<string>();
                if (obj.TryGetProperty("appliesToKanji", out var appliesEl) && appliesEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var a in appliesEl.EnumerateArray())
                    {
                        if (a.ValueKind == JsonValueKind.String)
                        {
                            var s = a.GetString();
                            if (!string.IsNullOrWhiteSpace(s))
                                applies.Add(s);
                        }
                    }
                }

                list.Add(new KanaCandidate(text, common, applies));
            }

            return list;
        }

        private static string PickBestKanaForKanji(string kanji, List<KanaCandidate> candidates)
        {
            if (candidates.Count == 0)
                return string.Empty;

            KanaCandidate? best = null;
            int bestScore = -1;

            foreach (var c in candidates)
            {
                bool appliesExact = c.AppliesToKanji.Any(x => string.Equals(x, kanji, StringComparison.Ordinal));
                bool appliesAll = c.AppliesToKanji.Any(x => x == "*");
                bool appliesEmpty = c.AppliesToKanji.Count == 0;

                int score =
                    (c.Common && appliesExact) ? 60 :
                    (c.Common && appliesAll) ? 55 :
                    (appliesExact) ? 50 :
                    (appliesAll) ? 45 :
                    (c.Common) ? 40 :
                    (appliesEmpty) ? 20 :
                    10;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }

            // Fallback: if no candidate scored, use the first common one, or just the first one
            if (best == null)
            {
                best = candidates.FirstOrDefault(c => c.Common) ?? candidates[0];
            }

            return best?.Text ?? string.Empty;
        }

        private static VerbGroupEnum? TryGetVerbGroupFromEntry(JsonElement entry)
        {
            if (!entry.TryGetProperty("sense", out var senses) || senses.ValueKind != JsonValueKind.Array)
                return null;

            bool sawVerbUnspec = false;
            bool sawVerb = false; // Track if we saw any verb-related tag

            foreach (var sense in senses.EnumerateArray())
            {
                if (sense.ValueKind != JsonValueKind.Object)
                    continue;

                if (!sense.TryGetProperty("partOfSpeech", out var pos) || pos.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var tagEl in pos.EnumerateArray())
                {
                    if (tagEl.ValueKind != JsonValueKind.String)
                        continue;

                    var t = tagEl.GetString();
                    if (string.IsNullOrWhiteSpace(t))
                        continue;

                    // Ichidan (v1 = ichidan verb, v1-s = ichidan verb - suru verb, vz = ichidan verb - zuru verb)
                    if (t == "v1" || t == "v1-s" || t == "vz")
                    {
                        sawVerb = true;
                        return VerbGroupEnum.Ichidan;
                    }

                    // Irregular (vk = kuru verb, vn = irregular nu verb, vs-* = suru verbs)
                    if (t == "vk" || t == "vn" || t.StartsWith("vs", StringComparison.Ordinal))
                    {
                        sawVerb = true;
                        return VerbGroupEnum.Irregular;
                    }

                    // Godan (v5 = godan verb, v5-* = specific godan endings)
                    if (t == "v5" || t.StartsWith("v5", StringComparison.Ordinal))
                    {
                        sawVerb = true;
                        return VerbGroupEnum.Godan;
                    }

                    // Sometimes verbs are marked as "v-unspec" (verb unspecified)
                    if (t == "v-unspec")
                    {
                        sawVerb = true;
                        sawVerbUnspec = true;
                    }

                    // Also check for vi (intransitive verb) and vt (transitive verb) as verb indicators
                    if (t == "vi" || t == "vt")
                    {
                        sawVerb = true;
                    }
                }
            }

            // Pragmatic fallback: if we saw "v-unspec", treat as Godan (most common)
            if (sawVerbUnspec)
                return VerbGroupEnum.Godan;

            // If we saw verb indicators (vi/vt) but no specific group, try heuristic fallback
            if (sawVerb)
            {
                // Try to infer from kanji/kana if available
                var kanjiSpellings = GetTextsFromArrayOfObjects(entry, "kanji");
                var kanaCandidates = GetKanaCandidates(entry);

                // Check if it ends in -する (suru) or -くる (kuru) - these are irregular
                foreach (var kana in kanaCandidates.Select(c => c.Text))
                {
                    if (kana.EndsWith("する", StringComparison.Ordinal) ||
                        kana.EndsWith("くる", StringComparison.Ordinal) ||
                        kana == "する" || kana == "くる" || kana == "来る")
                    {
                        return VerbGroupEnum.Irregular;
                    }
                }

                // Check if it ends in -iru or -eru (likely Ichidan, but could be Godan)
                // This is a heuristic - many Ichidan verbs end this way
                foreach (var kana in kanaCandidates.Select(c => c.Text))
                {
                    if (kana.Length >= 2)
                    {
                        var lastTwo = kana.Substring(kana.Length - 2);
                        if (lastTwo == "いる" || lastTwo == "える")
                        {
                            // Common Ichidan endings, but be cautious
                            // For now, default to Ichidan but this could be wrong for some Godan verbs
                            return VerbGroupEnum.Ichidan;
                        }
                    }
                }

                // Default fallback: if we saw verb indicators but can't determine, assume Godan
                return VerbGroupEnum.Godan;
            }

            return null;
        }
    }
}
