using JapaneseVerbConjugation.Enums;
using JapaneseVerbConjugation.Models.ModelsForSerialising;
using JapaneseVerbConjugation.SharedResources.Constants;
using JapaneseVerbConjugation.SharedResources.Methods;

namespace JapaneseVerbConjugation.Controls
{
    public sealed class ConjugationEntryControl : UserControl
    {
        private readonly Label _label = new();
        private readonly TextBox _inputTextArea = new();

        private readonly Button _checkButton = new();
        public event EventHandler? CheckRequested;
        
        private readonly Label _resultLabel = new();

        private readonly Button _hintButton = new();
        public event EventHandler? HintRequested;



        public ConjugationEntryState ConjugationEntryState { get; private set; }

        public ConjugationEntryControl(ConjugationEntryState state)
        {
            ConjugationEntryState = state ?? throw new ArgumentNullException(nameof(state));

            Height = 36;
            Dock = DockStyle.Fill;

            BuildLayout();
            BindState();
        }

        private void BindState()
        {
            _label.Text = ConjugationEntryState.ConjugationForm.ToDisplayLabel();
            _inputTextArea.Text = ConjugationEntryState.UserInput;
            RenderResult();
        }

        private void BuildLayout()
        {
            SuspendLayout();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); // label
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // input area
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70)); // check button
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 55)); // hint
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20)); //result

            _label.Dock = DockStyle.Fill;
            _label.TextAlign = ContentAlignment.MiddleLeft;

            _inputTextArea.Dock = DockStyle.Fill;
            _inputTextArea.Multiline = true;
            _inputTextArea.TextAlign = HorizontalAlignment.Left;
            _inputTextArea.Font = new Font("Yu Gothic UI", 14F, FontStyle.Bold);
            _inputTextArea.TextChanged += (_, _) =>
                ConjugationEntryState.UserInput = _inputTextArea.Text;

            _checkButton.Text = "Check";
            _checkButton.Dock = DockStyle.Fill;
            _checkButton.Click += (_, _) => CheckRequested?.Invoke(this, EventArgs.Empty);

            _hintButton.Text = "Hint";
            _hintButton.Dock = DockStyle.Fill;
            _hintButton.Click += (_, _) => HintRequested?.Invoke(this, EventArgs.Empty);

            _resultLabel.Dock = DockStyle.Fill;
            _resultLabel.TextAlign = ContentAlignment.MiddleCenter;
            _resultLabel.Font = new Font(Font, FontStyle.Bold);

            layout.Controls.Add(_label, 0, 0);
            layout.Controls.Add(_inputTextArea, 1, 0);
            layout.Controls.Add(_checkButton, 2, 0);
            layout.Controls.Add(_hintButton, 3, 0);
            layout.Controls.Add(_resultLabel, 4, 0);

            Controls.Add(layout);
            ResumeLayout();
        }

        public void RefreshFromState()
        {
            _inputTextArea.Text = ConjugationEntryState.UserInput ?? string.Empty;
            RenderResult();
        }

        public void RenderResult()
        {
            switch (ConjugationEntryState.Result)
            {
                case ConjugationResult.Correct:
                    _resultLabel.Text = "✓";
                    _resultLabel.ForeColor = ColourConstants.LabelCorrectColour;
                    _inputTextArea.BackColor = ColourConstants.TextAreaCorrectColour;
                    _inputTextArea.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ConjugationResult.Incorrect:
                    _resultLabel.Text = "✗";
                    _resultLabel.ForeColor = ColourConstants.LabelIncorrectColour;
                    _inputTextArea.BackColor = ColourConstants.TextAreaIncorrectColour;
                    _inputTextArea.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case ConjugationResult.Close:
                    _resultLabel.Text = "~";
                    _resultLabel.ForeColor = ColourConstants.LabelCloseColour;
                    _inputTextArea.BackColor = ColourConstants.TextAreaCloseColour;
                    _inputTextArea.BorderStyle = BorderStyle.FixedSingle;
                    break;

                default:
                    _resultLabel.Text = string.Empty;
                    _inputTextArea.BackColor = SystemColors.Window;
                    _inputTextArea.BorderStyle = BorderStyle.FixedSingle;
                    _checkButton.Enabled = true;
                    break;
            }

            _checkButton.Enabled = ConjugationEntryState.Result != ConjugationResult.Correct;
        }
    }
}
