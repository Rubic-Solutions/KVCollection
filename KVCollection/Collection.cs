using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace KeyValueFile
{
    public class Collection
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

                    // all row-headers will be cached
                    var pos = fileHeader.FirstRecStartPos;
                    while (pos > 0)
                    {
                        var row_header = readRowHeader(pos);
                        if (row_header?.Key == null) break;

                        this.cache.AddOrUpdate(row_header);
                        pos = row_header.NextPos;
                    }
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
        public void Add(string Key, string Value) =>
            WriteBegin((fileHeader) =>
            {
                var key_bytes = System.Text.Encoding.UTF8.GetBytes(Key);
                if (key_bytes.Length == 0) throw new Exception("Key length must be at least 1 character.");
                if (key_bytes.Length > HeadOfRow.KeyLength) throw new Exception("Key length can be 32 bytes.");
                if (this.cache.Exists(Key)) throw new Exception("Key already exists.");

                add(fileHeader, Key, enc.GetBytes(Value));
            });

        /// <summary>Deletes the value of the [Key]. If [Key] does not exist, nothing deleted.</summary>
        public void Delete(string Key) =>
            WriteBegin((fileHeader) => delete(fileHeader, this.cache.TryGet(Key)));

        /// <summary>All records is removed from collection, and file is shrinked.</summary>
        public void Truncate() =>
            WriteBegin((fileHeader) =>
            {
                this.cw.Truncate();
                this.cache.Clear();
                fileHeader = new HeadOfFile();
            });

        /// <summary>Updates the value of the [Key]. If [Key] does not exist, nothing updated.</summary>
        public void Update(string Key, string Value) =>
            WriteBegin((fileHeader) => update(fileHeader, this.cache.TryGet(Key), enc.GetBytes(Value)));

        /// <summary>
        /// Sets the value of the [Key]. If [Key] does not exist, a new [Key] is created. If [Key] already exists in the collection, it is overwritten.
        /// </summary>
        public void Upsert(string Key, string Value) =>
            WriteBegin((fileHeader) =>
            {
                var rowHeader = this.cache.TryGet(Key);
                if (rowHeader == null)
                    add(fileHeader, Key, enc.GetBytes(Value));
                else
                    update(fileHeader, rowHeader, enc.GetBytes(Value));
            });

        public string Get(string Key)
        {
            if (IsOpen == false) return null;

            var bytes = readRowValue(this.cache.TryGet(Key));
            if (bytes == null || bytes.Length == 0) return default;
            return enc.GetString(bytes);
        }
        public IEnumerable<string> GetKeys()
        {
            if (IsOpen == false) yield return default;

            foreach (var header in this.cache.Items())
                yield return header.Key;
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

        private void add(HeadOfFile fileHeader, string key, byte[] value)
        {
            var new_row = new HeadOfRow();
            { // header of new record
                new_row.Pos = this.cw.Length;
                new_row.Key = key;
                new_row.ValueLength = value.Length;
                new_row.RowLength = value.Length;
                new_row.PrevPos = fileHeader.Count > 0 ? fileHeader.LastRecStartPos : 0;
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
        private void update(HeadOfFile fileHeader, HeadOfRow rowHeader, byte[] value)
        {
            if (rowHeader == null) return;

            // if there is a next record and also new value is longer than old value, then delete old record and insert as new.
            if (value.Length > rowHeader.RowLength && rowHeader.NextPos != 0)
            {
                delete(fileHeader, rowHeader);
                add(fileHeader, rowHeader.Key, value);
            }
            else
            {
                rowHeader.ValueLength = value.Length;
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
                fs_read.Position = pos;
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
        private HeadOfRow readRowHeader(long position)
        {
            var retval = new HeadOfRow();
            if (!retval.FromArray(this.Read(position, retval.Size)))
                throw new Exception("Invalid row header.");

            retval.Pos = position;
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
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Dictionary<string, HeadOfRow> row_headers_by_key = null;
            public int InstanceCount = 0;

            public CacheOfHeaders()
            {
                row_headers_by_pos = new Dictionary<long, HeadOfRow>();
                row_headers_by_key = new Dictionary<string, HeadOfRow>();
            }


            public HeadOfFile AddOrUpdate(HeadOfFile value)
            {
                file_header = value;
                return value;
            }
            public HeadOfRow AddOrUpdate(HeadOfRow value)
            {
                if (this.row_headers_by_key.ContainsKey(value.Key))
                {
                    this.row_headers_by_pos[value.Pos] = value;
                    this.row_headers_by_key[value.Key] = value;
                }
                else
                {
                    this.row_headers_by_pos.Add(value.Pos, value);
                    this.row_headers_by_key.Add(value.Key, value);
                }
                return value;
            }

            public void Remove(HeadOfRow rowHeader)
            {
                this.row_headers_by_pos.Remove(rowHeader.Pos);
                this.row_headers_by_key.Remove(rowHeader.Key);
            }
            public void Clear()
            {
                file_header = null;
                row_headers_by_pos.Clear();
                row_headers_by_key.Clear();
            }

            public IEnumerable<HeadOfRow> Items() =>
                this.row_headers_by_key.Values;

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