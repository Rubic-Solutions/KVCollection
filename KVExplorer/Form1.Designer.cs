
namespace KVExplorer
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.DIALOG_SAVE = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusTitle = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusSpacer = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel3 = new System.Windows.Forms.Panel();
            this.BTN_TEST = new System.Windows.Forms.Button();
            this.BTN_OPEN = new System.Windows.Forms.Button();
            this.DGRID_FILTER = new System.Windows.Forms.TextBox();
            this.DGRID_LIST = new System.Windows.Forms.DataGridView();
            this.Keys = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BTN_CLOSE = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.EDIT_KEY5 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.EDIT_KEY4 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.EDIT_KEY3 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.EDIT_KEY2 = new System.Windows.Forms.TextBox();
            this.EDIT_VALUE_LEN = new System.Windows.Forms.Label();
            this.EDIT_BTN_SAV = new System.Windows.Forms.Button();
            this.EDIT_BTN_DEL = new System.Windows.Forms.Button();
            this.EDIT_BTN_NEW = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.EDIT_KEY1 = new System.Windows.Forms.TextBox();
            this.EDIT_VALUE = new System.Windows.Forms.TextBox();
            this.MESSAGE_LIST = new System.Windows.Forms.ListBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGRID_LIST)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // DIALOG_SAVE
            // 
            this.DIALOG_SAVE.OverwritePrompt = false;
            this.DIALOG_SAVE.Title = "Open Or Create Collection File";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusTitle,
            this.statusSpacer});
            this.statusStrip1.Location = new System.Drawing.Point(0, 534);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(1043, 26);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusTitle
            // 
            this.statusTitle.Name = "statusTitle";
            this.statusTitle.Size = new System.Drawing.Size(86, 20);
            this.statusTitle.Text = "KV Explorer";
            // 
            // statusSpacer
            // 
            this.statusSpacer.Name = "statusSpacer";
            this.statusSpacer.Size = new System.Drawing.Size(942, 20);
            this.statusSpacer.Spring = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.BackColor = System.Drawing.Color.Gainsboro;
            this.splitContainer2.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitContainer2.Location = new System.Drawing.Point(3, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.MESSAGE_LIST);
            this.splitContainer2.Size = new System.Drawing.Size(1040, 526);
            this.splitContainer2.SplitterDistance = 402;
            this.splitContainer2.TabIndex = 7;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Size = new System.Drawing.Size(1040, 402);
            this.splitContainer1.SplitterDistance = 230;
            this.splitContainer1.TabIndex = 6;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.BTN_TEST);
            this.panel3.Controls.Add(this.BTN_OPEN);
            this.panel3.Controls.Add(this.DGRID_FILTER);
            this.panel3.Controls.Add(this.DGRID_LIST);
            this.panel3.Controls.Add(this.BTN_CLOSE);
            this.panel3.Cursor = System.Windows.Forms.Cursors.Default;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(5);
            this.panel3.Size = new System.Drawing.Size(230, 402);
            this.panel3.TabIndex = 0;
            // 
            // BTN_TEST
            // 
            this.BTN_TEST.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BTN_TEST.BackColor = System.Drawing.Color.WhiteSmoke;
            this.BTN_TEST.FlatAppearance.BorderColor = System.Drawing.Color.Gainsboro;
            this.BTN_TEST.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_TEST.ForeColor = System.Drawing.Color.Black;
            this.BTN_TEST.Location = new System.Drawing.Point(170, 12);
            this.BTN_TEST.Name = "BTN_TEST";
            this.BTN_TEST.Size = new System.Drawing.Size(55, 29);
            this.BTN_TEST.TabIndex = 24;
            this.BTN_TEST.Text = "Test";
            this.BTN_TEST.UseVisualStyleBackColor = false;
            this.BTN_TEST.Click += new System.EventHandler(this.BTN_TEST_Click);
            // 
            // BTN_OPEN
            // 
            this.BTN_OPEN.BackColor = System.Drawing.Color.RoyalBlue;
            this.BTN_OPEN.FlatAppearance.BorderColor = System.Drawing.Color.MidnightBlue;
            this.BTN_OPEN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_OPEN.ForeColor = System.Drawing.Color.White;
            this.BTN_OPEN.Location = new System.Drawing.Point(5, 12);
            this.BTN_OPEN.Name = "BTN_OPEN";
            this.BTN_OPEN.Size = new System.Drawing.Size(58, 29);
            this.BTN_OPEN.TabIndex = 22;
            this.BTN_OPEN.Text = "Open";
            this.BTN_OPEN.UseVisualStyleBackColor = false;
            this.BTN_OPEN.Click += new System.EventHandler(this.BTN_OPEN_Click);
            // 
            // DGRID_FILTER
            // 
            this.DGRID_FILTER.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DGRID_FILTER.BackColor = System.Drawing.Color.FloralWhite;
            this.DGRID_FILTER.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DGRID_FILTER.Location = new System.Drawing.Point(5, 79);
            this.DGRID_FILTER.Name = "DGRID_FILTER";
            this.DGRID_FILTER.PlaceholderText = "Search";
            this.DGRID_FILTER.Size = new System.Drawing.Size(220, 20);
            this.DGRID_FILTER.TabIndex = 21;
            this.DGRID_FILTER.TextChanged += new System.EventHandler(this.DGRID_FILTER_TextChanged);
            // 
            // DGRID_LIST
            // 
            this.DGRID_LIST.AllowUserToAddRows = false;
            this.DGRID_LIST.AllowUserToDeleteRows = false;
            this.DGRID_LIST.AllowUserToResizeColumns = false;
            this.DGRID_LIST.AllowUserToResizeRows = false;
            this.DGRID_LIST.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DGRID_LIST.BackgroundColor = System.Drawing.SystemColors.Window;
            this.DGRID_LIST.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.DGRID_LIST.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DGRID_LIST.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.DGRID_LIST.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGRID_LIST.ColumnHeadersVisible = false;
            this.DGRID_LIST.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Keys});
            this.DGRID_LIST.Cursor = System.Windows.Forms.Cursors.Default;
            this.DGRID_LIST.EnableHeadersVisualStyles = false;
            this.DGRID_LIST.Location = new System.Drawing.Point(5, 105);
            this.DGRID_LIST.MultiSelect = false;
            this.DGRID_LIST.Name = "DGRID_LIST";
            this.DGRID_LIST.ReadOnly = true;
            this.DGRID_LIST.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.DGRID_LIST.RowHeadersVisible = false;
            this.DGRID_LIST.RowHeadersWidth = 51;
            this.DGRID_LIST.RowTemplate.Height = 29;
            this.DGRID_LIST.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.DGRID_LIST.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DGRID_LIST.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGRID_LIST.ShowCellErrors = false;
            this.DGRID_LIST.ShowCellToolTips = false;
            this.DGRID_LIST.ShowEditingIcon = false;
            this.DGRID_LIST.ShowRowErrors = false;
            this.DGRID_LIST.Size = new System.Drawing.Size(220, 290);
            this.DGRID_LIST.TabIndex = 19;
            this.DGRID_LIST.SelectionChanged += new System.EventHandler(this.DGRID_SelChanged);
            // 
            // Keys
            // 
            this.Keys.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Keys.DataPropertyName = "Value";
            this.Keys.HeaderText = "Keys";
            this.Keys.MinimumWidth = 6;
            this.Keys.Name = "Keys";
            this.Keys.ReadOnly = true;
            this.Keys.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // BTN_CLOSE
            // 
            this.BTN_CLOSE.BackColor = System.Drawing.Color.OrangeRed;
            this.BTN_CLOSE.FlatAppearance.BorderColor = System.Drawing.Color.DarkRed;
            this.BTN_CLOSE.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_CLOSE.ForeColor = System.Drawing.Color.White;
            this.BTN_CLOSE.Location = new System.Drawing.Point(5, 12);
            this.BTN_CLOSE.Name = "BTN_CLOSE";
            this.BTN_CLOSE.Size = new System.Drawing.Size(66, 29);
            this.BTN_CLOSE.TabIndex = 23;
            this.BTN_CLOSE.Text = "Close";
            this.BTN_CLOSE.UseVisualStyleBackColor = false;
            this.BTN_CLOSE.Visible = false;
            this.BTN_CLOSE.Click += new System.EventHandler(this.BTN_CLOSE_Click);
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.EDIT_KEY5);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.EDIT_KEY4);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.EDIT_KEY3);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.EDIT_KEY2);
            this.panel2.Controls.Add(this.EDIT_VALUE_LEN);
            this.panel2.Controls.Add(this.EDIT_BTN_SAV);
            this.panel2.Controls.Add(this.EDIT_BTN_DEL);
            this.panel2.Controls.Add(this.EDIT_BTN_NEW);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.EDIT_KEY1);
            this.panel2.Controls.Add(this.EDIT_VALUE);
            this.panel2.Cursor = System.Windows.Forms.Cursors.Default;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(806, 402);
            this.panel2.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 326);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 20);
            this.label6.TabIndex = 30;
            this.label6.Text = "Key5";
            // 
            // EDIT_KEY5
            // 
            this.EDIT_KEY5.BackColor = System.Drawing.Color.FloralWhite;
            this.EDIT_KEY5.Cursor = System.Windows.Forms.Cursors.Default;
            this.EDIT_KEY5.Location = new System.Drawing.Point(13, 349);
            this.EDIT_KEY5.Name = "EDIT_KEY5";
            this.EDIT_KEY5.ReadOnly = true;
            this.EDIT_KEY5.Size = new System.Drawing.Size(241, 27);
            this.EDIT_KEY5.TabIndex = 29;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 263);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 20);
            this.label5.TabIndex = 28;
            this.label5.Text = "Key4";
            // 
            // EDIT_KEY4
            // 
            this.EDIT_KEY4.BackColor = System.Drawing.Color.FloralWhite;
            this.EDIT_KEY4.Cursor = System.Windows.Forms.Cursors.Default;
            this.EDIT_KEY4.Location = new System.Drawing.Point(13, 286);
            this.EDIT_KEY4.Name = "EDIT_KEY4";
            this.EDIT_KEY4.ReadOnly = true;
            this.EDIT_KEY4.Size = new System.Drawing.Size(241, 27);
            this.EDIT_KEY4.TabIndex = 27;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 193);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 20);
            this.label4.TabIndex = 26;
            this.label4.Text = "Key3";
            // 
            // EDIT_KEY3
            // 
            this.EDIT_KEY3.BackColor = System.Drawing.Color.FloralWhite;
            this.EDIT_KEY3.Cursor = System.Windows.Forms.Cursors.Default;
            this.EDIT_KEY3.Location = new System.Drawing.Point(14, 216);
            this.EDIT_KEY3.Name = "EDIT_KEY3";
            this.EDIT_KEY3.ReadOnly = true;
            this.EDIT_KEY3.Size = new System.Drawing.Size(241, 27);
            this.EDIT_KEY3.TabIndex = 25;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 20);
            this.label3.TabIndex = 24;
            this.label3.Text = "Key2";
            // 
            // EDIT_KEY2
            // 
            this.EDIT_KEY2.BackColor = System.Drawing.Color.FloralWhite;
            this.EDIT_KEY2.Cursor = System.Windows.Forms.Cursors.Default;
            this.EDIT_KEY2.Location = new System.Drawing.Point(14, 148);
            this.EDIT_KEY2.Name = "EDIT_KEY2";
            this.EDIT_KEY2.ReadOnly = true;
            this.EDIT_KEY2.Size = new System.Drawing.Size(241, 27);
            this.EDIT_KEY2.TabIndex = 23;
            // 
            // EDIT_VALUE_LEN
            // 
            this.EDIT_VALUE_LEN.AutoSize = true;
            this.EDIT_VALUE_LEN.Location = new System.Drawing.Point(327, 58);
            this.EDIT_VALUE_LEN.Margin = new System.Windows.Forms.Padding(0);
            this.EDIT_VALUE_LEN.Name = "EDIT_VALUE_LEN";
            this.EDIT_VALUE_LEN.Size = new System.Drawing.Size(98, 20);
            this.EDIT_VALUE_LEN.TabIndex = 22;
            this.EDIT_VALUE_LEN.Text = "( Length = 0 )";
            // 
            // EDIT_BTN_SAV
            // 
            this.EDIT_BTN_SAV.BackColor = System.Drawing.Color.ForestGreen;
            this.EDIT_BTN_SAV.FlatAppearance.BorderColor = System.Drawing.Color.DarkGreen;
            this.EDIT_BTN_SAV.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EDIT_BTN_SAV.ForeColor = System.Drawing.Color.White;
            this.EDIT_BTN_SAV.Location = new System.Drawing.Point(87, 12);
            this.EDIT_BTN_SAV.Name = "EDIT_BTN_SAV";
            this.EDIT_BTN_SAV.Size = new System.Drawing.Size(68, 29);
            this.EDIT_BTN_SAV.TabIndex = 20;
            this.EDIT_BTN_SAV.Text = "Save";
            this.EDIT_BTN_SAV.UseVisualStyleBackColor = false;
            this.EDIT_BTN_SAV.Click += new System.EventHandler(this.EDIT_BTN_SAV_Click);
            // 
            // EDIT_BTN_DEL
            // 
            this.EDIT_BTN_DEL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EDIT_BTN_DEL.BackColor = System.Drawing.Color.OrangeRed;
            this.EDIT_BTN_DEL.FlatAppearance.BorderColor = System.Drawing.Color.DarkRed;
            this.EDIT_BTN_DEL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EDIT_BTN_DEL.ForeColor = System.Drawing.Color.White;
            this.EDIT_BTN_DEL.Location = new System.Drawing.Point(726, 46);
            this.EDIT_BTN_DEL.Name = "EDIT_BTN_DEL";
            this.EDIT_BTN_DEL.Size = new System.Drawing.Size(68, 29);
            this.EDIT_BTN_DEL.TabIndex = 19;
            this.EDIT_BTN_DEL.Text = "Delete";
            this.EDIT_BTN_DEL.UseVisualStyleBackColor = false;
            this.EDIT_BTN_DEL.Click += new System.EventHandler(this.EDIT_BTN_CNL_Click);
            // 
            // EDIT_BTN_NEW
            // 
            this.EDIT_BTN_NEW.BackColor = System.Drawing.Color.RoyalBlue;
            this.EDIT_BTN_NEW.FlatAppearance.BorderColor = System.Drawing.Color.MidnightBlue;
            this.EDIT_BTN_NEW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EDIT_BTN_NEW.ForeColor = System.Drawing.Color.White;
            this.EDIT_BTN_NEW.Location = new System.Drawing.Point(13, 12);
            this.EDIT_BTN_NEW.Name = "EDIT_BTN_NEW";
            this.EDIT_BTN_NEW.Size = new System.Drawing.Size(68, 29);
            this.EDIT_BTN_NEW.TabIndex = 18;
            this.EDIT_BTN_NEW.Text = "New";
            this.EDIT_BTN_NEW.UseVisualStyleBackColor = false;
            this.EDIT_BTN_NEW.Click += new System.EventHandler(this.EDIT_BTN_NEW_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(276, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 20);
            this.label2.TabIndex = 15;
            this.label2.Text = "Value";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 20);
            this.label1.TabIndex = 14;
            this.label1.Text = "Primary Key";
            // 
            // EDIT_KEY1
            // 
            this.EDIT_KEY1.BackColor = System.Drawing.Color.FloralWhite;
            this.EDIT_KEY1.Cursor = System.Windows.Forms.Cursors.Default;
            this.EDIT_KEY1.Location = new System.Drawing.Point(13, 81);
            this.EDIT_KEY1.Name = "EDIT_KEY1";
            this.EDIT_KEY1.ReadOnly = true;
            this.EDIT_KEY1.Size = new System.Drawing.Size(242, 27);
            this.EDIT_KEY1.TabIndex = 13;
            // 
            // EDIT_VALUE
            // 
            this.EDIT_VALUE.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EDIT_VALUE.BackColor = System.Drawing.Color.FloralWhite;
            this.EDIT_VALUE.CausesValidation = false;
            this.EDIT_VALUE.Cursor = System.Windows.Forms.Cursors.Default;
            this.EDIT_VALUE.Location = new System.Drawing.Point(276, 81);
            this.EDIT_VALUE.MinimumSize = new System.Drawing.Size(4, 150);
            this.EDIT_VALUE.Multiline = true;
            this.EDIT_VALUE.Name = "EDIT_VALUE";
            this.EDIT_VALUE.ReadOnly = true;
            this.EDIT_VALUE.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.EDIT_VALUE.Size = new System.Drawing.Size(518, 311);
            this.EDIT_VALUE.TabIndex = 12;
            this.EDIT_VALUE.WordWrap = false;
            // 
            // MESSAGE_LIST
            // 
            this.MESSAGE_LIST.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MESSAGE_LIST.Cursor = System.Windows.Forms.Cursors.Default;
            this.MESSAGE_LIST.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MESSAGE_LIST.FormattingEnabled = true;
            this.MESSAGE_LIST.IntegralHeight = false;
            this.MESSAGE_LIST.ItemHeight = 20;
            this.MESSAGE_LIST.Location = new System.Drawing.Point(0, 0);
            this.MESSAGE_LIST.Name = "MESSAGE_LIST";
            this.MESSAGE_LIST.Size = new System.Drawing.Size(1040, 120);
            this.MESSAGE_LIST.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 560);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KV Explorer";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGRID_LIST)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SaveFileDialog DIALOG_SAVE;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusSpacer;
        private System.Windows.Forms.ToolStripStatusLabel statusTitle;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox MESSAGE_LIST;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox EDIT_KEY1;
        private System.Windows.Forms.TextBox EDIT_VALUE;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox DGRID_FILTER;
        private System.Windows.Forms.DataGridView DGRID_LIST;
        private System.Windows.Forms.DataGridViewTextBoxColumn Keys;
        private System.Windows.Forms.Button EDIT_BTN_NEW;
        private System.Windows.Forms.Button EDIT_BTN_SAV;
        private System.Windows.Forms.Button EDIT_BTN_DEL;
        private System.Windows.Forms.Label EDIT_VALUE_LEN;
        private System.Windows.Forms.Button BTN_TEST;
        private System.Windows.Forms.Button BTN_CLOSE;
        private System.Windows.Forms.Button BTN_OPEN;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox EDIT_KEY2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox EDIT_KEY5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox EDIT_KEY4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox EDIT_KEY3;
    }
}

