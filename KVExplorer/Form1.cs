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

            KeyValue.CollectionIndexer.Define<testModel>()
                .EnsureIndex(x => x.Key)
                .EnsureIndex(x => x.IsAdult)
                .EnsureIndex(x => x.BirtDate);

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
                    dataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
                var fi = new System.IO.FileInfo(fullFileName);
                var dir = fi.DirectoryName;
                var nam = fi.Extension.Length > 0 ? fi.Name.Replace(fi.Extension, "") : fi.Name;

                kvFile.Open(dir, nam);  // => bu method OPEN ve COUNT vs interface de olmalı. 
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
            SetItem(-1);

            var items = new List<KeyValue.RowHeader>();
            var result = await DoWork(() =>
            {
                if (kvFile.Count == 0) return;
                foreach (var item in kvFile.GetHeaders())
                    items.Add(item);

            }, "Keys are being loaded.");
            MessageAdd("\tCount = " + items.Count);
            if (result.Success == false) return false;

            result = await DoWork(() =>
            {
                if (items.Count == 0) return;
                DGRID_LIST_SOURCE.BeginLoadData();
                foreach (var item in items)
                    DGRID_LIST_SOURCE.Rows.Add(item.Pos, item.Id);

                DGRID_LIST_SOURCE.EndLoadData();
            }, "DataView is being filled-up.");
            MessageAdd("\tCount = " + DGRID_LIST_SOURCE.Rows.Count);
            if (result.Success == false) return false;

            var adult_count = 0;
            var dt = DateTime.Now.AddYears(-30);
            result = await DoWork(() =>
                {
                    foreach (var item in kvFile.FindAll(x => (bool)x[1] && (DateTime)x[2] > dt))
                    {
                        var i = item;
                        adult_count++;
                    }
                }, "FindAll predicate search.");
            MessageAdd("\tTotal " + adult_count + " adult(s) found.");

            var lastItem = kvFile.GetLast();
            result = await DoWork(() =>
                {
                    var item = kvFile.Get(lastItem.Key.Id);
                    if (item.Key is object)
                        MessageAdd("\tKey = " + item.Key.Id);
                }, "ID FIND search.");

            result = await DoWork(() =>
                {
                    KeyValuePair<KeyValue.RowHeader, byte[]> item = default;
                    for (int i = 0; i < 100; i++)
                        item = kvFile.GetFirst();

                    if (item.Key is object)
                        MessageAdd("\tKey = " + item.Key.Id);
                }, "Get First Record 100 times.");

            result = await DoWork(() =>
                {
                    KeyValuePair<KeyValue.RowHeader, byte[]> item = default;
                    for (int i = 0; i < 100; i++)
                        item = kvFile.GetLast();

                    if (item.Key is object)
                        MessageAdd("\tKey = " + item.Key.Id);
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
            long selPos = 0;
            if (DGRID_LIST.SelectedRows.Count > 0)
                if (DGRID_LIST.SelectedCells.Count > 0)
                {
                    selPos = (long)DGRID_LIST_SOURCE.Rows[DGRID_LIST.SelectedRows[0].Index][0];
                }

            SetItem(selPos);
        }
        #endregion

        #region "EDIT"
        private bool EDIT_IS_NEW => (kvFile.IsOpen && EDIT_POS == 0);
        private bool EDIT_IS_EDIT => (kvFile.IsOpen && EDIT_POS > 1);
        private async void SetItem(long pos)
        {
            EDIT_HDR.Text = string.Empty;
            EDIT_KEY.Text = string.Empty;
            EDIT_VALUE.Text = string.Empty;
            EDIT_POS = pos;
            EDIT_KEY.Enabled = this.EDIT_IS_NEW;
            EDIT_VALUE.Enabled = this.EDIT_IS_NEW || this.EDIT_IS_EDIT;
            BTN_NEW.Visible = kvFile.IsOpen;
            // BTN_SAV
            BTN_SAV.Enabled = (kvFile.IsOpen && EDIT_POS != -1);
            EDIT_BTN_SetColor(BTN_SAV, System.Drawing.Color.ForestGreen, System.Drawing.Color.White);
            // BTN_DEL
            BTN_DEL.Enabled = this.EDIT_IS_EDIT;
            EDIT_BTN_SetColor(BTN_DEL, System.Drawing.Color.OrangeRed, System.Drawing.Color.White);

            if (pos > 0)
            {
                KeyValue.RowHeader head = null;
                string data = null;
                var result = await DoWork(() =>
                {
                    var row = kvFile.GetByPos(EDIT_POS);
                    head = row.Key;
                    data = System.Text.Encoding.UTF8.GetString(row.Value);
                }, null);
                EDIT_KEY.Text = string.Join(", ",  head.IndexValues );
                EDIT_VALUE.Text = data;

                var txt = new List<string>();
                txt.Add("Id");
                txt.Add("    " + head.Id);
                txt.Add("");
                txt.Add("Pos (pointer)");
                txt.Add("    " + head.Pos);
                txt.Add("");
                txt.Add("Value Length (Size on disk)");
                txt.Add("    " + head.ValueActualSize + " (" + head.ValueSize + ") byte(s)");
                txt.Add("");
                txt.Add("Elapsed");
                txt.Add("    " + result.Duration.ToString(@"ss\.fffffff") + " sec.");

                EDIT_HDR.Text = string.Join(Environment.NewLine, txt);
            }
        }
        private void EDIT_BTN_SetColor(Button btn, System.Drawing.Color bgColor, System.Drawing.Color color)
        {
            btn.BackColor = btn.Enabled ? bgColor : System.Drawing.Color.WhiteSmoke;
            btn.ForeColor = btn.Enabled ? color : System.Drawing.Color.Silver;
            btn.FlatAppearance.BorderColor = btn.Enabled ? bgColor : System.Drawing.Color.WhiteSmoke;
        }
        private long EDIT_POS = 0;
        private async void BTN_SAV_Click(object sender, EventArgs e)
        {
            var result = await DoWork(() =>
            {
                //if (EDIT_IS_NEW)
                //    kvFile.Add(EDIT_KEY.Text, KeyValue.Serializer.GetBytes(EDIT_VALUE.Text));
                //else
                //    kvFile.Update(EDIT_KEY.Text, KeyValue.Serializer.GetBytes(EDIT_VALUE.Text));
            }, "Item has been saved.");
        }
        private async void BTN_DEL_Click(object sender, EventArgs e)
        {
            var result = await DoWork(() =>
            {
                //kvFile.Delete(EDIT_KEY.Text);
            }, "Item has been deleted.");
        }
        #endregion

        #region "TEST"
        private async void BTN_TEST_Click(object sender, EventArgs e)
        {
            BTN_CLOSE_Click(sender, e);

            if (DIALOG_SAVE.ShowDialog() != DialogResult.OK ||
                string.IsNullOrEmpty(DIALOG_SAVE.FileName)) return;

            var fi = new System.IO.FileInfo(DIALOG_SAVE.FileName);
            var dir = fi.DirectoryName;
            var nam = fi.Extension.Length > 0 ? fi.Name.Replace(fi.Extension, "") : fi.Name;

            var kvFileTyped = new KeyValue.Collection<testModel>();
            kvFileTyped.Open(dir, nam);
            DoWorkResult result = null;

            result = await DoWork(() =>
             {
                 kvFileTyped.Truncate();
             }, "File is being truncated.");
            if (result.Success == false) return;


            result = await DoWork(() =>
            {
                for (int i = 0; i < 50000; i++)
                {
                    var item = new testModel();
                    item.Key = "Key-" + i;
                    item.Age = ((i % 90) + 1) + 10;
                    item.BirtDate = new DateTime(DateTime.Now.Year - item.Age, (i % 12) + 1, 1);
                    item.IsAdult = item.Age > 18;
                    item.MailBody = "".PadRight(5000, 'X');

                    //kvFileTyped.Add(KeyValue.Serializer.GetBytes(item));
                    kvFileTyped.Add(item);
                }
            }, "Sample records are being generated.");

            if (result.Success == false) return;

            kvFileTyped.Close();

            await file_set(DIALOG_SAVE.FileName);
        }

        private class testModel
        {
            public string Key;
            public int Age;
            public DateTime BirtDate;
            public bool IsAdult;
            public string MailBody;

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
                            MESSAGE_LIST.Items[msgIndex] = StatusText + " (" + sw.Elapsed.ToString(@"ss\.fffffff") + " sec.)";
                        retval.Success = true;
                    }
                    catch (Exception ex)
                    {
                        MessageAdd("ERROR:" + ex.Message + " (" + sw.Elapsed.ToString(@"ss\.fffffff") + " sec.)");
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
