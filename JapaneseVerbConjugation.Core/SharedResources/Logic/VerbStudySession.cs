using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Methods;

namespace JapaneseVerbConjugation.SharedResources.Logic
{
    public sealed class VerbStudySession
    {
        public AppOptions Options { get; private set; }
        public VerbStore Store { get; }
        public Verb? CurrentVerb { get; private set; }
        public Dictionary<ConjugationFormEnum, IReadOnlyList<string>> ExpectedAnswers { get; private set; } = [];

        private VerbStudySession(AppOptions options, VerbStore store)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public static VerbStudySession LoadFromStorage()
        {
            var options = AppOptionsStore.LoadOrCreateDefault();
            var store = VerbStoreStore.LoadOrCreateDefault();
            return new VerbStudySession(options, store);
        }

        public void UpdateOptions(AppOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public Verb? GetInitialVerb()
        {
            if (Store.Verbs.Count == 0)
                return null;

            return Store.Verbs.FirstOrDefault(v => v.Active) ?? Store.Verbs[0];
        }

        public void LoadVerb(Verb verb, List<ConjugationEntryState> entryStates)
        {
            ArgumentNullException.ThrowIfNull(verb);
            ArgumentNullException.ThrowIfNull(entryStates);

            // Update active verb if it's different from the one we're loading
            if (CurrentVerb?.Id != verb.Id)
            {
                foreach (var v in Store.Verbs)
                {
                    v.Active = false;
                }
                verb.Active = true;
                VerbStoreStore.Save(Store);
            }

            CurrentVerb = verb;

            ExpectedAnswers = BuildExpectedAnswers(entryStates, verb);

            // Reset entry states
            foreach (var entry in entryStates)
            {
                entry.UserInput = string.Empty;
                entry.Result = ConjugationResultEnum.Unchecked;
            }

            VerbStateRestorer.RestoreSavedAnswers(
                CurrentVerb,
                entryStates,
                ExpectedAnswers,
                Options);
        }

        public ConjugationResultEnum CheckAnswer(ConjugationEntryState state)
        {
            if (CurrentVerb is null)
                return ConjugationResultEnum.Unchecked;

            var expected = ExpectedAnswers.TryGetValue(state.ConjugationForm, out var list)
                ? list
                : [];

            state.Result = AnswerChecker.Check(state.UserInput, expected, Options);

            AnswerPersistenceService.PersistAnswer(
                CurrentVerb,
                state.ConjugationForm,
                state.UserInput,
                expected,
                state.Result,
                Options,
                Store);

            return state.Result;
        }

        public void PersistAllAnswers(List<ConjugationEntryState> entryStates)
        {
            if (CurrentVerb is null)
                return;

            AnswerPersistenceService.PersistAllAnswers(
                CurrentVerb,
                entryStates,
                ExpectedAnswers,
                Options,
                Store);
        }

        public VerbGroupCheckResult CheckVerbGroup(VerbGroupEnum guess)
        {
            var correct = CurrentVerb?.Group ?? throw new NullReferenceException("Current verb is not set.");
            bool isCorrect = guess == correct;

            if (isCorrect && CurrentVerb != null)
            {
                CurrentVerb.VerbGroupAnsweredCorrectly = true;
                VerbStoreStore.Save(Store);
            }

            return new VerbGroupCheckResult(guess, correct, isCorrect);
        }

        public VerbGroupState GetVerbGroupStateForRestore()
        {
            if (CurrentVerb is null)
                return new VerbGroupState(null, false, false);

            if (CurrentVerb.VerbGroupAnsweredCorrectly)
            {
                return new VerbGroupState(CurrentVerb.Group, true, true);
            }

            return new VerbGroupState(null, false, false);
        }

        public NavigationState GetNavigationState()
        {
            if (CurrentVerb is null || Store.Verbs.Count == 0)
                return new NavigationState(false, false);

            int currentIndex = Store.Verbs.FindIndex(v => v.Id == CurrentVerb.Id);
            bool canPrev = currentIndex > 0;
            bool canNext = currentIndex >= 0 && currentIndex < Store.Verbs.Count - 1;

            return new NavigationState(canPrev, canNext);
        }

        public bool TryMoveToNext(List<ConjugationEntryState> entryStates, out Verb? nextVerb)
        {
            nextVerb = null;
            if (CurrentVerb is null || Store.Verbs.Count == 0)
                return false;

            int currentIndex = Store.Verbs.FindIndex(v => v.Id == CurrentVerb.Id);
            if (currentIndex < 0 || currentIndex >= Store.Verbs.Count - 1)
                return false;

            PersistAllAnswers(entryStates);
            nextVerb = Store.Verbs[currentIndex + 1];
            LoadVerb(nextVerb, entryStates);
            return true;
        }

        public bool TryMoveToPrevious(List<ConjugationEntryState> entryStates, out Verb? prevVerb)
        {
            prevVerb = null;
            if (CurrentVerb is null || Store.Verbs.Count == 0)
                return false;

            int currentIndex = Store.Verbs.FindIndex(v => v.Id == CurrentVerb.Id);
            if (currentIndex <= 0)
                return false;

            PersistAllAnswers(entryStates);
            prevVerb = Store.Verbs[currentIndex - 1];
            LoadVerb(prevVerb, entryStates);
            return true;
        }

        public HintResult GetHint(ConjugationFormEnum form)
        {
            if (CurrentVerb is null)
                return new HintResult(false, string.Empty, string.Empty);

            var expected = ConjugationEngine.Generate(
                CurrentVerb.DictionaryForm,
                CurrentVerb.Reading,
                CurrentVerb.Group,
                form);

            var full = HintAnswerPicker.PickBestForHint(expected);
            if (string.IsNullOrWhiteSpace(full))
                return new HintResult(false, string.Empty, string.Empty);

            var masked = HintMasking.MaskHint(full);
            return new HintResult(true, masked, full);
        }

        private static Dictionary<ConjugationFormEnum, IReadOnlyList<string>> BuildExpectedAnswers(
            List<ConjugationEntryState> entryStates,
            Verb verb)
        {
            var dict = new Dictionary<ConjugationFormEnum, IReadOnlyList<string>>();

            foreach (var state in entryStates)
            {
                dict[state.ConjugationForm] = ConjugationEngine.Generate(
                    dictionaryForm: verb.DictionaryForm,
                    reading: verb.Reading,
                    group: verb.Group,
                    form: state.ConjugationForm);
            }

            return dict;
        }
    }

    public readonly record struct VerbGroupState(
        VerbGroupEnum? Selected,
        bool? IsCorrect,
        bool LockSelection);

    public readonly record struct VerbGroupCheckResult(
        VerbGroupEnum Selected,
        VerbGroupEnum Correct,
        bool IsCorrect);

    public readonly record struct NavigationState(
        bool CanGoPrevious,
        bool CanGoNext);

    public readonly record struct HintResult(
        bool HasHint,
        string Masked,
        string Full);
}
