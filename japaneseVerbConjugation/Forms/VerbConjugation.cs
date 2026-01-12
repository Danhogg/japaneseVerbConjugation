using JapaneseVerbConjugation.Controls;
using JapaneseVerbConjugation.Enums;
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

        // This is our list of expected answers for the current verb. Will also be used for
        // the hints and answers if a user needs help.
        private Dictionary<ConjugationForm, IReadOnlyList<string>> _expectedAnswers = [];

        // AppOptions loaded from user settings.
        private AppOptions _appOptions = new()
        {
            AllowKanji = true,
            AllowKana = false,
            PersistUserAnswers = false,
            FocusModeOnly = false,
            ShowFurigana = false,
            EnabledConjugations = [] // leave empty for now; your UI will control this later
        };

        private Verb _currentVerb = new();

        public VerbConjugationForm()
        {
            InitializeComponent();
            MinimumSize = new Size(950, 600);
            conjugationTableLayout.SuspendLayout();
            WireUpDynamicContent();
            conjugationTableLayout.ResumeLayout();

            //This is hard coded for testing purposes only
            // when the app loads verbs from storage we will load the last verb the user was
            // working on or a random one if we have no saved states
            LoadNextVerb(new Verb
            {
                DictionaryForm = "泳ぐ",
                Reading = "およぐ",
                Group =
                VerbGroup.Godan
            });
        }

        private void WireUpDynamicContent()
        {
            //TODO this will be updated to check the users app settings for which conjugations
            // we need to display. It will also need to be called to update when the user changes
            // their settings
            AddConjugationEntry(ConjugationForm.PresentPlain);
            AddConjugationEntry(ConjugationForm.PresentPolite);

            AddConjugationEntry(ConjugationForm.PastPlain);
            AddConjugationEntry(ConjugationForm.PastPolite);

            AddConjugationEntry(ConjugationForm.NegativePlain);
            AddConjugationEntry(ConjugationForm.NegativePolite);

            AddConjugationEntry(ConjugationForm.VolitionalPlain);
            AddConjugationEntry(ConjugationForm.VolitionalPolite);

            AddConjugationEntry(ConjugationForm.ConditionalBa);
            AddConjugationEntry(ConjugationForm.ConditionalTara);

            AddConjugationEntry(ConjugationForm.PotentialPlain);
            AddConjugationEntry(ConjugationForm.PotentialPolite);

            AddConjugationEntry(ConjugationForm.PassivePlain);
            AddConjugationEntry(ConjugationForm.PassivePolite);

            AddConjugationEntry(ConjugationForm.CausativePlain);
            AddConjugationEntry(ConjugationForm.CausativePolite);

            AddConjugationEntry(ConjugationForm.CausativePassivePlain);
            AddConjugationEntry(ConjugationForm.CausativePassivePolite);

            AddConjugationEntry(ConjugationForm.TeForm);

            AddConjugationEntry(ConjugationForm.Imperative);
        }

        private void AddConjugationEntry(ConjugationForm form)
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
                entry.Result = ConjugationResult.Unchecked;
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
                VerbGroup.Godan => 五段,
                VerbGroup.Ichidan => 一段,
                VerbGroup.Irregular => 不規則,
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

        private bool TryGetSelectedVerbGroupGuess(out VerbGroup guess)
        {
            if (不規則.Checked) { guess = VerbGroup.Irregular; return true; }
            if (一段.Checked) { guess = VerbGroup.Ichidan; return true; }
            if (五段.Checked) { guess = VerbGroup.Godan; return true; }

            guess = default;
            return false;
        }

        private Dictionary<ConjugationForm, IReadOnlyList<string>> BuildExpectedAnswersForCurrentVerb()
        {
            if (_currentVerb is null)
                throw new NullReferenceException("Current verb is not set.");

            var dict = new Dictionary<ConjugationForm, IReadOnlyList<string>>();

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

            if (_currentVerb is null)
                return;

            var form = entry.ConjugationEntryState.ConjugationForm;

            if (!_expectedAnswers.TryGetValue(form, out var answers) || answers.Count == 0)
            {
                HintPopupForm.Show(this, "Hint", "No hint available.", "No hint available.", new Font("Yu Gothic UI", 12F));
                return;
            }

            var full = PickAnswerForHint(answers);
            var masked = MaskAnswer(full);

            HintPopupForm.Show(this, $"{form.ToDisplayLabel()} hint", masked, full, new Font("Yu Gothic UI", 16F));
        }        

        private string PickAnswerForHint(IReadOnlyList<string> answers)
        {
            // Follow options: prefer kanji when kana not allowed, etc.
            if (_appOptions.AllowKanji && !_appOptions.AllowKana)
                return answers.FirstOrDefault(ContainsKanji) ?? answers[0];

            if (!_appOptions.AllowKanji && _appOptions.AllowKana)
                return answers.FirstOrDefault(LooksLikeKana) ?? answers[0];

            // both allowed: prefer kanji if available
            return answers.FirstOrDefault(ContainsKanji) ?? answers[0];
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
    }
}
