using JapaneseVerbConjugation.Controls;
using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Forms;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Constants;
using JapaneseVerbConjugation.SharedResources.Logic;
using JapaneseVerbConjugation.SharedResources.Methods;
using System.Diagnostics;

namespace JapaneseVerbConjugation
{
    public partial class VerbConjugationForm : Form
    {
        // Holds the persistent learning state for each conjugation row.
        // This is intentionally separate from UI controls for save/load later.
        private readonly List<ConjugationEntryState> _entryStates = [];

        // This is our list of expected answers for the current verb. Will also be used for
        // the hints and answers if a user needs help.
        private Dictionary<ConjugationFormEnum, IReadOnlyList<string>> _expectedAnswers = [];

        // AppOptions loaded from user settings.
        private AppOptions _appOptions;

        private readonly VerbStore _verbStore;

        private Verb? _currentVerb = null;

        // Track if verb group is locked (answered correctly) to prevent changes
        private bool _verbGroupLocked = false;

        #region Constructor & Initialization

        public VerbConjugationForm()
        {
            InitializeComponent();
            MinimumSize = new Size(950, 600);

            // Wire up event handlers to prevent radio button changes when locked
            五段.CheckedChanged += PreventVerbGroupChange;
            一段.CheckedChanged += PreventVerbGroupChange;
            不規則.CheckedChanged += PreventVerbGroupChange;

            _appOptions = AppOptionsStore.LoadOrCreateDefault();
            _verbStore = VerbStoreStore.LoadOrCreateDefault();

            // ApplyUserOptions will call RebuildConjugationRows() on startup
            // This creates the entry states and controls BEFORE we try to load saved answers
            ApplyUserOptions(_appOptions, true);

            // Now load the verb - entry states and controls already exist from RebuildConjugationRows
            if (_verbStore.Verbs.Count > 0)
            {
                // Find the active verb, or use the first one if none is active
                var activeVerb = _verbStore.Verbs.FirstOrDefault(v => v.Active) ?? _verbStore.Verbs[0];
                LoadNextVerb(activeVerb);
            }
            else
            {
                LoadNextVerb(null, verbStoreEmpty: true);
            }
        }

        #endregion

        #region Options & UI Configuration

        private void ApplyUserOptions(AppOptions newOptions, bool startUp = false)
        {
            var oldOptions = _appOptions;

            bool enabledConjugationsChanged =
                !oldOptions.EnabledConjugations.SetEquals(newOptions.EnabledConjugations);

            bool showFuriganaChanged = oldOptions.ShowFurigana != newOptions.ShowFurigana;

            bool focusModeChanged = oldOptions.FocusModeOnly != newOptions.FocusModeOnly;

            bool persistAnswersChanged = oldOptions.PersistUserAnswers != newOptions.PersistUserAnswers;

            // Store first (so any subsequent logic reads the new options)
            _appOptions = newOptions;

            // Rebuild rows to match enabled conjugations
            if (enabledConjugationsChanged || startUp)
                RebuildConjugationRows();

            if (showFuriganaChanged || startUp)
                UpdateVisibilityOfFuriganaReading();
            // Furigana display can be applied
            // For now currentVerb is dictionary form only, so nothing else to do here yet.

            if (focusModeChanged || startUp)
                ApplyFocusFilter(); // later; can be empty for now

            if (persistAnswersChanged || startUp)
                ApplyPersistencePolicyUi(); // probably nothing visual
        }

        private void RebuildConjugationRows()
        {
            conjugationTableLayout.SuspendLayout();

            conjugationTableLayout.Controls.Clear();
            conjugationTableLayout.RowStyles.Clear();
            conjugationTableLayout.RowCount = 0;

            _entryStates.Clear();

            var enabled = _appOptions.EnabledConjugations;

            // If nothing enabled, show nothing (or you can add a placeholder label)
            foreach (var form in Enum.GetValues<ConjugationFormEnum>())
            {
                if (!enabled.Contains(form))
                    continue;

                AddConjugationEntry(form);
            }

            // Only build expected answers if we have a current verb
            // (this will be rebuilt in LoadNextVerb if needed)
            if (_currentVerb != null)
            {
                _expectedAnswers = BuildExpectedAnswersForCurrentVerb();
            }
            else
            {
                _expectedAnswers = [];
            }
            
            conjugationTableLayout.ResumeLayout();
        }

