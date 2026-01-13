using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JapaneseVerbConjugation.Forms
{
    public sealed class ImportPacksForm : Form
    {
        private readonly CheckBox _n5 = new() { Text = "Import N5 starter pack", Checked = true };
        private readonly CheckBox _n4 = new() { Text = "Import N4 starter pack" };
        private readonly CheckBox _custom = new() { Text = "Import custom CSV/PSV file(s)..." };

        private readonly ListBox _customFiles = new()
        {
            Height = 120
        };

        private readonly Button _pickFiles = new() { Text = "Choose file(s)...", Enabled = false };
        private readonly Button _import = new() { Text = "Import", DialogResult = DialogResult.OK };
        private readonly Button _cancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

        public bool ImportN5 => _n5.Checked;
        public bool ImportN4 => _n4.Checked;
        public bool ImportCustom => _custom.Checked;

        public ImportPacksForm()
        {
            Text = "Import verbs";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new System.Drawing.Size(520, 500);

            BuildLayout();

            _custom.CheckedChanged += (_, _) =>
            {
                _pickFiles.Enabled = _custom.Checked;
                _customFiles.Enabled = _custom.Checked;
                if (!_custom.Checked)
                    _customFiles.Items.Clear();
            };

            _pickFiles.Click += (_, _) => ChooseFiles();

            AcceptButton = _import;
            CancelButton = _cancel;
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(12),
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Height = 85
            };

            top.Controls.Add(_n5);
            top.Controls.Add(_n4);
            top.Controls.Add(_custom);

            var customArea = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            customArea.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            customArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _customFiles.Dock = DockStyle.Fill;
            _customFiles.Enabled = false;

            _pickFiles.Enabled = false;
            _pickFiles.AutoSize = false;
            _pickFiles.MinimumSize = new Size(140, 34);

            customArea.Controls.Add(_pickFiles, 0, 0);
            customArea.Controls.Add(_customFiles, 0, 1);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false
            };

            _import.AutoSize = false;
            _cancel.AutoSize = false;
            _import.MinimumSize = new Size(100, 34);
            _cancel.MinimumSize = new Size(100, 34);

            buttons.Controls.Add(_import);
            buttons.Controls.Add(_cancel);

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(
                new Label { 
                    Text = "Custom files (optional):",
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0, 10, 0, 4),
                    Height = 40 },
                0, 1);
            root.Controls.Add(customArea, 0, 2);
            root.Controls.Add(buttons, 0, 3);

            Controls.Add(root);
        }

        private void ChooseFiles()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select CSV/PSV files to import",
                Filter = "CSV or PSV (*.csv;*.psv)|*.csv;*.psv|All files (*.*)|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            _customFiles.Items.Clear();
            foreach (var f in ofd.FileNames)
                _customFiles.Items.Add(f);
        }
    }
}
