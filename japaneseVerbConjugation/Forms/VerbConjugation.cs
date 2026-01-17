using JapaneseVerbConjugation.Controls;
using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Forms;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Constants;
using JapaneseVerbConjugation.SharedResources.Logic;
using JapaneseVerbConjugation.SharedResources.Methods;

namespace JapaneseVerbConjugation
{
    public partial class VerbConjugationForm : Form
    {
        // Holds the persistent learning state for each conjugation row.
        // This is intentionally separate from UI controls for save/load later.
        private readonly List<ConjugationEntryState> _entryStates = [];

        private readonly VerbStudySession _session;

        // Track if verb group is locked (answered correctly) to prevent changes
        private bool _verbGroupLocked = false;

        #region Constructor & Initialization

        public VerbConjugationForm()
        {
            InitializeComponent();
            MinimumSize = new Size(950, 600);
            
            // Set window title with version
            Text = VersionInfo.GetApplicationTitle();

            // Wire up event handlers to prevent radio button changes when locked
            五段.CheckedChanged += PreventVerbGroupChange;
            一段.CheckedChanged += PreventVerbGroupChange;
            不規則.CheckedChanged += PreventVerbGroupChange;

            _session = VerbStudySession.LoadFromStorage();

            // ApplyUserOptions will call RebuildConjugationRows() on startup
            // This creates the entry states and controls BEFORE we try to load saved answers
            ApplyUserOptions(_session.Options, true);

            // Now load the verb - entry states and controls already exist from RebuildConjugationRows
            var initialVerb = _session.GetInitialVerb();
            if (initialVerb != null)
            {
                LoadNextVerb(initialVerb);
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
            var oldOptions = _session.Options;

            bool enabledConjugationsChanged =
                !oldOptions.EnabledConjugations.SetEquals(newOptions.EnabledConjugations);

            bool showFuriganaChanged = oldOptions.ShowFurigana != newOptions.ShowFurigana;

            bool focusModeChanged = oldOptions.FocusModeOnly != newOptions.FocusModeOnly;


            // Store first (so any subsequent logic reads the new options)
            _session.UpdateOptions(newOptions);

            // Rebuild rows to match enabled conjugations
            if (enabledConjugationsChanged || startUp)
                RebuildConjugationRows();

            if ((enabledConjugationsChanged || startUp) && _session.CurrentVerb != null)
            {
                _session.LoadVerb(_session.CurrentVerb, _entryStates);
                RefreshUiFromSession();
            }

            if (showFuriganaChanged || startUp)
                UpdateVisibilityOfFuriganaReading();
            // Furigana display can be applied
            // For now currentVerb is dictionary form only, so nothing else to do here yet.

            if (focusModeChanged || startUp)
                ApplyFocusFilter(); // later; can be empty for now

        }

        private void RebuildConjugationRows()
        {
            conjugationTableLayout.SuspendLayout();

            conjugationTableLayout.Controls.Clear();
            conjugationTableLayout.RowStyles.Clear();
            conjugationTableLayout.RowCount = 0;

            _entryStates.Clear();

            var enabled = _session.Options.EnabledConjugations;

            // If nothing enabled, show nothing (or you can add a placeholder label)
            foreach (var form in Enum.GetValues<ConjugationFormEnum>())
            {
                if (!enabled.Contains(form))
                    continue;

                AddConjugationEntry(form);
            }

            conjugationTableLayout.ResumeLayout();
        }

        private void UpdateVisibilityOfFuriganaReading()
        {
            dictionaryTableLayout.SuspendLayout();
            conjugationTableLayout.SuspendLayout();
            furiganaReading.Visible = _session.Options.ShowFurigana;
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

            _session.LoadVerb(nextVerb, _entryStates);
            RefreshUiFromSession();
        }

        private void RefreshUiFromSession()
        {
            if (_session.CurrentVerb is null)
                return;

            SetStudyUiEnabled(enabled: true);

            currentVerb.Text = _session.CurrentVerb.DictionaryForm;
            furiganaReading.Text = _session.CurrentVerb.Reading;

            foreach (var c in conjugationTableLayout.Controls)
            {
                if (c is ConjugationEntryControl control)
                {
                    control.RefreshFromState();
                }
            }

            var verbGroupState = _session.GetVerbGroupStateForRestore();
            SetVerbGroupState(
                verbGroupState.Selected,
                verbGroupState.IsCorrect,
                verbGroupState.LockSelection);

            UpdateNavigationButtonStates();
        }

        /// <summary>
        /// Updates the enabled state of Prev/Next buttons based on current verb position.
        /// </summary>
        private void UpdateNavigationButtonStates()
        {
            var nav = _session.GetNavigationState();
            prevVerbButton.Enabled = nav.CanGoPrevious;
            nextVerbButton.Enabled = nav.CanGoNext;
        }

        private void SkipToNextVerb(object sender, EventArgs e)
        {
            if (_session.TryMoveToNext(_entryStates, out _))
            {
                RefreshUiFromSession();
            }
        }

        private void SkipToPreviousVerb(object sender, EventArgs e)
        {
            if (_session.TryMoveToPrevious(_entryStates, out _))
            {
                RefreshUiFromSession();
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
            if (_session.CurrentVerb is null)
                return;

            var result = MessageBox.Show(
                "This will remove all saved answers for the current verb. Are you sure?",
                "Clear conjugation data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            _session.ClearCurrentVerbData(_entryStates);
            RefreshUiFromSession();
        }

        #endregion

        #region Answer Checking & Persistence

        private void OnCheckRequested(object? sender, EventArgs e)
        {
            if (sender is not ConjugationEntryControl entry)
                return;

            var state = entry.ConjugationEntryState;
            _session.CheckAnswer(state);

            entry.RenderResult();
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
            var result = _session.CheckVerbGroup(guess);

            // Use the centralized method to set state
            SetVerbGroupState(result.Selected, result.IsCorrect, result.IsCorrect);
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
                if (_session.CurrentVerb != null &&
                    _session.CurrentVerb.VerbGroupAnsweredCorrectly)
                {
                    var correctGroup = _session.CurrentVerb.Group;
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
            var hint = _session.GetHint(state.ConjugationForm);
            if (!hint.HasHint)
                return;

            HintPopupForm.Show(
                owner: this,
                title: $"{state.ConjugationForm.ToDisplayLabel()}",
                masked: hint.Masked,
                full: hint.Full,
                baseFont: Font);
        }

        #endregion

        #region Options & Import Dialogs

        private void ChangeUserOptions(object sender, EventArgs e)
        {
            using var form = new OptionsForm(_session.Options);

            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            var newOptions = form.Result;

            // If absolutely nothing changed, do nothing (no save, no flicker)
            if (AppOptionsStore.AreEqual(_session.Options, newOptions))
                return;

            // Persist then apply
            AppOptionsStore.Save(newOptions);
            ApplyUserOptions(newOptions);
        }

        private void ShowImportDialog(object? sender, EventArgs e)
        {
            var verbStoreEmptyBeforeImport = _session.Store.Verbs.Count == 0;

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

                    VerbImportService.ImportFromDelimitedFile(file, _session.Store, ev =>
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

                    VerbStoreStore.Save(_session.Store);
                }
            }

            using var form = new ImportPacksForm(RunImport, customPath);
            form.ShowDialog(this);

            // Unlock + load active verb only if we were empty before
            if (verbStoreEmptyBeforeImport && _session.Store.Verbs.Count > 0)
            {
                SetStudyUiEnabled(true);
                // Find the active verb (should be the first one imported)
                var activeVerb = _session.GetInitialVerb();
                if (activeVerb is null)
                    return;
                LoadNextVerb(activeVerb);
            }
        }

        #endregion
    }
}