        private void UpdateVisibilityOfFuriganaReading()
        {
            dictionaryTableLayout.SuspendLayout();
            conjugationTableLayout.SuspendLayout();
            furiganaReading.Visible = _appOptions.ShowFurigana;
            conjugationTableLayout.ResumeLayout();
            dictionaryTableLayout.ResumeLayout();
        }

        private void ApplyFocusFilter() { }
        private void ApplyPersistencePolicyUi() { }

        private void AddConjugationEntry(ConjugationFormEnum form)
        {
            var entry = new ConjugationEntryState
            {
                ConjugationForm = form,
            };

            _entryStates.Add(entry);

            var entryControl = new ConjugationEntryControl(entry);
            entryControl.CheckRequested += OnCheckRequested;
            entryControl.HintRequested += OnHintRequested;

            int index = conjugationTableLayout.Controls.Count;
            int column = index % 2;
            int row = index / 2;

            if (conjugationTableLayout.RowCount <= row)
            {
                conjugationTableLayout.RowStyles.Add(
                    new RowStyle(SizeType.AutoSize));
                conjugationTableLayout.RowCount++;
            }

            conjugationTableLayout.Controls.Add(entryControl, column, row);
        }

        #endregion

        #region Verb Loading & State Management

        private void LoadNextVerb(Verb? nextVerb = null, bool verbStoreEmpty = false)
        {
            if (nextVerb == null && verbStoreEmpty)
            {
                SetStudyUiEnabled(enabled: false);
                currentVerb.Text = "Import verbs to begin";
                furiganaReading.Text = "";
                return;
            }
            //When we move to loading from storage we will change this to 
            // if (nextVerb == null) => load a random verb from storage
            ArgumentNullException.ThrowIfNull(nextVerb);

            // Update active verb if it's different from the one we're loading
            if (_currentVerb?.Id != nextVerb.Id)
            {
                // Deactivate all verbs, then activate the one we're loading
                foreach (var verb in _verbStore.Verbs)
                {
                    verb.Active = false;
                }
                nextVerb.Active = true;
                VerbStoreStore.Save(_verbStore);
            }

            _currentVerb = nextVerb;

            // UI label only
            currentVerb.Text = _currentVerb.DictionaryForm;
            furiganaReading.Text = _currentVerb.Reading;

            // Build expected answers FIRST (needed for checking saved answers)
            _expectedAnswers = BuildExpectedAnswersForCurrentVerb();

            // Reset entry states
            foreach (var entry in _entryStates)
            {
                entry.UserInput = string.Empty;
                entry.Result = ConjugationResultEnum.Unchecked;
            }

            // Restore saved answers from verb store (source of truth)
            VerbStateRestorer.RestoreSavedAnswers(
                _currentVerb,
                _entryStates,
                _expectedAnswers,
                _appOptions);

            // Push restored state into controls (this updates the UI text boxes)
            // IMPORTANT: This must happen AFTER restoring to entry states
            foreach (var c in conjugationTableLayout.Controls)
            {
                if (c is ConjugationEntryControl control)
                {
                    // Refresh the control to show the restored state
                    control.RefreshFromState();
                }
            }

            // Restore verb group selection if previously answered correctly
            VerbStateRestorer.RestoreVerbGroup(
                _currentVerb,
                _appOptions,
                SetVerbGroupState);

            // Update navigation button states
            UpdateNavigationButtonStates();
        }

        /// <summary>
        /// Updates the enabled state of Prev/Next buttons based on current verb position.
        /// </summary>
        private void UpdateNavigationButtonStates()
        {
            if (_currentVerb is null || _verbStore.Verbs.Count == 0)
            {
                prevVerbButton.Enabled = false;
                nextVerbButton.Enabled = false;
                return;
            }

            int currentIndex = _verbStore.Verbs.FindIndex(v => v.Id == _currentVerb.Id);
            
            // Disable Prev if at first verb, disable Next if at last verb
            prevVerbButton.Enabled = currentIndex > 0;
            nextVerbButton.Enabled = currentIndex >= 0 && currentIndex < _verbStore.Verbs.Count - 1;
        }

