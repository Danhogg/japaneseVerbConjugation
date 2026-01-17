using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    /// <summary>
    /// Service for persisting user answers to the verb store.
    /// </summary>
    public static class AnswerPersistenceService
    {
        /// <summary>
        /// Persists a single user answer to the verb store.
        /// </summary>
        public static void PersistAnswer(
            Verb verb,
            ConjugationFormEnum form,
            string? userInput,
            IReadOnlyList<string> expected,
            ConjugationResultEnum result,
            AppOptions options,
            VerbStore store)
        {
            if (verb is null)
                return;

            // Save the user's input as the source of truth
            // If the answer is correct, use the canonical form; otherwise save what the user typed
            string answerToSave;
            if (result == ConjugationResultEnum.Correct && expected.Count > 0)
            {
                // For correct answers, save the canonical form
                answerToSave = ConjugationAnswerPicker.PickCanonical(expected) ?? userInput ?? string.Empty;
            }
            else
            {
                // For incorrect/unchecked answers, save what the user typed
                answerToSave = userInput ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(answerToSave))
                return;

            verb.Conjugations[form] = new ConjugationAnswer
            {
                Kanji = answerToSave
            };

            VerbStoreStore.Save(store);
        }

        /// <summary>
        /// Persists all current answers from entry states to the verb store.
        /// </summary>
        public static void PersistAllAnswers(
            Verb verb,
            List<ConjugationEntryState> entryStates,
            Dictionary<ConjugationFormEnum, IReadOnlyList<string>> expectedAnswers,
            AppOptions options,
            VerbStore store)
        {
            if (verb is null)
                return;

            bool anyChanges = false;

            foreach (var entry in entryStates)
            {
                if (string.IsNullOrWhiteSpace(entry.UserInput))
                    continue;

                var expected = expectedAnswers.TryGetValue(entry.ConjugationForm, out var list)
                    ? list
                    : [];

                // Determine what to save based on the result
                string answerToSave;
                if (entry.Result == ConjugationResultEnum.Correct && expected.Count > 0)
                {
                    answerToSave = ConjugationAnswerPicker.PickCanonical(expected) ?? entry.UserInput;
                }
                else
                {
                    answerToSave = entry.UserInput;
                }

                // Only update if different from what's already saved
                if (!verb.Conjugations.TryGetValue(entry.ConjugationForm, out var existing) ||
                    existing.Kanji != answerToSave)
                {
                    verb.Conjugations[entry.ConjugationForm] = new ConjugationAnswer
                    {
                        Kanji = answerToSave
                    };
                    anyChanges = true;
                }
            }

            if (anyChanges)
            {
                VerbStoreStore.Save(store);
            }
        }
    }
}
