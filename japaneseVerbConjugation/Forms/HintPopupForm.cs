namespace JapaneseVerbConjugation.Controls
{
    public sealed class HintPopupForm : Form
    {
        private readonly Label _label = new();
        private readonly Button _revealButton = new();

        private readonly string _masked;
        private readonly string _full;
        private bool _revealed;

        private HintPopupForm(string title, string masked, string full, Font baseFont)
        {
            _masked = masked;
            _full = full;
            _revealed = false;

            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            // Bigger, consistent popup size
            ClientSize = new Size(300, 150);

            // Label
            _label.Dock = DockStyle.Fill;
            _label.TextAlign = ContentAlignment.MiddleCenter;
            _label.Padding = new Padding(16);

            // Slightly larger than app font, keeps family consistent
            _label.Font = new Font(baseFont.FontFamily, baseFont.Size + 6, FontStyle.Bold);
            _label.Text = _masked;


            if (Text == "No hint available")
            {
                // No hint case: disable reveal button right away
                _revealButton.Enabled = false;
                _revealButton.Text = "No hint available";
            };

            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                ColumnCount = 1,
                RowCount = 1
            };

            buttonPanel.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            _revealButton.Text = "Show answer";
            _revealButton.Width = 120;
            _revealButton.Height = 32;
            _revealButton.Click += (_, _) => Reveal();
            _revealButton.Anchor = AnchorStyles.None;

            buttonPanel.Controls.Add(_revealButton, 0, 0);

            Controls.Add(_label);
            Controls.Add(buttonPanel);

            // ESC closes
            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    Close();
            };
        }

        private void Reveal()
        {
            if (_revealed) return;

            _revealed = true;
            _label.Text = _full;
            _revealButton.Enabled = false;
            _revealButton.Text = "Answer shown";
        }

        public static void Show(IWin32Window owner, string title, string masked, string full, Font baseFont)
        {
            using var f = new HintPopupForm(title, masked, full, baseFont);
            f.ShowDialog(owner);
        }
    }
}