        private Dictionary<ConjugationFormEnum, IReadOnlyList<string>> BuildExpectedAnswersForCurrentVerb()
        {
            if (_currentVerb is null)
                return [];

            var dict = new Dictionary<ConjugationFormEnum, IReadOnlyList<string>>();

            foreach (var state in _entryStates)
            {
                dict[state.ConjugationForm] = ConjugationEngine.Generate(
                    dictionaryForm: _currentVerb.DictionaryForm,
                    reading: _currentVerb.Reading,
                    group: _currentVerb.Group,
                    form: state.ConjugationForm);
            }

            return dict;
        }

        private void SkipToNextVerb(object sender, EventArgs e)
        {
            if (_currentVerb is null || _verbStore.Verbs.Count == 0)
                return;

            // Save all current answers before moving to next verb
            PersistAllCurrentAnswers();

            // Find current verb index
            int currentIndex = _verbStore.Verbs.FindIndex(v => v.Id == _currentVerb.Id);
            if (currentIndex < 0)
                return;

            // Don't go past the last verb
            if (currentIndex >= _verbStore.Verbs.Count - 1)
                return;

            // Deactivate current verb
            _currentVerb.Active = false;

            // Get next verb
            int nextIndex = currentIndex + 1;
            var nextVerb = _verbStore.Verbs[nextIndex];

            // Activate the next verb
            nextVerb.Active = true;
            VerbStoreStore.Save(_verbStore);

            LoadNextVerb(nextVerb);
            
            // Refresh UI controls to show the new verb's state
            foreach (var c in conjugationTableLayout.Controls)
            {
                if (c is ConjugationEntryControl control)
                {
                    control.RefreshFromState();
                }
            }
        }

        private void SkipToPreviousVerb(object sender, EventArgs e)
        {
            if (_currentVerb is null || _verbStore.Verbs.Count == 0)
                return;

            // Save all current answers before moving to previous verb
            PersistAllCurrentAnswers();

            // Find current verb index
            int currentIndex = _verbStore.Verbs.FindIndex(v => v.Id == _currentVerb.Id);
            if (currentIndex < 0)
                return;

            // Don't go before the first verb
            if (currentIndex <= 0)
                return;

            // Deactivate current verb
            _currentVerb.Active = false;

            // Get previous verb
            int prevIndex = currentIndex - 1;
            var prevVerb = _verbStore.Verbs[prevIndex];

            // Activate the previous verb
            prevVerb.Active = true;
            VerbStoreStore.Save(_verbStore);

            LoadNextVerb(prevVerb);
            
            // Refresh UI controls to show the new verb's state
            foreach (var c in conjugationTableLayout.Controls)
            {
                if (c is ConjugationEntryControl control)
                {
                    control.RefreshFromState();
                }
            }
        }

        private void SetStudyUiEnabled(bool enabled)
        {
            conjugationTableLayout.Enabled = enabled;
            checkVerbGroupButton.Enabled = enabled;
            optionsButton.Enabled = enabled;
            verbGroup.Enabled = enabled;
            prevVerbButton.Enabled = enabled;
            nextVerbButton.Enabled = enabled;
            
            // Update navigation button states based on position if enabled
            if (enabled)
            {
                UpdateNavigationButtonStates();
            }
        }

        private void ClearAnswers(object sender, EventArgs e)
        {
            //TODO Make sure we load a popup that says you will lose all your progress
            // then clear all the answers from the _entryStates and the user data storage
        }

        #endregion

        #region Answer Checking & Persistence

        private void OnCheckRequested(object? sender, EventArgs e)
        {
            if (sender is not ConjugationEntryControl entry)
                return;

            var state = entry.ConjugationEntryState;

            var expected = _expectedAnswers.TryGetValue(state.ConjugationForm, out var list)
                ? list
                : [];

            state.Result = AnswerChecker.Check(state.UserInput, expected, _appOptions);

            // Save the user's answer to the verb store (source of truth)
            PersistUserAnswer(state.ConjugationForm, state.UserInput, expected, state.Result);

            entry.RenderResult();
        }

        private void PersistUserAnswer(
            ConjugationFormEnum form,
            string? userInput,
            IReadOnlyList<string> expected,
            ConjugationResultEnum result)
        {
            if (_currentVerb is null)
                return;

            AnswerPersistenceService.PersistAnswer(
                _currentVerb,
                form,
                userInput,
                expected,
                result,
                _appOptions,
                _verbStore);
        }

