namespace JapaneseVerbConjugation
{
    partial class VerbConjugationForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dictionaryTableLayout = new TableLayoutPanel();
            dictionaryFormAndReadingLayout = new TableLayoutPanel();
            checkVerbGroupButton = new Button();
            furiganaReading = new Label();
            currentVerb = new Label();
            verbGroup = new GroupBox();
            不規則 = new RadioButton();
            一段 = new RadioButton();
            五段 = new RadioButton();
            conjugationScrollPanel = new Panel();
            conjugationTableLayout = new TableLayoutPanel();
            controlFlowButtons = new TableLayoutPanel();
            optionsButton = new Button();
            importButton = new Button();
            clearButton = new Button();
            prevVerbButton = new Button();
            nextVerbButton = new Button();
            dictionaryTableLayout.SuspendLayout();
            dictionaryFormAndReadingLayout.SuspendLayout();
            verbGroup.SuspendLayout();
            conjugationScrollPanel.SuspendLayout();
            controlFlowButtons.SuspendLayout();
            SuspendLayout();
            // 
            // dictionaryTableLayout
            // 
            dictionaryTableLayout.ColumnCount = 1;
            dictionaryTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dictionaryTableLayout.Controls.Add(dictionaryFormAndReadingLayout, 0, 0);
            dictionaryTableLayout.Controls.Add(conjugationScrollPanel, 0, 1);
            dictionaryTableLayout.Controls.Add(controlFlowButtons, 0, 2);
            dictionaryTableLayout.Dock = DockStyle.Fill;
            dictionaryTableLayout.Location = new Point(0, 0);
            dictionaryTableLayout.Name = "dictionaryTableLayout";
            dictionaryTableLayout.RowCount = 3;
            dictionaryTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 34.3333321F));
            dictionaryTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 51.5F));
            dictionaryTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 13.9534893F));
            dictionaryTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            dictionaryTableLayout.Size = new Size(900, 600);
            dictionaryTableLayout.TabIndex = 2;
            // 
            // dictionaryFormAndReadingLayout
            // 
            dictionaryFormAndReadingLayout.ColumnCount = 1;
            dictionaryFormAndReadingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dictionaryFormAndReadingLayout.Controls.Add(checkVerbGroupButton, 0, 3);
            dictionaryFormAndReadingLayout.Controls.Add(furiganaReading, 0, 1);
            dictionaryFormAndReadingLayout.Controls.Add(currentVerb, 0, 0);
            dictionaryFormAndReadingLayout.Controls.Add(verbGroup, 0, 2);
            dictionaryFormAndReadingLayout.Dock = DockStyle.Fill;
            dictionaryFormAndReadingLayout.Location = new Point(0, 0);
            dictionaryFormAndReadingLayout.Margin = new Padding(0);
            dictionaryFormAndReadingLayout.Name = "dictionaryFormAndReadingLayout";
            dictionaryFormAndReadingLayout.RowCount = 4;
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 66.6666641F));
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            dictionaryFormAndReadingLayout.Size = new Size(900, 206);
            dictionaryFormAndReadingLayout.TabIndex = 2;
            // 
            // checkVerbGroupButton
            // 
            checkVerbGroupButton.Anchor = AnchorStyles.None;
            checkVerbGroupButton.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkVerbGroupButton.Location = new Point(390, 168);
            checkVerbGroupButton.Name = "checkVerbGroupButton";
            checkVerbGroupButton.Size = new Size(119, 32);
            checkVerbGroupButton.TabIndex = 3;
            checkVerbGroupButton.Text = "Check Group";
            checkVerbGroupButton.UseVisualStyleBackColor = true;
            checkVerbGroupButton.Click += CheckVerbGroup;
            // 
            // furiganaReading
            // 
            furiganaReading.Anchor = AnchorStyles.None;
            furiganaReading.AutoSize = true;
            furiganaReading.Font = new Font("Yu Gothic UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            furiganaReading.Location = new Point(450, 71);
            furiganaReading.Name = "furiganaReading";
            furiganaReading.Size = new Size(0, 25);
            furiganaReading.TabIndex = 6;
            // 
            // currentVerb
            // 
            currentVerb.Anchor = AnchorStyles.None;
            currentVerb.AutoSize = true;
            currentVerb.Font = new Font("Yu Gothic UI", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            currentVerb.Location = new Point(450, 1);
            currentVerb.Name = "currentVerb";
            currentVerb.Size = new Size(0, 65);
            currentVerb.TabIndex = 7;
            currentVerb.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // verbGroup
            // 
            verbGroup.Anchor = AnchorStyles.None;
            verbGroup.Controls.Add(不規則);
            verbGroup.Controls.Add(一段);
            verbGroup.Controls.Add(五段);
            verbGroup.Font = new Font("Yu Gothic UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            verbGroup.Location = new Point(258, 107);
            verbGroup.Name = "verbGroup";
            verbGroup.Size = new Size(384, 48);
            verbGroup.TabIndex = 3;
            verbGroup.TabStop = false;
            verbGroup.Text = "Verb Group";
            // 
            // 不規則
            // 
            不規則.Anchor = AnchorStyles.Right;
            不規則.AutoSize = true;
            不規則.Font = new Font("Yu Gothic UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            不規則.Location = new Point(290, 13);
            不規則.Name = "不規則";
            不規則.Size = new Size(90, 32);
            不規則.TabIndex = 2;
            不規則.TabStop = true;
            不規則.Text = "不規則";
            不規則.UseVisualStyleBackColor = true;
            // 
            // 一段
            // 
            一段.Anchor = AnchorStyles.None;
            一段.AutoSize = true;
            一段.Font = new Font("Yu Gothic UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            一段.Location = new Point(148, 13);
            一段.Name = "一段";
            一段.Size = new Size(70, 32);
            一段.TabIndex = 1;
            一段.TabStop = true;
            一段.Text = "一段";
            一段.UseVisualStyleBackColor = true;
            // 
            // 五段
            // 
            五段.Anchor = AnchorStyles.Left;
            五段.AutoSize = true;
            五段.Font = new Font("Yu Gothic UI", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            五段.Location = new Point(6, 13);
            五段.Name = "五段";
            五段.Size = new Size(70, 32);
            五段.TabIndex = 0;
            五段.TabStop = true;
            五段.Text = "五段";
            五段.UseVisualStyleBackColor = true;
            // 
            // conjugationScrollPanel
            // 
            conjugationScrollPanel.AutoScroll = true;
            conjugationScrollPanel.Controls.Add(conjugationTableLayout);
            conjugationScrollPanel.Dock = DockStyle.Fill;
            conjugationScrollPanel.Location = new Point(3, 209);
            conjugationScrollPanel.Name = "conjugationScrollPanel";
            conjugationScrollPanel.Size = new Size(894, 303);
            conjugationScrollPanel.TabIndex = 3;
            // 
            // conjugationTableLayout
            // 
            conjugationTableLayout.AutoSize = true;
            conjugationTableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            conjugationTableLayout.ColumnCount = 2;
            conjugationTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            conjugationTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            conjugationTableLayout.Dock = DockStyle.Top;
            conjugationTableLayout.Location = new Point(0, 0);
            conjugationTableLayout.Name = "conjugationTableLayout";
            conjugationTableLayout.RowCount = 1;
            conjugationTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            conjugationTableLayout.Size = new Size(894, 0);
            conjugationTableLayout.TabIndex = 6;
            // 
            // controlFlowButtons
            // 
            controlFlowButtons.ColumnCount = 5;
            controlFlowButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            controlFlowButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            controlFlowButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            controlFlowButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            controlFlowButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            controlFlowButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            controlFlowButtons.Controls.Add(optionsButton, 0, 0);
            controlFlowButtons.Controls.Add(importButton, 1, 0);
            controlFlowButtons.Controls.Add(clearButton, 2, 0);
            controlFlowButtons.Controls.Add(prevVerbButton, 3, 0);
            controlFlowButtons.Controls.Add(nextVerbButton, 4, 0);
            controlFlowButtons.Location = new Point(3, 518);
            controlFlowButtons.Name = "controlFlowButtons";
            controlFlowButtons.RowCount = 1;
            controlFlowButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            controlFlowButtons.Size = new Size(894, 79);
            controlFlowButtons.TabIndex = 4;
            // 
            // optionsButton
            // 
            optionsButton.Anchor = AnchorStyles.Left;
            optionsButton.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            optionsButton.Location = new Point(5, 21);
            optionsButton.Margin = new Padding(5, 3, 3, 3);
            optionsButton.Name = "optionsButton";
            optionsButton.Size = new Size(90, 36);
            optionsButton.TabIndex = 5;
            optionsButton.Text = "Options";
            optionsButton.UseVisualStyleBackColor = true;
            optionsButton.Click += ChangeUserOptions;
            // 
            // importButton
            // 
            importButton.Anchor = AnchorStyles.Left;
            importButton.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            importButton.Location = new Point(110, 21);
            importButton.Margin = new Padding(10, 3, 3, 3);
            importButton.Name = "importButton";
            importButton.Size = new Size(90, 36);
            importButton.TabIndex = 6;
            importButton.Text = "Import";
            importButton.UseVisualStyleBackColor = true;
            importButton.Click += ShowImportDialog;
            // 
            // prevVerbButton
            // 
            prevVerbButton.Anchor = AnchorStyles.Right;
            prevVerbButton.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            prevVerbButton.Location = new Point(701, 21);
            prevVerbButton.Name = "prevVerbButton";
            prevVerbButton.Size = new Size(90, 36);
            prevVerbButton.TabIndex = 7;
            prevVerbButton.Text = "Prev Verb";
            prevVerbButton.UseVisualStyleBackColor = true;
            prevVerbButton.Click += SkipToPreviousVerb;
            // 
            // nextVerbButton
            // 
            nextVerbButton.Anchor = AnchorStyles.Right;
            nextVerbButton.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            nextVerbButton.Location = new Point(801, 21);
            nextVerbButton.Name = "nextVerbButton";
            nextVerbButton.Size = new Size(90, 36);
            nextVerbButton.TabIndex = 8;
            nextVerbButton.Text = "Next Verb";
            nextVerbButton.UseVisualStyleBackColor = true;
            nextVerbButton.Click += SkipToNextVerb;
            // 
            // clearButton
            // 
            clearButton.Anchor = AnchorStyles.Right;
            clearButton.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            clearButton.Location = new Point(601, 21);
            clearButton.Name = "clearButton";
            clearButton.Size = new Size(90, 36);
            clearButton.TabIndex = 6;
            clearButton.Text = "Clear";
            clearButton.UseVisualStyleBackColor = true;
            clearButton.Click += ClearAnswers;
            // 
            // VerbConjugationForm
            // 
            ClientSize = new Size(900, 600);
            Controls.Add(dictionaryTableLayout);
            Name = "VerbConjugationForm";
            Text = "Verb Conjugation";
            dictionaryTableLayout.ResumeLayout(false);
            dictionaryFormAndReadingLayout.ResumeLayout(false);
            dictionaryFormAndReadingLayout.PerformLayout();
            verbGroup.ResumeLayout(false);
            verbGroup.PerformLayout();
            conjugationScrollPanel.ResumeLayout(false);
            conjugationScrollPanel.PerformLayout();
            controlFlowButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel dictionaryTableLayout;
        private TableLayoutPanel dictionaryFormAndReadingLayout;
        private GroupBox verbGroup;
        private RadioButton 不規則;
        private RadioButton 一段;
        private RadioButton 五段;
        private Button checkVerbGroupButton;
        private Label furiganaReading;
        private Label currentVerb;
        private Panel conjugationScrollPanel;
        private TableLayoutPanel conjugationTableLayout;
        private TableLayoutPanel controlFlowButtons;
        private Button optionsButton;
        private Button importButton;
        private Button clearButton;
        private Button prevVerbButton;
        private Button nextVerbButton;
    }
}