using System.Diagnostics;
using JapaneseVerbConjugation.SharedResources.Logic;

namespace JapaneseVerbConjugation.Forms
{
    public sealed class ImportPacksForm : Form
    {
        private readonly CheckBox _n5 = new() { Text = "Import N5 starter pack", Checked = true };
        private readonly CheckBox _n4 = new() { Text = "Import N4 starter pack" };
        private readonly CheckBox _custom = new() { Text = "Import Custom pack (custom.csv)" };

        private readonly Label _customPathLabel = new();
        private readonly Button _openCustom = new() { Text = "Open custom.csv" };

        private readonly RichTextBox _log = new()
        {
            ReadOnly = true,
            Dock = DockStyle.Fill,
            DetectUrls = false,
            HideSelection = false
        };

        private readonly Button _import = new() { Text = "Import" };
        private readonly Button _close = new() { Text = "Close" };

        private readonly Action<ImportSelection, Action<ImportLogLine>> _runImport;

        public ImportPacksForm(
            Action<ImportSelection, Action<ImportLogLine>> runImport,
            string customCsvPath)
        {
            _runImport = runImport ?? throw new ArgumentNullException(nameof(runImport));

            Text = "Import verbs";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(620, 560);

            _customPathLabel.Text = customCsvPath;
            _customPathLabel.AutoSize = true;
            _customPathLabel.Padding = new Padding(0, 0, 0, 6);

            _openCustom.AutoSize = false;
            _openCustom.MinimumSize = new Size(140, 34);
            _openCustom.Click += (_, _) => OpenFile(customCsvPath);

            _import.AutoSize = false;
            _close.AutoSize = false;
            _import.MinimumSize = new Size(100, 34);
            _close.MinimumSize = new Size(100, 34);

            _import.Click += (_, _) => DoImport(customCsvPath);
            _close.Click += (_, _) => Close();

            BuildLayout();
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
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // top checkboxes
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // custom path + open button
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));     // log
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // buttons

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true
            };
            top.Controls.Add(_n5);
            top.Controls.Add(_n4);
            top.Controls.Add(_custom);

            var customRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            var customText = new Label
            {
                Text = "Custom file:",
                AutoSize = true,
                Padding = new Padding(0, 6, 8, 0)
            };

            customRow.Controls.Add(customText);
            customRow.Controls.Add(_customPathLabel);
            customRow.Controls.Add(_openCustom);

            // Make the log look nicer
            _log.Font = new Font(Font.FontFamily, Font.Size + 1);
            _log.BackColor = SystemColors.Window;

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };
            buttons.Controls.Add(_close);
            buttons.Controls.Add(_import);

            root.Controls.Add(top, 0, 0);
            root.Controls.Add(customRow, 0, 1);
            root.Controls.Add(_log, 0, 2);
            root.Controls.Add(buttons, 0, 3);

            Controls.Add(root);
        }

        private async void DoImport(string customCsvPath)
        {
            _import.Enabled = false;

            AppendLog(new ImportLogLine("Starting import...", ImportLogColour.Neutral));

            var selection = new ImportSelection(
                ImportN5: _n5.Checked,
                ImportN4: _n4.Checked,
                ImportCustom: _custom.Checked,
                CustomPath: customCsvPath);

            await Task.Run(() => _runImport(selection, line =>
            {
                // marshal back to UI thread
                if (IsHandleCreated)
                    BeginInvoke(() => AppendLog(line));
            }));

            AppendLog(new ImportLogLine("Import complete.", ImportLogColour.Neutral));
            _import.Enabled = true;
        }

        private void AppendLog(ImportLogLine line)
        {
            // RichTextBox colour append
            _log.SelectionStart = _log.TextLength;
            _log.SelectionLength = 0;

            _log.SelectionColor = line.Colour switch
            {
                ImportLogColour.Added => Color.Green,
                ImportLogColour.Duplicate => Color.Orange,
                ImportLogColour.Error => Color.Red,
                _ => SystemColors.ControlText
            };

            _log.AppendText(line.Message + Environment.NewLine);
            _log.SelectionColor = SystemColors.ControlText;

            _log.ScrollToCaret();
        }

        private static void OpenFile(string path)
        {
            try
            {
                // Opens in default editor; on Windows this will usually be Notepad for .csv
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show($"Could not open file:\n{path}", "Open file failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    public readonly record struct ImportSelection(bool ImportN5, bool ImportN4, bool ImportCustom, string CustomPath);

    public enum ImportLogColour { Neutral, Added, Duplicate, Error }

    public readonly record struct ImportLogLine(string Message, ImportLogColour Colour);
}
