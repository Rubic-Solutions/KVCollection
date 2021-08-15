using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KVExplorer
{

    public partial class Form1 : Form
    {
        private static string exe_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private KeyValue.CollectionBase kvFile = new KeyValue.CollectionBase();


        private readonly DataTable DGRID_LIST_SOURCE = new DataTable();

        public Form1()
        {
            InitializeComponent();

            DGRID_LIST.Columns.Clear();
            DGRID_LIST_SOURCE.Columns.Add("POS", typeof(long));
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
            DGRID_LIST.Columns["POS"].Visible = false;
            SetItem(-1);
        }

        private async Task<bool> file_set(string fullFileName)
        {
            SetItem(-1);
            this.Text = "KV Explorer";
            MESSAGE_LIST.Items.Add(string.Empty);
            if (string.IsNullOrEmpty(fullFileName)) return false;
            //----------------------------------------------------
            this.Text = "KV Explorer - " + fullFileName;
            var result = await DoWork(() =>
            {
                kvFile.Open(fullFileName);  // => bu method OPEN ve COUNT vs interface de olmalı. 
                SetItem(-1);
                BTN_CLOSE.Visible = true;
                BTN_OPEN.Visible = false;
                BTN_TEST.Visible = false;
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
                foreach (var item in kvFile.GetHeaders())
                {
                    //var pkey = item.GetPrimaryKey;
                    DGRID_LIST_SOURCE.Rows.Add(item.Pos, item.PrimaryKey);
                }
                DGRID_LIST_SOURCE.EndLoadData();
            }, "Keys are being listed.");
            MessageAdd("\tCount = " + DGRID_LIST_SOURCE.Rows.Count);
            if (result.Success == false) return false;

            var adult_count = 0;
            var dt = DateTime.Now.AddYears(-30);
            result = await DoWork(() =>
                {
                    adult_count = (from x
                                  in kvFile.All<testModel>()
                                   where x.Value.IsAdult && x.Value.BirtDate > dt
                                   select x).Count();
                }, "LINQ search.");
            MessageAdd("\tTotal " + adult_count + " adult(s) found.");

            result = await DoWork(() =>
                {
                    KeyValuePair<KeyValue.RowHeader, byte[]> item = default;
                    for (int i = 0; i < 100; i++)
                        item = kvFile.GetFirst();

                    MessageAdd("\tKey = " + item.Key.PrimaryKey);
                }, "Get First Record 100 times.");

            result = await DoWork(() =>
                {
                    KeyValuePair<KeyValue.RowHeader, byte[]> item = default;
                    for (int i = 0; i < 100; i++)
                        item = kvFile.GetLast();

                    MessageAdd("\tKey = " + item.Key.PrimaryKey);
                }, "Get Last Record 100 times.");

            return result.Success;
        }

        private async void BTN_OPEN_Click(object sender, EventArgs e)
        {
            if (DIALOG_SAVE.ShowDialog() == DialogResult.OK)
                if (await file_set(DIALOG_SAVE.FileName) == false)
                    return;
        }

        private void BTN_CLOSE_Click(object sender, EventArgs e)
        {
            this.Text = "KV Explorer";
            kvFile.Close();
            DGRID_LIST_SOURCE.Clear();
            DGRID_SEL_INX = -1;
            SetItem(-1);
            BTN_CLOSE.Visible = false;
            BTN_OPEN.Visible = true;
            BTN_TEST.Visible = true;
        }
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
            EDIT_HDR.Text = string.Empty;
            EDIT_VALUE.Text = string.Empty;

            DGRID_SEL_INX = SelIndex;
            EDIT_VALUE.ReadOnly = false;

            if (this.EDIT_IS_EDIT)
            {
                var pos = (long)DGRID_LIST_SOURCE.Rows[SelIndex][0];
                //var pkey = (string)DGRID_LIST.Rows[SelIndex].Cells[0].Value;

                KeyValue.RowHeader head = null;
                string data = null;
                var result = await DoWork(() =>
                {
                    var row = kvFile.GetValue(pos);
                    head = row.Key;
                    data = System.Text.Encoding.UTF8.GetString(row.Value);
                }, null);
                EDIT_VALUE.Text = data;

                var txt = new List<string>();
                txt.Add("Primary Key");
                txt.Add("    " + head.PrimaryKey);

                txt.Add("");
                txt.Add("Elapsed");
                txt.Add("    " + result.Duration.ToString(@"ss\.fffffff"));

                EDIT_HDR.Text = string.Join(Environment.NewLine, txt);
            }
        }
        private void EDIT_BTN_SetColor(Button btn, System.Drawing.Color bgColor, System.Drawing.Color color)
        {
            btn.BackColor = btn.Enabled ? bgColor : System.Drawing.Color.WhiteSmoke;
            btn.ForeColor = btn.Enabled ? color : System.Drawing.Color.Silver;
            btn.FlatAppearance.BorderColor = btn.Enabled ? bgColor : System.Drawing.Color.WhiteSmoke;
        }
        #endregion

        #region "TEST"
        private async void BTN_TEST_Click(object sender, EventArgs e)
        {
            BTN_CLOSE_Click(sender, e);

            if (DIALOG_SAVE.ShowDialog() != DialogResult.OK ||
                string.IsNullOrEmpty(DIALOG_SAVE.FileName)) return;


            //var kvFile2 = new KeyValue.CollectionBase();
            //kvFile2.Open(DIALOG_SAVE.FileName, dontLoad:true);

            //var kvFile3 = new KeyValue.CollectionBase();
            //kvFile3.Open(DIALOG_SAVE.FileName, dontLoad:true);

            kvFile = new KeyValue.CollectionBase();
            kvFile.Open(DIALOG_SAVE.FileName);

            DoWorkResult result = null;

            result = await DoWork(() =>
             {
                 kvFile.Truncate();
             }, "File is being truncated.");
            if (result.Success == false) return;


            result = await DoWork(() =>
            {
                for (int i = 1; i < 500000; i++)
                {
                    var item = new testModel();
                    item.Name = "Person " + i;
                    item.Age = ((i % 90) + 1) + 10;
                    item.BirtDate = new DateTime(DateTime.Now.Year - item.Age, (i % 12) + 1, 1);
                    item.IsAdult = item.Age > 18;

                    kvFile.Add("Key-" + i, KeyValue.Serializer.ToBytes(item));
                }
            }, "Sample records are being generated.");

            if (result.Success == false) return;
            await loadKeys();

        }

        private class testModel
        {
            public string Name;
            public int Age;
            public DateTime BirtDate;
            public bool IsAdult;

            [JsonIgnore()]
            public bool IsAdult2;
        }
        //sw.Restart();
        //System.Diagnostics.Debug.Print("Count = " + KeyValueFile.Count() + " (" + sw.Elapsed.ToString() + ")");

        //sw.Restart();
        //System.Diagnostics.Debug.Print("Count = " + KeyValueFile.Count() + " (" + sw.Elapsed.ToString() + ")");
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
                    sw.Stop();
                    retval.Duration = sw.Elapsed;
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