        private void PersistAllCurrentAnswers()
        {
            if (_currentVerb is null)
                return;

            AnswerPersistenceService.PersistAllAnswers(
                _currentVerb,
                _entryStates,
                _expectedAnswers,
                _appOptions,
                _verbStore);
        }

        #endregion

        #region Verb Group Management

        private void CheckVerbGroup(object sender, EventArgs e)
        {
            if (!TryGetSelectedVerbGroupGuess(out var guess))
            {
                MessageBox.Show("Please select a verb group before checking.", "No selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var correct = _currentVerb?.Group ?? throw new NullReferenceException("Current verb is not set.");

            // Highlight the selected guess (green if correct, red if incorrect)
            var selectedRadio = guess switch
            {
                VerbGroupEnum.Godan => 五段,
                VerbGroupEnum.Ichidan => 一段,
                VerbGroupEnum.Irregular => 不規則,
                _ => 五段
            };

            var isCorrect = guess == correct;

            // Use the centralized method to set state
            SetVerbGroupState(guess, isCorrect, isCorrect);

            // If correct, save the state
            if (isCorrect && _appOptions.PersistUserAnswers && _currentVerb != null)
            {
                _currentVerb.VerbGroupAnsweredCorrectly = true;
                VerbStoreStore.Save(_verbStore);
            }
        }

        private void ResetVerbGroup()
        {
            SetVerbGroupState(null, false, false);
        }

        /// <summary>
        /// Sets the verb group UI state consistently.
        /// </summary>
        /// <param name="selectedGroup">The group to select and highlight (null to reset all)</param>
        /// <param name="isCorrect">True if the selected group is correct (green), false if incorrect (red), null to reset</param>
        /// <param name="lockSelection">True to lock selection (prevent changes), false to allow interaction</param>
        private void SetVerbGroupState(VerbGroupEnum? selectedGroup, bool? isCorrect, bool lockSelection)
        {
            verbGroup.SuspendLayout();

            // Update lock state
            _verbGroupLocked = lockSelection;

            // Reset all radio buttons
            五段.Checked = false;
            一段.Checked = false;
            不規則.Checked = false;
            五段.ForeColor = ColourConstants.DefaultTextColour;
            一段.ForeColor = ColourConstants.DefaultTextColour;
            不規則.ForeColor = ColourConstants.DefaultTextColour;

            // Set state for selected group if provided
            if (selectedGroup.HasValue)
            {
                var selectedRadio = selectedGroup.Value switch
                {
                    VerbGroupEnum.Godan => 五段,
                    VerbGroupEnum.Ichidan => 一段,
                    VerbGroupEnum.Irregular => 不規則,
                    _ => 五段
                };

                selectedRadio.Checked = true;

                // Set color based on correctness
                if (isCorrect.HasValue)
                {
                    selectedRadio.ForeColor = isCorrect.Value
                        ? ColourConstants.LabelCorrectColour
                        : ColourConstants.LabelIncorrectColour;
                }
            }

            // Keep radio buttons enabled to preserve color display
            // We prevent interaction via event handling when locked
            五段.Enabled = true;
            一段.Enabled = true;
            不規則.Enabled = true;
            checkVerbGroupButton.Enabled = !lockSelection;

            verbGroup.ResumeLayout();
        }

        /// <summary>
        /// Prevents radio button changes when verb group is locked (answered correctly).
        /// </summary>
        private void PreventVerbGroupChange(object? sender, EventArgs e)
        {
            if (_verbGroupLocked && sender is RadioButton radio)
            {
                // Revert the change by restoring the correct selection
                if (_currentVerb != null && _appOptions.PersistUserAnswers && _currentVerb.VerbGroupAnsweredCorrectly)
                {
                    var correctGroup = _currentVerb.Group;
                    var correctRadio = correctGroup switch
                    {
                        VerbGroupEnum.Godan => 五段,
                        VerbGroupEnum.Ichidan => 一段,
                        VerbGroupEnum.Irregular => 不規則,
                        _ => 五段
                    };
                    
                    // If they tried to change from the correct one, revert it
                    if (radio != correctRadio)
                    {
                        radio.Checked = false;
                        correctRadio.Checked = true;
                    }
                }
            }
        }

        private bool TryGetSelectedVerbGroupGuess(out VerbGroupEnum guess)
        {
            if (不規則.Checked) { guess = VerbGroupEnum.Irregular; return true; }
            if (一段.Checked) { guess = VerbGroupEnum.Ichidan; return true; }
            if (五段.Checked) { guess = VerbGroupEnum.Godan; return true; }

            guess = default;
            return false;
        }

        #endregion

        #region Hints

        private void OnHintRequested(object? sender, EventArgs e)
        {
            if (sender is not ConjugationEntryControl entry)
                return;

            var state = entry.ConjugationEntryState;

            var verb = _currentVerb ?? throw new NullReferenceException("Current verb is not set.");

            var expected = ConjugationEngine.Generate(
                verb.DictionaryForm,
                verb.Reading,
                verb.Group,
                state.ConjugationForm);

            var full = HintAnswerPicker.PickBestForHint(expected);

            if (string.IsNullOrWhiteSpace(full))
                return;

            var masked = HintMasking.MaskHint(full);

            HintPopupForm.Show(
                owner: this,
                title: $"{state.ConjugationForm.ToDisplayLabel()}",
                masked: masked,
                full: full,
                baseFont: Font);
        }

        #endregion

        #region Options & Import Dialogs

        private void ChangeUserOptions(object sender, EventArgs e)
        {
            using var form = new OptionsForm(_appOptions);

            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            var newOptions = form.Result;

            // If absolutely nothing changed, do nothing (no save, no flicker)
            if (AppOptionsStore.AreEqual(_appOptions, newOptions))
                return;

            // Persist then apply
            AppOptionsStore.Save(newOptions);
            ApplyUserOptions(newOptions);
        }

        private void ShowImportDialog(object? sender, EventArgs e)
        {
            var verbStoreEmptyBeforeImport = _verbStore.Verbs.Count == 0;

            var customPath = CustomCsvStore.EnsureExists();

            void RunImport(ImportSelection sel, Action<ImportLogLine> log)
            {
                var files = new List<string>();

                if (sel.ImportN5) files.Add(Path.Combine(AppContext.BaseDirectory, "Data", "N5.csv"));
                if (sel.ImportN4) files.Add(Path.Combine(AppContext.BaseDirectory, "Data", "N4.csv"));
                if (sel.ImportCustom) files.Add(sel.CustomPath);

                foreach (var file in files)
                {
                    if (!File.Exists(file))
                    {
                        log(new ImportLogLine($"Missing file: {file}", ImportLogColour.Error));
                        continue;
                    }

                    log(new ImportLogLine($"Importing: {Path.GetFileName(file)}", ImportLogColour.Neutral));

                    VerbImportService.ImportFromDelimitedFile(file, _verbStore, ev =>
                    {
                        log(ev.Status switch
                        {
                            VerbImportService.RowStatus.Added =>
                                new ImportLogLine(ev.Message, ImportLogColour.Added),

                            VerbImportService.RowStatus.Duplicate =>
                                new ImportLogLine(ev.Message, ImportLogColour.Duplicate),

                            VerbImportService.RowStatus.Error =>
                                new ImportLogLine(ev.Message, ImportLogColour.Error),

                            VerbImportService.RowStatus.Summary =>
                                // Color code summary lines: Added=Green, Skipped=Orange, Errored=Red, Total=Neutral
                                new ImportLogLine(ev.Message, ev.Message.StartsWith("Added:", StringComparison.Ordinal) ? ImportLogColour.Added :
                                                              ev.Message.StartsWith("Skipped:", StringComparison.Ordinal) ? ImportLogColour.Duplicate :
                                                              ev.Message.StartsWith("Errored:", StringComparison.Ordinal) ? ImportLogColour.Error :
                                                              ImportLogColour.Neutral),

                            _ =>
                                new ImportLogLine(ev.Message, ImportLogColour.Neutral),
                        });
                    });

                    VerbStoreStore.Save(_verbStore);
                }
            }

            using var form = new ImportPacksForm(RunImport, customPath);
            form.ShowDialog(this);

            // Unlock + load active verb only if we were empty before
            if (verbStoreEmptyBeforeImport && _verbStore.Verbs.Count > 0)
            {
                SetStudyUiEnabled(true);
                // Find the active verb (should be the first one imported)
                var activeVerb = _verbStore.Verbs.FirstOrDefault(v => v.Active) ?? _verbStore.Verbs[0];
                LoadNextVerb(activeVerb);
            }
        }

        #endregion
    }
}
