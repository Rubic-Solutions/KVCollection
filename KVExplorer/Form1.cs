using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KVExplorer
{

    public partial class Form1 : Form
    {
        private static string exe_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private KeyValueFile.Collection kvFile =  new KeyValueFile.Collection();


        private readonly DataTable DGRID_LIST_SOURCE = new DataTable();

        public Form1()
        {
            InitializeComponent();

            DGRID_LIST.Columns.Clear();
            DGRID_LIST_SOURCE.Columns.Add("KEYS", typeof(string));
            DGRID_LIST.DataBindingComplete += (o, _) =>
            {
                var dataGridView = o as DataGridView;
                if (dataGridView != null)
                {
                    //dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            };
            DGRID_LIST.DataSource = DGRID_LIST_SOURCE;
            SetItem(-1);
        }

        private async Task<bool> file_set(string fullFileName)
        {
            SetItem(-1);
            if (string.IsNullOrEmpty(fullFileName)) return false;

            MESSAGE_LIST.Items.Add(string.Empty);
            EDIT_KEY.Text = string.Empty;
            EDIT_VALUE.Text = string.Empty;

            KV_FILENAME.Text = fullFileName;
            var result = await DoWork(() =>
            {
                kvFile.Open(KV_FILENAME.Text);
                SetItem(-1);
            }, "File is being loaded.");

            return result.Success && await loadKeys();
        }

        private async Task<bool> loadKeys()
        {
            DGRID_LIST_SOURCE.Clear();
            DGRID_SEL_INX = -1;

            var result = await DoWork(() =>
            {
                if (kvFile.Count == 0) return;

                DGRID_LIST_SOURCE.BeginLoadData();
                foreach (var item in kvFile.GetKeys())
                {
                    DGRID_LIST_SOURCE.Rows.Add(item);
                }
                DGRID_LIST_SOURCE.EndLoadData();
            }, "Keys are being loaded.");
            MessageAdd("\tCount = " + DGRID_LIST_SOURCE.Rows.Count);
            return result.Success;
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            if (DIALOG_SAVE.ShowDialog() == DialogResult.OK)
                if (await file_set(DIALOG_SAVE.FileName) == false)
                    return;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            kvFile.Close();
            DGRID_LIST_SOURCE.Clear();
            DGRID_SEL_INX = -1;
            SetItem(-1);
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            if (DIALOG_SAVE.ShowDialog() == DialogResult.OK)
                if (await file_set(DIALOG_SAVE.FileName) == false)
                    return;

            var result = await DoWork(() =>
             {
                 kvFile.Truncate();
             }, "File is being truncated.");
            if (result.Success == false) return;

            result = await DoWork(() =>
            {
                for (int i = 1; i < 100000; i++)
                {
                    kvFile.Add("Key-" + i, ".üğişçöÜĞİŞÇÖıI");
                }
            }, "Sample records are being generated.");
            if (result.Success == false) return;

            await loadKeys();
        }


        //sw.Restart();
        //System.Diagnostics.Debug.Print("Count = " + KeyValueFile.Count() + " (" + sw.Elapsed.ToString() + ")");

        //sw.Restart();
        //System.Diagnostics.Debug.Print("Count = " + KeyValueFile.Count() + " (" + sw.Elapsed.ToString() + ")");

        #region "DGRID"
        private void DGRID_FILTER_TextChanged(object sender, EventArgs e)
        {
            var filterKey = DGRID_FILTER.Text.Trim().Replace("'", "''");
            DGRID_LIST_SOURCE.DefaultView.RowFilter = DGRID_LIST_SOURCE.Columns[0].ColumnName + " LIKE '%" + filterKey + "%'";
            DGRID_LIST.Refresh();
        }
        private void DGRID_SelChanged(object sender, EventArgs e)
        {
            DGRID_SEL_INX = -1;
            if (DGRID_LIST.SelectedRows.Count > 0)
                if (DGRID_LIST.SelectedCells.Count > 0)
                    SetItem(DGRID_LIST.SelectedRows[0].Index);
        }
        #endregion

        #region "EDIT"
        private int DGRID_SEL_INX = -1;
        private bool EDIT_IS_NEW => (DGRID_SEL_INX == -1);
        private bool EDIT_IS_EDIT => (DGRID_SEL_INX != -1);
        private async void SetItem(int SelIndex)
        {
            EDIT_KEY.Text = string.Empty;
            EDIT_VALUE.Text = string.Empty;
            EDIT_ELAPSED.Text = "Elapsed : 0";

            DGRID_SEL_INX = SelIndex;
            EDIT_KEY.ReadOnly = this.EDIT_IS_EDIT;
            EDIT_VALUE.ReadOnly = false;

            if (this.EDIT_IS_EDIT)
            {
                EDIT_KEY.Text = (string)DGRID_LIST.Rows[SelIndex].Cells[0].Value;

                var result = await DoWork(() =>
                {
                    EDIT_VALUE.Text = kvFile.Get(EDIT_KEY.Text);
                }, null);
                EDIT_ELAPSED.Text = "Elapsed : " + result.Duration.TotalSeconds.ToString();
            }

            EDIT_BTN_NEW.Enabled = (kvFile.FileInfo is object);
            EDIT_BTN_SetColor(EDIT_BTN_NEW, System.Drawing.Color.RoyalBlue, System.Drawing.Color.White);

            EDIT_BTN_SAV.Enabled = (kvFile.FileInfo is object);
            EDIT_BTN_SetColor(EDIT_BTN_SAV, System.Drawing.Color.ForestGreen, System.Drawing.Color.White);

            EDIT_BTN_DEL.Enabled = (SelIndex != -1);
            EDIT_BTN_SetColor(EDIT_BTN_DEL, System.Drawing.Color.OrangeRed, System.Drawing.Color.White);



        }
        private void EDIT_BTN_SetColor(Button btn, System.Drawing.Color bgColor, System.Drawing.Color color)
        {
            btn.BackColor = btn.Enabled ? bgColor : System.Drawing.Color.WhiteSmoke;
            btn.ForeColor = btn.Enabled ? color : System.Drawing.Color.Silver;
            btn.FlatAppearance.BorderColor = btn.Enabled ? bgColor : System.Drawing.Color.WhiteSmoke;
        }

        private void EDIT_BTN_NEW_Click(object sender, EventArgs e) => SetItem(-1);
        private async void EDIT_BTN_SAV_Click(object sender, EventArgs e)
        {
            if (this.EDIT_IS_NEW)
                await DoWork(() =>
                {
                    kvFile.Add(EDIT_KEY.Text, EDIT_VALUE.Text);
                    DGRID_LIST_SOURCE.Rows.Add(EDIT_KEY.Text);
                    Application.DoEvents();

                    DGRID_LIST.Rows[DGRID_LIST.Rows.GetLastRow(DataGridViewElementStates.Displayed)].Selected = true;
                }, "Record has been added. (KEY = " + EDIT_KEY.Text + ")");
            else
                await DoWork(() =>
                {
                    kvFile.Update(EDIT_KEY.Text, EDIT_VALUE.Text);
                }, "Record has been updated. (KEY = " + EDIT_KEY.Text + ")");
        }
        private async void EDIT_BTN_CNL_Click(object sender, EventArgs e)
        {
            if (DGRID_SEL_INX == -1) return;
            await DoWork(() =>
            {
                kvFile.Delete(EDIT_KEY.Text);
                DGRID_LIST_SOURCE.Rows.RemoveAt(DGRID_SEL_INX);
            }, "Record has been deleted. (KEY = " + EDIT_KEY.Text + ")");
        }
        #endregion


        #region "Messages"
        private int MessageAdd(string message)
        {
            if (message == null) return -1;
            var retval = MESSAGE_LIST.Items.Add(message);
            MESSAGE_LIST.SelectedIndex = MESSAGE_LIST.Items.Count - 1;
            return retval;
        }
        #endregion
        #region "DoWork"
        private class DoWorkResult
        {
            public bool Success;
            public TimeSpan Duration;
        }
        private Stopwatch sw = new Stopwatch();
        private async Task<DoWorkResult> DoWork(Action fn, string StatusText)
        {
            var retval = new DoWorkResult();
            await Task.Run(() =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    Cursor.Current = Cursors.WaitCursor;
                    var msgIndex = MessageAdd(StatusText);

                    Application.DoEvents();
                    //---------------------------------------
                    sw.Restart();
                    try
                    {
                        fn();
                        if (msgIndex != -1)
                            MESSAGE_LIST.Items[msgIndex] = StatusText + " (" + sw.Elapsed.ToString() + ")";
                        retval.Success = true;
                    }
                    catch (Exception ex)
                    {
                        MessageAdd("ERROR:" + ex.Message + " (" + sw.Elapsed.ToString() + ")");
                    }
                    MESSAGE_LIST.SelectedIndex = MESSAGE_LIST.Items.Count - 1;
                    retval.Duration = sw.Elapsed;
                    sw.Stop();
                    //---------------------------------------
                    Cursor.Current = Cursors.Default;
                    Application.DoEvents();
                }));
            });
            return retval;
        }
        #endregion

    }
}
