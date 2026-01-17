using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    /// <summary>
    /// Service for restoring verb state from persistent storage.
    /// </summary>
    public static class VerbStateRestorer
    {
        /// <summary>
        /// Restores saved answers from the verb store into entry states.
        /// </summary>
        public static void RestoreSavedAnswers(
            Verb verb,
            List<ConjugationEntryState> entryStates,
            Dictionary<ConjugationFormEnum, IReadOnlyList<string>> expectedAnswers,
            AppOptions options)
        {
            if (verb.Conjugations.Count == 0)
                return;

            foreach (var entry in entryStates)
            {
                if (verb.Conjugations.TryGetValue(entry.ConjugationForm, out var saved))
                {
                    var savedText = saved.Kanji ?? string.Empty;
                    entry.UserInput = savedText;

                    // Check if the saved answer is still correct
                    var expected = expectedAnswers.TryGetValue(entry.ConjugationForm, out var list)
                        ? list
                        : [];
                    entry.Result = AnswerChecker.Check(entry.UserInput, expected, options);
                }
            }
        }

        /// <summary>
        /// Restores verb group selection if previously answered correctly.
        /// </summary>
        public static void RestoreVerbGroup(
            Verb verb,
            AppOptions options,
            Action<VerbGroupEnum?, bool?, bool> setVerbGroupState)
        {
            if (verb.VerbGroupAnsweredCorrectly)
            {
                setVerbGroupState(verb.Group, true, true);
            }
            else
            {
                setVerbGroupState(null, false, false);
            }
        }
    }
}
