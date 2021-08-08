using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace KeyValue
{
    public class Collection : IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static object lock_ctor = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static Dictionary<string, CollectionWriter> cws = null;   // CollectionWriters
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private CollectionWriter cw = null;                               // CollectionWriter

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static Dictionary<string, CacheOfHeaders> caches = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private CacheOfHeaders cache = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private System.Text.Encoding enc = System.Text.Encoding.UTF8;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private System.IO.FileInfo fs_info = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private FileStream fs_read = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private object fs_lock_read = new object();

        static Collection()
        {
            cws = new Dictionary<string, CollectionWriter>();
            caches = new Dictionary<string, CacheOfHeaders>();
        }

        public void Dispose() => Close();

        #region "Open/Close"
        public System.IO.FileInfo FileInfo => this.fs_info;

        public void Open(string CollectionName)
        {
            if (this.fs_info?.FullName == CollectionName) return;
            if (this.fs_info is object) this.Close();

            this.fs_info = new System.IO.FileInfo(CollectionName);
            var name = this.fs_info.FullName;
            var fi_exists = fs_info.Exists;

            this.fs_read = new FileStream(name, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

            lock (lock_ctor)
            {
                bool is_first = false;
                if (cws.ContainsKey(name) == false)
                {
                    cws.Add(name, new CollectionWriter(this.fs_info));
                    caches.Add(name, new CacheOfHeaders());
                    is_first = true;
                }

                this.cw = cws[name];
                this.cache = caches[name];

                if (is_first)
                {
                    HeadOfFile fileHeader;
                    if (fi_exists == false)
                    {
                        if (this.cw.IsInitial == false)
                            throw new Exception("Invalid collection file");

                        // file is being created newly (file-header is being initialized)
                        WriteBegin(null);
                        fileHeader = this.cache.TryGet();
                    }
                    else
                    {
                        fileHeader = this.cache.AddOrUpdate(readFileHeader());
                    }

                    var sw = new Stopwatch();
                    sw.Restart();
                    // all row-headers will be cached
                    int hsiz = (new HeadOfRow()).Size;
                    var posx = fileHeader.FirstRecStartPos;
                    fs_read.Seek(posx, SeekOrigin.Begin);
                    while (true)
                    {
                        HeadOfRow row_header = null;
                        {
                            var bytes = new byte[hsiz];
                            if (fs_read.Read(bytes, 0, hsiz) > 0)
                            {
                                row_header = new HeadOfRow();
                                row_header.FromArray(bytes);
                                row_header.Pos = posx;
                            }
                        }

                        if (row_header == null) break;
                        this.cache.Add(row_header);

                        if (row_header.NextPos == 0) break;

                        // code below takes 04.6148817 sec for 999.999 record.
                        //  fs_read.Position = row_header.NextPos;

                        // code below takes 01.8439142 sec for 999.999 record.
                        for (int i = 0; i < row_header.RowLength; i++) // row_header.ValueLength = (row_header.NextPos - (row_header.Pos + size));
                            fs_read.ReadByte();

                        posx = row_header.NextPos;
                    }
                    System.Diagnostics.Debug.WriteLine(sw.Elapsed.ToString());
                }
            }

            this.cache.InstanceCount++;
        }
        public void Close()
        {
            lock (lock_ctor)
            {
                if (this.fs_info == null) return;

                this.cache.InstanceCount--;
                this.fs_read.Close();

                if (this.cache.InstanceCount == 0)
                {
                    var name = this.fs_info.FullName;

                    this.cache.Clear();
                    caches.Remove(name);

                    this.cw.Close();
                    cws.Remove(name);
                }

                this.fs_info = null;
            }
        }
        #endregion

        #region "Public Operation Methods"
        public long Count => this.cache?.TryGet()?.Count ?? 0;
        public bool IsOpen => this.fs_info != null;

        /// <summary>Add an item into the collection.</summary>
        public void Add(string PrimaryKey, string Value,
                        string Key2 = null, string Key3 = null, string Key4 = null, string Key5 = null) =>
            WriteBegin((fileHeader) =>
            {
                var key_bytes = System.Text.Encoding.UTF8.GetBytes(PrimaryKey);
                if (key_bytes.Length == 0) throw new Exception("Key length must be at least 1 character.");
                if (this.cache.Exists(PrimaryKey)) throw new Exception("Key already exists.");

                add(fileHeader, PrimaryKey, enc.GetBytes(Value), Key2, Key3, Key4, Key5);
            });

        /// <summary>Deletes the value of the [Key]. If [Key] does not exist, nothing deleted.</summary>
        public void Delete(string PrimaryKey) =>
            WriteBegin((fileHeader) => delete(fileHeader, this.cache.TryGet(PrimaryKey)));

        /// <summary>All records is removed from collection, and file is shrinked.</summary>
        public void Truncate() =>
            WriteBegin((fileHeader) =>
            {
                this.cw.Truncate();
                this.cache.Clear();
                fileHeader = new HeadOfFile();
            });

        /// <summary>Updates the value of the [Key]. If [Key] does not exist, nothing updated.</summary>
        public void Update(string PrimaryKey, string Value,
                           string Key2 = null, string Key3 = null, string Key4 = null, string Key5 = null) =>
            WriteBegin((fileHeader) => update(fileHeader, this.cache.TryGet(PrimaryKey), enc.GetBytes(Value)));

        /// <summary>
        /// Sets the value of the [Key]. If [Key] does not exist, a new [Key] is created. If [Key] already exists in the collection, it is overwritten.
        /// <para>
        /// PrimaryKey value cannot be changed. Only can be changed other Key-Values . To clear Key-Value then set EMPTY value. To keep as is value of KEY, then leave NULL.
        /// </para>
        /// </summary>
        public void Upsert(string PrimaryKey, string Value,
                           string Key2 = null, string Key3 = null, string Key4 = null, string Key5 = null) =>
            WriteBegin((fileHeader) =>
            {
                var rowHeader = this.cache.TryGet(PrimaryKey);
                if (rowHeader == null)
                    add(fileHeader, PrimaryKey, enc.GetBytes(Value), Key2, Key3, Key4, Key5);
                else
                    update(fileHeader, rowHeader, enc.GetBytes(Value), Key2, Key3, Key4, Key5);
            });

        public bool Exists(string PrimaryKey) => this.cache?.Exists(PrimaryKey) ?? false;

        public string Get(string PrimaryKey) => Get(PrimaryKey, out _);
        public string Get(string PrimaryKey, out string[] Keys)
        {
            Keys = default;
            if (IsOpen == false) return null;

            var h = this.cache.TryGet(PrimaryKey);
            Keys = h.Keys;
            var bytes = readRowValue(h);
            if (bytes == null || bytes.Length == 0) return default;
            return enc.GetString(bytes);
        }

        public IEnumerable<string> GetKeys()
        {
            if (IsOpen == false) return default;

            return this.cache.Keys();
        }

        public IEnumerable<KeyValuePair<string, string>> All()
        {
            foreach (var header in this.cache.Items())
            {
                var bytes = readRowValue(header.Value);
                string val = (bytes == null || bytes.Length == 0) ? default : enc.GetString(bytes);

                yield return new KeyValuePair<string, string>(header.Key, val);
            }
        }
        public IEnumerable<KeyValuePair<string, string>> FindAll(Func<HeadOfRow, bool> match)
        {
            foreach (var header in this.cache.Items())
            {
                if (match(header.Value) == false) continue;
                var bytes = readRowValue(header.Value);
                string val = (bytes == null || bytes.Length == 0) ? default : enc.GetString(bytes);

                yield return new KeyValuePair<string, string>(header.Key, val);
            }
        }
        #endregion

        #region "private methods"
        private void WriteBegin(Action<HeadOfFile> fn)
        {
            if (IsOpen == false) return;

            this.cw.WriteBegin((Action)(() =>
            {
                var fileHeader = this.cache.TryGet() ?? new HeadOfFile();
                if (fn is object) fn(fileHeader);
                writeFileHeader(fileHeader);
            }));
        }

        private void add(HeadOfFile fileHeader, string PrimaryKey, byte[] value,
                        string Key2 = null, string Key3 = null, string Key4 = null, string Key5 = null)
        {
            var new_row = new HeadOfRow();
            { // header of new record
                new_row.Pos = this.cw.Length;
                new_row.ValueLength = value.Length;
                new_row.RowLength = value.Length;
                new_row.PrevPos = fileHeader.Count > 0 ? fileHeader.LastRecStartPos : 0;
                new_row.Keys[0] = PrimaryKey;
                new_row.Keys[1] = Key2;
                new_row.Keys[2] = Key3;
                new_row.Keys[3] = Key4;
                new_row.Keys[4] = Key5;
            }

            // [NextPos] value of the [LastRecord] will be update; If any record has exists
            if (fileHeader.Count > 0)
            {
                var lastRecHeader = this.cache.TryGet(fileHeader.LastRecStartPos);
                if (lastRecHeader is object)
                {
                    lastRecHeader.NextPos = new_row.Pos;
                    writeRowHeader(lastRecHeader);
                }
            }

            { // header of the file is being updated
                fileHeader.LastRecStartPos = new_row.Pos;   // last position is new record-position
                fileHeader.Count++;
            }

            { // new record is being inserted
                writeRowHeader(new_row);
                writeRowValue(new_row, value);
            }
        }
        private void delete(HeadOfFile fileHeader, HeadOfRow rowHeader)
        {
            if (rowHeader == null) return;

            // is there previous record ?
            if (rowHeader.PrevPos != 0)
            {
                var prev_row_header = this.cache.TryGet(rowHeader.PrevPos);
                prev_row_header.NextPos = rowHeader.NextPos;
                writeRowHeader(prev_row_header);
            }
            else
            { // there is no previous record.
                fileHeader.FirstRecStartPos = rowHeader.NextPos;
            }

            // is there next record ?
            if (rowHeader.NextPos != 0)
            {
                var next_row_header = this.cache.TryGet(rowHeader.NextPos);
                next_row_header.PrevPos = rowHeader.PrevPos;
                writeRowHeader(next_row_header);
            }
            else
            { // there is no next record.
                fileHeader.LastRecStartPos = rowHeader.PrevPos;
            }

            fileHeader.Count--;

            this.cache.Remove(rowHeader);
        }
        private void update(HeadOfFile fileHeader, HeadOfRow rowHeader, byte[] value,
                            string Key2 = null, string Key3 = null, string Key4 = null, string Key5 = null)
        {
            if (rowHeader == null) return;

            // if there is a next record and also new value is longer than old value,
            // then delete old record and insert as new.
            if (value.Length > rowHeader.RowLength && rowHeader.NextPos != 0)
            {
                delete(fileHeader, rowHeader);
                add(fileHeader, rowHeader.PrimaryKey, value, Key2, Key3, Key4, Key5);
            }
            else
            {
                // has data been changed ?
                var has_changed = rowHeader.ValueLength != value.Length;
                if (!has_changed)
                {
                    var old_value = readRowValue(rowHeader);
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] != old_value[i])
                        {
                            has_changed = true;
                            break;
                        }
                    }
                }
                if (!has_changed) return;

                rowHeader.ValueLength = value.Length;
                if (Key2 != null) rowHeader.Keys[1] = Key2;
                if (Key3 != null) rowHeader.Keys[2] = Key3;
                if (Key4 != null) rowHeader.Keys[3] = Key4;
                if (Key5 != null) rowHeader.Keys[4] = Key5;
                writeRowHeader(rowHeader);
                writeRowValue(rowHeader, value);
            }
        }

        //------- File/Row HEADER ---------------------------------------------------------
        private byte[] Read(long pos, int length)
        {
            if (length < 1) return default;

            var retval = new byte[length];
            lock (fs_lock_read)
            {
                //fs_read.Position = pos;
                fs_read.Seek(pos, SeekOrigin.Begin);
                return fs_read.Read(retval, 0, length) == 0 ? default : retval;
            }
        }

        private HeadOfFile readFileHeader()
        {
            var retval = new HeadOfFile();
            if (!retval.FromArray(this.Read(0, retval.Size)))
                throw new Exception("Invalid file header.");

            return retval;

        }

        private void writeFileHeader(HeadOfFile header)
        {
            this.cache.AddOrUpdate(header);
            this.cw.Write(header.Pos, header?.ToArray());
        }
        private void writeRowHeader(HeadOfRow header)
        {
            this.cache.AddOrUpdate(header);
            this.cw.Write(header.Pos, header?.ToArray());
        }
        //------- Row VALUE ------------------------------------------------------
        private byte[] readRowValue(HeadOfRow header)
        {
            if (header == null) return default;
            return this.Read(header.Pos + header.Size, header.ValueLength);
        }
        private void writeRowValue(HeadOfRow header, byte[] data) => this.cw.Write(header.Pos + header.Size, data);
        #endregion

        #region "caching"
        private class CacheOfHeaders
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private HeadOfFile file_header = null;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Dictionary<long, HeadOfRow> row_headers_by_pos = null;
            //[DebuggerBrowsable(DebuggerBrowsableState.Never)] private Dictionary<string, long> row_headers_by_key = null;
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Dictionary<string, HeadOfRow> row_headers_by_key = null;
            public int InstanceCount = 0;

            public CacheOfHeaders()
            {
                row_headers_by_pos = new Dictionary<long, HeadOfRow>(10000);
                row_headers_by_key = new Dictionary<string, HeadOfRow>(10000);
            }

            //public void EnsureCapacity(int capacity)
            //{
            //    row_headers_by_pos.EnsureCapacity(capacity);
            //    row_headers_by_key.EnsureCapacity(capacity);
            //}

            public HeadOfFile AddOrUpdate(HeadOfFile value)
            {
                file_header = value;
                return value;
            }
            public HeadOfRow Add(HeadOfRow value)
            {
                this.row_headers_by_pos.Add(value.Pos, value);
                this.row_headers_by_key.Add(value.PrimaryKey, value);
                return value;
            }

            public HeadOfRow AddOrUpdate(HeadOfRow value)
            {
                if (this.row_headers_by_key.ContainsKey(value.PrimaryKey))
                {
                    this.row_headers_by_pos[value.Pos] = value;
                    this.row_headers_by_key[value.PrimaryKey] = value;
                }
                else
                {
                    this.row_headers_by_pos.Add(value.Pos, value);
                    this.row_headers_by_key.Add(value.PrimaryKey, value);
                }
                return value;
            }

            public void Remove(HeadOfRow rowHeader)
            {
                this.row_headers_by_pos.Remove(rowHeader.Pos);
                this.row_headers_by_key.Remove(rowHeader.PrimaryKey);
            }
            public void Clear()
            {
                file_header = null;
                row_headers_by_pos.Clear();
                row_headers_by_key.Clear();
            }

            public IEnumerable<string> Keys() =>
                this.row_headers_by_key.Keys;

            public Dictionary<string, HeadOfRow> Items() =>
                this.row_headers_by_key;

            public bool Exists(string key) =>
                this.row_headers_by_key.ContainsKey(key);

            public HeadOfRow TryGet(string key) =>
                this.row_headers_by_key.TryGetValue(key, out HeadOfRow retval) ? retval : default;

            public HeadOfRow TryGet(long position) =>
                this.row_headers_by_pos.TryGetValue(position, out HeadOfRow retval) ? retval : default;

            public HeadOfFile TryGet() => file_header;

        }
        #endregion
    }
}