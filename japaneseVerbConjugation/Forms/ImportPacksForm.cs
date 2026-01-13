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
            SuspendLayout();

            // Root layout: stable rows + stable spacing
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12)
            };

            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // options group
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));     // log
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));         // bottom buttons

            // --- Options group ---
            var optionsGroup = new GroupBox
            {
                Text = "What do you want to import?",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(10)
            };

            var optionsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                AutoSize = true
            };

            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            optionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Checkboxes
            _n5.AutoSize = true;
            _n4.AutoSize = true;
            _custom.AutoSize = true;

            // Custom buttons (no path label)
            _openCustom.AutoSize = false;
            _openCustom.MinimumSize = new Size(140, 34);

            var openFolder = new Button
            {
                Text = "Open data folder",
                AutoSize = false,
                MinimumSize = new Size(140, 34)
            };
            openFolder.Click += (_, _) =>
            {
                // Opens AppData folder where custom.csv lives
                var dir = Path.GetDirectoryName(_customPathLabel.Text); // you already store the path in this label
                if (string.IsNullOrWhiteSpace(dir)) return;

                try
                {
                    Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
                }
                catch
                {
                    MessageBox.Show(this, $"Could not open folder:\n{dir}", "Open folder failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // Row 0-2: packs
            optionsLayout.Controls.Add(_n5, 0, 0);
            optionsLayout.SetColumnSpan(_n5, 2);

            optionsLayout.Controls.Add(_n4, 0, 1);
            optionsLayout.SetColumnSpan(_n4, 2);

            optionsLayout.Controls.Add(_custom, 0, 2);
            optionsLayout.SetColumnSpan(_custom, 2);

            // Row 3: custom buttons aligned to the left
            var customButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(22, 0, 0, 0) // indent under the checkbox
            };
            customButtons.Controls.Add(_openCustom);
            customButtons.Controls.Add(openFolder);

            optionsLayout.Controls.Add(customButtons, 0, 3);
            optionsLayout.SetColumnSpan(customButtons, 2);

            optionsGroup.Controls.Add(optionsLayout);

            // --- Log ---
            _log.Dock = DockStyle.Fill;
            _log.ReadOnly = true;
            _log.BorderStyle = BorderStyle.FixedSingle;
            _log.BackColor = SystemColors.Window;
            _log.Font = new Font(Font.FontFamily, Font.Size); // don’t inflate; keeps consistent sizing

            // --- Bottom buttons ---
            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            _import.AutoSize = false;
            _close.AutoSize = false;
            _import.MinimumSize = new Size(100, 34);
            _close.MinimumSize = new Size(100, 34);

            buttons.Controls.Add(_close);
            buttons.Controls.Add(_import);

            // Add to root
            root.Controls.Add(optionsGroup, 0, 0);
            root.Controls.Add(_log, 0, 1);
            root.Controls.Add(buttons, 0, 2);

            Controls.Add(root);

            ResumeLayout();
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
            
            // Force immediate UI update so messages appear one by one
            _log.Refresh();
            Application.DoEvents();
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
