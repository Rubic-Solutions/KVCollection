
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.KV_FILENAME = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.DIALOG_SAVE = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusTitle = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusSpacer = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel3 = new System.Windows.Forms.Panel();
            this.DGRID_FILTER = new System.Windows.Forms.TextBox();
            this.DGRID_LIST = new System.Windows.Forms.DataGridView();
            this.Keys = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.EDIT_BTN_SAV = new System.Windows.Forms.Button();
            this.EDIT_BTN_DEL = new System.Windows.Forms.Button();
            this.EDIT_BTN_NEW = new System.Windows.Forms.Button();
            this.EDIT_ELAPSED = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.EDIT_KEY = new System.Windows.Forms.TextBox();
            this.EDIT_VALUE = new System.Windows.Forms.TextBox();
            this.MESSAGE_LIST = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
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
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.KV_FILENAME);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1043, 35);
            this.panel1.TabIndex = 3;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.WhiteSmoke;
            this.button3.FlatAppearance.BorderColor = System.Drawing.Color.Gainsboro;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.ForeColor = System.Drawing.Color.Black;
            this.button3.Location = new System.Drawing.Point(135, 3);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(93, 29);
            this.button3.TabIndex = 6;
            this.button3.Text = "Close";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.Gainsboro;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(997, 2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(34, 29);
            this.button2.TabIndex = 5;
            this.button2.Text = "X";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // KV_FILENAME
            // 
            this.KV_FILENAME.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.KV_FILENAME.Location = new System.Drawing.Point(234, 3);
            this.KV_FILENAME.Name = "KV_FILENAME";
            this.KV_FILENAME.ReadOnly = true;
            this.KV_FILENAME.Size = new System.Drawing.Size(758, 27);
            this.KV_FILENAME.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Gainsboro;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(8, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(133, 29);
            this.button1.TabIndex = 3;
            this.button1.Text = "Open / Create";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
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
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 35);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 499);
            this.splitter1.TabIndex = 6;
            this.splitter1.TabStop = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.BackColor = System.Drawing.Color.Gainsboro;
            this.splitContainer2.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitContainer2.Location = new System.Drawing.Point(3, 41);
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
            this.splitContainer2.Size = new System.Drawing.Size(1040, 485);
            this.splitContainer2.SplitterDistance = 371;
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
            this.splitContainer1.Size = new System.Drawing.Size(1040, 371);
            this.splitContainer1.SplitterDistance = 230;
            this.splitContainer1.TabIndex = 6;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel3.Controls.Add(this.DGRID_FILTER);
            this.panel3.Controls.Add(this.DGRID_LIST);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(5);
            this.panel3.Size = new System.Drawing.Size(230, 371);
            this.panel3.TabIndex = 0;
            // 
            // DGRID_FILTER
            // 
            this.DGRID_FILTER.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DGRID_FILTER.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DGRID_FILTER.Location = new System.Drawing.Point(5, 5);
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
            this.DGRID_LIST.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DGRID_LIST.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.DGRID_LIST.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DGRID_LIST.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.DGRID_LIST.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGRID_LIST.ColumnHeadersVisible = false;
            this.DGRID_LIST.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Keys});
            this.DGRID_LIST.Cursor = System.Windows.Forms.Cursors.Default;
            this.DGRID_LIST.EnableHeadersVisualStyles = false;
            this.DGRID_LIST.Location = new System.Drawing.Point(5, 35);
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
            this.DGRID_LIST.Size = new System.Drawing.Size(220, 329);
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
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.EDIT_BTN_SAV);
            this.panel2.Controls.Add(this.EDIT_BTN_DEL);
            this.panel2.Controls.Add(this.EDIT_BTN_NEW);
            this.panel2.Controls.Add(this.EDIT_ELAPSED);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.EDIT_KEY);
            this.panel2.Controls.Add(this.EDIT_VALUE);
            this.panel2.Cursor = System.Windows.Forms.Cursors.Default;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(806, 371);
            this.panel2.TabIndex = 0;
            // 
            // EDIT_BTN_SAV
            // 
            this.EDIT_BTN_SAV.BackColor = System.Drawing.Color.ForestGreen;
            this.EDIT_BTN_SAV.FlatAppearance.BorderColor = System.Drawing.Color.DarkGreen;
            this.EDIT_BTN_SAV.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.EDIT_BTN_SAV.ForeColor = System.Drawing.Color.White;
            this.EDIT_BTN_SAV.Location = new System.Drawing.Point(87, 17);
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
            this.EDIT_BTN_DEL.Location = new System.Drawing.Point(726, 79);
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
            this.EDIT_BTN_NEW.Location = new System.Drawing.Point(13, 17);
            this.EDIT_BTN_NEW.Name = "EDIT_BTN_NEW";
            this.EDIT_BTN_NEW.Size = new System.Drawing.Size(68, 29);
            this.EDIT_BTN_NEW.TabIndex = 18;
            this.EDIT_BTN_NEW.Text = "New";
            this.EDIT_BTN_NEW.UseVisualStyleBackColor = false;
            this.EDIT_BTN_NEW.Click += new System.EventHandler(this.EDIT_BTN_NEW_Click);
            // 
            // EDIT_ELAPSED
            // 
            this.EDIT_ELAPSED.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EDIT_ELAPSED.Location = new System.Drawing.Point(564, 121);
            this.EDIT_ELAPSED.Name = "EDIT_ELAPSED";
            this.EDIT_ELAPSED.Size = new System.Drawing.Size(230, 20);
            this.EDIT_ELAPSED.TabIndex = 17;
            this.EDIT_ELAPSED.Text = "Elapsed : 0,000000";
            this.EDIT_ELAPSED.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 121);
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
            this.label1.Size = new System.Drawing.Size(33, 20);
            this.label1.TabIndex = 14;
            this.label1.Text = "Key";
            // 
            // EDIT_KEY
            // 
            this.EDIT_KEY.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EDIT_KEY.Cursor = System.Windows.Forms.Cursors.Default;
            this.EDIT_KEY.Location = new System.Drawing.Point(13, 81);
            this.EDIT_KEY.Name = "EDIT_KEY";
            this.EDIT_KEY.ReadOnly = true;
            this.EDIT_KEY.Size = new System.Drawing.Size(704, 27);
            this.EDIT_KEY.TabIndex = 13;
            // 
            // EDIT_VALUE
            // 
            this.EDIT_VALUE.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EDIT_VALUE.Cursor = System.Windows.Forms.Cursors.Default;
            this.EDIT_VALUE.Location = new System.Drawing.Point(13, 144);
            this.EDIT_VALUE.MinimumSize = new System.Drawing.Size(4, 150);
            this.EDIT_VALUE.Multiline = true;
            this.EDIT_VALUE.Name = "EDIT_VALUE";
            this.EDIT_VALUE.ReadOnly = true;
            this.EDIT_VALUE.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.EDIT_VALUE.Size = new System.Drawing.Size(781, 217);
            this.EDIT_VALUE.TabIndex = 12;
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
            this.MESSAGE_LIST.Size = new System.Drawing.Size(1040, 110);
            this.MESSAGE_LIST.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 560);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KV Explorer";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox KV_FILENAME;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SaveFileDialog DIALOG_SAVE;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusSpacer;
        private System.Windows.Forms.ToolStripStatusLabel statusTitle;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox MESSAGE_LIST;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label EDIT_ELAPSED;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox EDIT_KEY;
        private System.Windows.Forms.TextBox EDIT_VALUE;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox DGRID_FILTER;
        private System.Windows.Forms.DataGridView DGRID_LIST;
        private System.Windows.Forms.DataGridViewTextBoxColumn Keys;
        private System.Windows.Forms.Button EDIT_BTN_NEW;
        private System.Windows.Forms.Button EDIT_BTN_SAV;
        private System.Windows.Forms.Button EDIT_BTN_DEL;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

