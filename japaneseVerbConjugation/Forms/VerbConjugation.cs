using JapaneseVerbConjugation.Controls;
using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Forms;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Constants;
using JapaneseVerbConjugation.SharedResources.Logic;
using JapaneseVerbConjugation.SharedResources.Methods;
using System.Collections.Generic;
using System.Security.Cryptography;

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

        private VerbStore _verbStore;

        private Verb _currentVerb = new();

        public VerbConjugationForm()
        {
            InitializeComponent();
            MinimumSize = new Size(950, 600);

            _appOptions = AppOptionsStore.LoadOrCreateDefault();
            _verbStore = VerbStoreStore.LoadOrCreateDefault();

            ApplyUserOptions(_appOptions, true);

            //This is hard coded for testing purposes only
            // when the app loads verbs from storage we will load the last verb the user was
            // working on or a random one if we have no saved states
            LoadNextVerb(new Verb
            {
                DictionaryForm = "泳ぐ",
                Reading = "およぐ",
                Group =
                VerbGroupEnum.Godan
            });
        }

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

            _expectedAnswers = BuildExpectedAnswersForCurrentVerb();
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

        private void LoadNextVerb(Verb? nextVerb)
        {
            //When we move to loading from storage we will change this to 
            // if (nextVerb == null) => load a random verb from storage
            ArgumentNullException.ThrowIfNull(nextVerb);

            _currentVerb = nextVerb;

            // UI label only
            currentVerb.Text = _currentVerb.DictionaryForm;
            furiganaReading.Text = _currentVerb.Reading;

            // Reset entry states
            foreach (var entry in _entryStates)
            {
                entry.UserInput = string.Empty;
                entry.Result = ConjugationResultEnum.Unchecked;
            }

            // Push reset state into controls + clear UI result styles
            foreach (var c in conjugationTableLayout.Controls)
            {
                if (c is ConjugationEntryControl control)
                {
                    control.RefreshFromState(); // add this method (below)
                }
            }

            // Reset verb group selection UI
            ResetVerbGroup();

            _expectedAnswers = BuildExpectedAnswersForCurrentVerb();
            // Optionally, update any other UI elements to show the loaded verb
        }

        private void OnCheckRequested(object? sender, EventArgs e)
        {
            if (sender is not ConjugationEntryControl entry)
                return;

            var conjugationEntryState = entry.ConjugationEntryState;

            var expected = _expectedAnswers.TryGetValue(conjugationEntryState.ConjugationForm, out var list)
                ? list
                : [];

            conjugationEntryState.Result = AnswerChecker.Check(conjugationEntryState.UserInput, expected, _appOptions);

            entry.RenderResult();
        }

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

            verbGroup.SuspendLayout();
            ResetVerbGroup();
            selectedRadio.ForeColor =
                isCorrect
                    ? ColourConstants.LabelCorrectColour
                    : ColourConstants.LabelIncorrectColour;
            checkVerbGroup.Enabled = !isCorrect;
            verbGroup.ResumeLayout();
        }

        private void ResetVerbGroup()
        {
            五段.Checked = false;
            一段.Checked = false;
            不規則.Checked = false;

            五段.ForeColor = ColourConstants.DefaultTextColour;
            一段.ForeColor = ColourConstants.DefaultTextColour;
            不規則.ForeColor = ColourConstants.DefaultTextColour;
            checkVerbGroup.Enabled = true;
        }

        private bool TryGetSelectedVerbGroupGuess(out VerbGroupEnum guess)
        {
            if (不規則.Checked) { guess = VerbGroupEnum.Irregular; return true; }
            if (一段.Checked) { guess = VerbGroupEnum.Ichidan; return true; }
            if (五段.Checked) { guess = VerbGroupEnum.Godan; return true; }

            guess = default;
            return false;
        }

        private Dictionary<ConjugationFormEnum, IReadOnlyList<string>> BuildExpectedAnswersForCurrentVerb()
        {
            if (_currentVerb is null)
                throw new NullReferenceException("Current verb is not set.");

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

            var masked = MaskHint(full);

            HintPopupForm.Show(
                owner: this,
                title: $"{state.ConjugationForm.ToDisplayLabel()}",
                masked: masked,
                full: full,
                baseFont: Font);
        }
        private static string MaskHint(string answer)
        {
            // keep kanji, mask hiragana, leave punctuation as-is
            return new string([.. answer.Select(c =>
            {
                bool isHiragana = c >= '\u3040' && c <= '\u309F';
                return isHiragana ? '＊' : c;
            })]);
        }

        private static string MaskAnswer(string s)
        {
            // Simple masking: show first char + last char, mask the middle
            // e.g. 食べて -> 食**て, たべません -> た*****ん
            s = s.Trim();
            if (s.Length <= 2) return s;

            var first = s[0];
            var last = s[^1];
            return first + new string('＊', s.Length - 2) + last;
        }

        private static bool ContainsKanji(string s)
            => s.Any(c => c >= '\u4E00' && c <= '\u9FFF');

        private static bool LooksLikeKana(string s)
            => s.All(c =>
                (c >= '\u3040' && c <= '\u309F') || // hiragana
                (c >= '\u30A0' && c <= '\u30FF') || // katakana
                c == 'ー');

        private void ChangeUserOptions(object sender, EventArgs e)
        {
            using var form = new OptionsForm(_appOptions);

            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            var newOptions = form.Result;

            // If absolutely nothing changed, do nothing (no save, no flicker)
            if (OptionsEqual(_appOptions, newOptions))
                return;

            // Persist then apply
            AppOptionsStore.Save(newOptions);
            ApplyUserOptions(newOptions);
        }

        private static bool OptionsEqual(AppOptions a, AppOptions b)
        {
            if (a.ShowFurigana != b.ShowFurigana) return false;
            if (a.AllowHiragana != b.AllowHiragana) return false;
            if (a.FocusModeOnly != b.FocusModeOnly) return false;
            if (a.PersistUserAnswers != b.PersistUserAnswers) return false;

            // HashSet comparison
            return a.EnabledConjugations.SetEquals(b.EnabledConjugations);
        }

        private void ShowImportDialog(object? sender, EventArgs e)
        {
            _verbStore ??= VerbStoreStore.LoadOrCreateDefault();

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

                            _ =>
                                new ImportLogLine(ev.Message, ImportLogColour.Neutral),
                        });
                    });

                    VerbStoreStore.Save(_verbStore);
                }
            }

            using var form = new ImportPacksForm(RunImport, customPath);
            form.ShowDialog(this);
        }
    }
}
