using japaneseVerbConjugation.Enums;
using japaneseVerbConjugation.Models.ModelsForSerialising;
using japaneseVerbConjugation.SharedMethods;

namespace japaneseVerbConjugation.Controls
{
    public sealed class ConjugationEntryControl : UserControl
    {
        private readonly Label _label = new();
        private readonly TextBox _inputTextArea = new();
        private readonly Button _checkButton = new();
        private readonly Label _resultLabel = new();

        public event EventHandler? CheckRequested;


        public ConjugationEntryState ConjugationEntryState { get; private set; }

        public ConjugationEntryControl(ConjugationEntryState state)
        {
            ConjugationEntryState = state ?? throw new ArgumentNullException(nameof(state));

            Height = 40;
            Dock = DockStyle.Top;

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
                ColumnCount = 4,
                RowCount = 1
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            _label.Dock = DockStyle.Fill;
            _label.TextAlign = ContentAlignment.MiddleLeft;

            _inputTextArea.Dock = DockStyle.Fill;
            _inputTextArea.Multiline = true;
            _inputTextArea.TextAlign = HorizontalAlignment.Left;
            _inputTextArea.TextChanged += (_, _) =>
                ConjugationEntryState.UserInput = _inputTextArea.Text;

            _checkButton.Text = "Check";
            _checkButton.Dock = DockStyle.Fill;
            _checkButton.Click += (_, _) => CheckRequested?.Invoke(this, EventArgs.Empty);

            _resultLabel.Dock = DockStyle.Fill;
            _resultLabel.TextAlign = ContentAlignment.MiddleCenter;
            _resultLabel.Font = new Font(Font, FontStyle.Bold);

            layout.Controls.Add(_label, 0, 0);
            layout.Controls.Add(_inputTextArea, 1, 0);
            layout.Controls.Add(_checkButton, 2, 0);
            layout.Controls.Add(_resultLabel, 3, 0);

            Controls.Add(layout);
            ResumeLayout();
        }

        public void RenderResult()
        {
            switch (ConjugationEntryState.Result)
            {
                case ConjugationResult.Correct:
                    _resultLabel.Text = "✓";
                    _resultLabel.ForeColor = Color.Green;
                    break;

                case ConjugationResult.Incorrect:
                    _resultLabel.Text = "✗";
                    _resultLabel.ForeColor = Color.Red;
                    break;

                case ConjugationResult.Close:
                    _resultLabel.Text = "~";
                    _resultLabel.ForeColor = Color.Orange;
                    break;

                default:
                    _resultLabel.Text = string.Empty;
                    break;
            }
        }
    }
}
