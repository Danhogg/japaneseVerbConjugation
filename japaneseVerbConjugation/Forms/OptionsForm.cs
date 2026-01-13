using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models;
using JapaneseVerbConjugation.SharedResources.Constants;

namespace JapaneseVerbConjugation.Forms
{
    public sealed class OptionsForm : Form
    {
        private readonly CheckBox _showFurigana = new() { Text = "Show Furigana" };
        private readonly CheckBox _allowHiragana = new() { Text = "Allow Hiragana" };
        private readonly CheckBox _focusOnly = new() { Text = "Focus mode only" };
        private readonly CheckBox _persistAnswers = new() { Text = "Persist user answers" };

        private readonly CheckedListBox _conjugations = new()
        {
            CheckOnClick = true,
            IntegralHeight = false
        };

        private readonly Button _save = new() { Text = "Save", DialogResult = DialogResult.OK };
        private readonly Button _cancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

        public AppOptions Result { get; private set; }

        public OptionsForm(AppOptions current)
        {
            Text = "Options";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 600);

            Result = Clone(current);

            BuildLayout();
            LoadFromOptions(Result);

            AcceptButton = _save;
            CancelButton = _cancel;

            _save.Click += (_, _) => SaveToResult();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // Top options
            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };

            // Ensure checkboxes can display full text
            _showFurigana.AutoSize = true;
            _allowHiragana.AutoSize = true;
            _focusOnly.AutoSize = true;
            _persistAnswers.AutoSize = true;

            top.Controls.Add(_showFurigana);
            top.Controls.Add(_allowHiragana);
            top.Controls.Add(_focusOnly);
            top.Controls.Add(_persistAnswers);

            // Conjugations checklist
            var group = new GroupBox
            {
                Dock = DockStyle.Fill,
                Text = "Enabled conjugations"
            };

            _conjugations.Dock = DockStyle.Fill;
            _conjugations.Font = new Font(Font.FontFamily, Font.Size + 1, FontStyle.Regular);

            group.Controls.Add(_conjugations);

            // Buttons
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = false,
            };

            _save.Height = 30;
            _cancel.Height = 30;

            buttons.Controls.Add(_save);
            buttons.Controls.Add(_cancel);

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(group, 0, 1);
            root.Controls.Add(buttons, 0, 2);

            Controls.Add(root);

            PopulateConjugations();
        }

        private void PopulateConjugations()
        {
            _conjugations.Items.Clear();

            foreach (var form in Enum.GetValues<ConjugationFormEnum>()
                .Where(form => form.ToString() != ConjugationNameConstants.DictionaryFormConst))
            {
                _conjugations.Items.Add(form, false);
            }
        }

        private void LoadFromOptions(AppOptions options)
        {
            _showFurigana.Checked = options.ShowFurigana;
            _allowHiragana.Checked = options.AllowHiragana;
            _focusOnly.Checked = options.FocusModeOnly;
            _persistAnswers.Checked = options.PersistUserAnswers;

            // mark enabled
            for (int i = 0; i < _conjugations.Items.Count; i++)
            {
                var form = (ConjugationFormEnum)_conjugations.Items[i]!;
                _conjugations.SetItemChecked(i, options.EnabledConjugations.Contains(form));
            }
        }

        private void SaveToResult()
        {
            Result.ShowFurigana = _showFurigana.Checked;
            Result.AllowHiragana = _allowHiragana.Checked;
            Result.FocusModeOnly = _focusOnly.Checked;
            Result.PersistUserAnswers = _persistAnswers.Checked;

            Result.EnabledConjugations.Clear();
            for (int i = 0; i < _conjugations.Items.Count; i++)
            {
                if (_conjugations.GetItemChecked(i))
                {
                    var form = (ConjugationFormEnum)_conjugations.Items[i]!;
                    Result.EnabledConjugations.Add(form);
                }
            }
        }

        private static AppOptions Clone(AppOptions o) => new()
        {
            SchemaVersion = o.SchemaVersion,
            ShowFurigana = o.ShowFurigana,
            AllowHiragana = o.AllowHiragana,
            FocusModeOnly = o.FocusModeOnly,
            PersistUserAnswers = o.PersistUserAnswers,
            EnabledConjugations = [.. o.EnabledConjugations]
        };
    }
}
