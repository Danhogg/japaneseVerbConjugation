using japaneseVerbConjugation.Controls;
using japaneseVerbConjugation.Enums;
using japaneseVerbConjugation.Models.ModelsForSerialising;

namespace japaneseVerbConjugation
{
    public partial class VerbConjugation : Form
    {
        // Holds the persistent learning state for each conjugation row.
        // This is intentionally separate from UI controls for save/load later.
        private readonly List<ConjugationEntryState> _entryStates = [];

        public VerbConjugation()
        {
            InitializeComponent();
            MinimumSize = new Size(740, 600);
            conjugationTableLayout.SuspendLayout();
            WireUpDynamicContent();
            conjugationTableLayout.ResumeLayout();
        }

        private void WireUpDynamicContent()
        {
            AddConjugationEntry(ConjugationForm.TeForm);

            AddConjugationEntry(ConjugationForm.PastPlain);

            AddConjugationEntry(ConjugationForm.PastPolite);
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

        private void OnCheckRequested(object? sender, EventArgs e)
        {
            if (sender == checkVerbGroup)
            {
                if (五段.Checked)
                {
                    五段.ForeColor = Color.Green;
                }
                else if (一段.Checked)
                {
                    一段.ForeColor = Color.Green;
                }
                else if (不規則.Checked)
                {
                    不規則.ForeColor = Color.Green;
                }
                //TODO add correct behaviour
            }
            if (sender is not ConjugationEntryControl entry)
                return;

            var conjugationEntryState = entry.ConjugationEntryState;

            // TEMP logic — engine plugs in here later
            switch (conjugationEntryState.ConjugationForm)
            {
                case ConjugationForm.TeForm:
                    conjugationEntryState.Result = ConjugationResult.Correct;
                    entry.RenderResult();
                    break;

                case ConjugationForm.PastPlain:
                    conjugationEntryState.Result = ConjugationResult.Incorrect;
                    entry.RenderResult();
                    break;

                case ConjugationForm.PastPolite:
                    conjugationEntryState.Result = ConjugationResult.Close;
                    entry.RenderResult();
                    break;
            }
        }
    }
}
