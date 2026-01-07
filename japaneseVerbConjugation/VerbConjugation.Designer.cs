namespace japaneseVerbConjugation
{
    partial class VerbConjugation
    {
        private System.ComponentModel.IContainer components = null;

        private TextBox dictionaryTextBox;

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
            checkVerbGroup = new Button();
            たべる = new Label();
            食べる = new Label();
            verbGroup = new GroupBox();
            不規則 = new RadioButton();
            一段 = new RadioButton();
            五段 = new RadioButton();
            conjugationTableLayout = new TableLayoutPanel();
            dictionaryTableLayout.SuspendLayout();
            dictionaryFormAndReadingLayout.SuspendLayout();
            verbGroup.SuspendLayout();
            SuspendLayout();
            // 
            // dictionaryTableLayout
            // 
            dictionaryTableLayout.ColumnCount = 1;
            dictionaryTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dictionaryTableLayout.Controls.Add(dictionaryFormAndReadingLayout, 0, 0);
            dictionaryTableLayout.Controls.Add(conjugationTableLayout, 0, 1);
            dictionaryTableLayout.Dock = DockStyle.Fill;
            dictionaryTableLayout.Location = new Point(0, 0);
            dictionaryTableLayout.Name = "dictionaryTableLayout";
            dictionaryTableLayout.RowCount = 3;
            dictionaryTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.5F));
            dictionaryTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28.166666F));
            dictionaryTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            dictionaryTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 5F));
            dictionaryTableLayout.Size = new Size(900, 600);
            dictionaryTableLayout.TabIndex = 2;
            // 
            // dictionaryFormAndReadingLayout
            // 
            dictionaryFormAndReadingLayout.ColumnCount = 1;
            dictionaryFormAndReadingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            dictionaryFormAndReadingLayout.Controls.Add(checkVerbGroup, 0, 3);
            dictionaryFormAndReadingLayout.Controls.Add(たべる, 0, 1);
            dictionaryFormAndReadingLayout.Controls.Add(食べる, 0, 0);
            dictionaryFormAndReadingLayout.Controls.Add(verbGroup, 0, 2);
            dictionaryFormAndReadingLayout.Dock = DockStyle.Fill;
            dictionaryFormAndReadingLayout.Location = new Point(0, 0);
            dictionaryFormAndReadingLayout.Margin = new Padding(0);
            dictionaryFormAndReadingLayout.Name = "dictionaryFormAndReadingLayout";
            dictionaryFormAndReadingLayout.RowCount = 4;
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 71.13402F));
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28.86598F));
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 63F));
            dictionaryFormAndReadingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            dictionaryFormAndReadingLayout.Size = new Size(900, 211);
            dictionaryFormAndReadingLayout.TabIndex = 2;
            // 
            // checkVerbGroup
            // 
            checkVerbGroup.Anchor = AnchorStyles.None;
            checkVerbGroup.Font = new Font("Yu Gothic UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkVerbGroup.Location = new Point(390, 176);
            checkVerbGroup.Name = "checkVerbGroup";
            checkVerbGroup.Size = new Size(119, 32);
            checkVerbGroup.TabIndex = 3;
            checkVerbGroup.Text = "Check Verb";
            checkVerbGroup.UseVisualStyleBackColor = true;
            checkVerbGroup.Click += OnCheckRequested;
            // 
            // たべる
            // 
            たべる.Anchor = AnchorStyles.None;
            たべる.AutoSize = true;
            たべる.Font = new Font("Yu Gothic UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            たべる.Location = new Point(421, 81);
            たべる.Name = "たべる";
            たべる.Size = new Size(58, 25);
            たべる.TabIndex = 6;
            たべる.Text = "たべる";
            // 
            // 食べる
            // 
            食べる.Anchor = AnchorStyles.None;
            食べる.AutoSize = true;
            食べる.Font = new Font("Yu Gothic UI", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            食べる.Location = new Point(373, 6);
            食べる.Name = "食べる";
            食べる.Size = new Size(153, 65);
            食べる.TabIndex = 7;
            食べる.Text = "食べる";
            食べる.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // verbGroup
            // 
            verbGroup.Anchor = AnchorStyles.None;
            verbGroup.Controls.Add(不規則);
            verbGroup.Controls.Add(一段);
            verbGroup.Controls.Add(五段);
            verbGroup.Font = new Font("Yu Gothic UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            verbGroup.Location = new Point(258, 117);
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
            // conjugationTableLayout
            // 
            conjugationTableLayout.AutoSize = true;
            conjugationTableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            conjugationTableLayout.ColumnCount = 2;
            conjugationTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            conjugationTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            conjugationTableLayout.Dock = DockStyle.Top;
            conjugationTableLayout.Location = new Point(3, 214);
            conjugationTableLayout.Name = "conjugationTableLayout";
            conjugationTableLayout.RowCount = 1;
            conjugationTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            conjugationTableLayout.Size = new Size(894, 0);
            conjugationTableLayout.TabIndex = 5;
            // 
            // VerbConjugation
            // 
            ClientSize = new Size(900, 600);
            Controls.Add(dictionaryTableLayout);
            Name = "VerbConjugation";
            Text = "Verb Conjugation";
            dictionaryTableLayout.ResumeLayout(false);
            dictionaryTableLayout.PerformLayout();
            dictionaryFormAndReadingLayout.ResumeLayout(false);
            dictionaryFormAndReadingLayout.PerformLayout();
            verbGroup.ResumeLayout(false);
            verbGroup.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel dictionaryTableLayout;
        private TableLayoutPanel dictionaryFormAndReadingLayout;
        private TextBox furiganaReading;
        private GroupBox verbGroup;
        private RadioButton 不規則;
        private RadioButton 一段;
        private RadioButton 五段;
        private Button checkVerbGroup;
        private TableLayoutPanel conjugationTableLayout;
        private Label たべる;
        private Label 食べる;
        private TableLayoutPanel checkVerbGroupLayout;
    }
}