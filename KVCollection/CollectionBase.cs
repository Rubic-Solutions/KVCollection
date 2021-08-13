using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace KeyValue
{
    public class CollectionBase : IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static object lock_ctor = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static Dictionary<string, CollectionWriter> cws = null;   // CollectionWriters
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal CollectionWriter cw = null;                               // CollectionWriter

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static Dictionary<string, FileHeader> fhs = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal FileHeader fh = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal System.IO.FileInfo fs_info = null;


        static CollectionBase()
        {
            cws = new Dictionary<string, CollectionWriter>();
            fhs = new Dictionary<string, FileHeader>();
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

            lock (lock_ctor)
            {
                bool is_first = false;
                if (cws.ContainsKey(name) == false)
                {
                    cws.Add(name, new CollectionWriter(this.fs_info));
                    fhs.Add(name, new FileHeader());
                    is_first = true;
                }

                this.cw = cws[name];
                this.fh = fhs[name];

                if (is_first)
                {
                    var fs = this.cw.fs;

                    // file is being created newly (file-header is being initialized)
                    if (fi_exists)
                    {
                        var bytes = new byte[this.fh.Size];
                        if (fs.Read(bytes) != bytes.Length)
                            throw new Exception("Invalid file header.");

                        this.fh.FromArray(bytes);
                    }
                    else
                    {
                        writeBegin(null);
                    }

                    var hs = new HashSet<string>();
                    foreach (var rh in GetHeaders())
                    {
                        hs.Add(rh.GetPrimaryKey);
                    }
                }
            }

            this.cw.InstanceCount++;
        }
        public void Close()
        {
            lock (lock_ctor)
            {
                if (this.fs_info == null) return;

                this.cw.InstanceCount--;

                if (this.cw.InstanceCount == 0)
                {
                    var name = this.fs_info.FullName;

                    this.cw.Close();
                    cws.Remove(name);
                    fhs.Remove(name);
                }

                this.fs_info = null;
            }
        }

        #endregion

        #region "Public Operation Methods"
        public long Count => this.fh.Count;
        public bool IsOpen => this.fs_info != null;

        /// <summary>Add an item into the collection.</summary>
        public void Add(string PrimaryKey, byte[] Value) =>
            writeBegin(() =>
            {
                var key_bytes = System.Text.Encoding.UTF8.GetBytes(PrimaryKey);
                if (key_bytes.Length == 0) throw new Exception("Key length must be at least 1 character.");
                if (Exists(PrimaryKey)) throw new Exception("Key already exists.");

                insert(PrimaryKey, Value);
            });

        /// <summary>Updates the value of the [Key]. If [Key] does not exist, nothing updated.</summary>
        public void Update(string PrimaryKey, byte[] Value)
        {
            var rh = GetHeader(PrimaryKey);
            if (rh == null) return;
            writeBegin(() => update(rh, Value));
        }

        /// <summary>
        /// Sets the value of the [Key]. If [Key] does not exist, a new [Key] is created. If [Key] already exists in the collection, it is overwritten.
        /// <para>
        /// PrimaryKey value cannot be changed. Only can be changed other Key-Values . To clear Key-Value then set EMPTY value. To keep as is value of KEY, then leave NULL.
        /// </para>
        /// </summary>
        public void Upsert(string PrimaryKey, byte[] Value)
        {
            var rh = GetHeader(PrimaryKey);
            writeBegin(() =>
            {
                if (rh == null) insert(PrimaryKey, Value);
                else update(rh, Value);
            });
        }

        /// <summary>Deletes the value of the [Key]. If [Key] does not exist, nothing deleted.</summary>
        public void Delete(string PrimaryKey)
        {
            var rh = GetHeader(PrimaryKey);
            if (rh == null) return;
            writeBegin(() => delete(rh));
        }

        /// <summary>All records is removed from collection, and file is shrinked.</summary>
        public void Truncate() =>
            writeBegin(() =>
                {
                    this.cw.Truncate();
                    this.fh = new FileHeader();
                });

        public bool Exists(string PrimaryKey)
        {
            foreach (var row in GetHeaders())
                if (row.GetPrimaryKey == PrimaryKey) return true;
            return default;
        }

        public KeyValuePair<RowHeader, byte[]> GetFirst() => GetValue(this.fh.FirstRecStartPos);
        public KeyValuePair<RowHeader, byte[]> GetLast() => GetValue(this.fh.LastRecStartPos);
        public KeyValuePair<RowHeader, byte[]> GetValue(long Pos)
        {
            foreach (var row in Iterate(Pos, false))
                return row;

            return default;
        }
        public byte[] GetValue(string PrimaryKey)
        {
            foreach (var row in Iterate(this.fh.FirstRecStartPos, false))
                if (row.Key.GetPrimaryKey == PrimaryKey)
                    return row.Value;

            return default;
        }

        public RowHeader GetHeader(string PrimaryKey)
        {
            foreach (var row in Iterate(this.fh.FirstRecStartPos, true))
                if (row.Key.GetPrimaryKey == PrimaryKey)
                    return row.Key;

            return default;
        }

        public IEnumerable<RowHeader> GetHeaders()
        {
            foreach (var row in Iterate(this.fh.FirstRecStartPos, true))
                yield return row.Key;
        }

        /// <summary>Retrieves all the elements. </summary>
        public IEnumerable<KeyValuePair<RowHeader, byte[]>> All() => Iterate(this.fh.FirstRecStartPos,false);
        /// <summary>Retrieves all the elements. </summary>
        public IEnumerable<KeyValuePair<RowHeader, T>> All<T>()
        {
            foreach (var row in All())
                yield return KeyValuePair.Create(row.Key, Serializer.FromBytes<T>(row.Value));
        }
        private IEnumerable<KeyValuePair<RowHeader, byte[]>> Iterate(long StartPos, bool skipValue)
        {
            if (IsOpen)
                using (var fs = reader())
                {
                    fs.Seek(StartPos, SeekOrigin.Begin);
                    while (StartPos > 0)
                    {
                        var row = io_read_row(fs, StartPos, skipValue);
                        yield return row;
                        // GoTo Next
                        StartPos = row.Key.GetNextPos;
                    }
                }
        }
        #endregion


        #region "private methods"
        internal void writeBegin(Action fn)
        {
            if (IsOpen == false) return;

            this.cw.WriteBegin((Action)(() =>
            {
                fn?.Invoke();

                this.cw.Write(this.fh.Pos, this.fh.ToArray());
            }));
        }
        internal void insert(string PrimaryKey, byte[] Value)
        {
            var newPos = this.fh.FirstRecStartPos;
            // [NextPos] value of the [LastRecord] will be update; If any record has exists
            if (this.fh.Count > 0)
            {
                var last_rh = GetLast().Key;
                if (last_rh is object)
                {
                    newPos = last_rh.Pos + last_rh.RowSize;
                    last_rh.NextPos = newPos;
                    io_write_row_header(last_rh);
                    //this.cw.Write(RowHeader.GetPointer(fh.LastRecStartPos,RowHeaderPointers.NextPos), BitConverter.GetBytes(newPos));
                }
            }

            var rh = new RowHeader();
            { // header of new record
                rh.Pos = newPos;
                rh.PrevPos = this.fh.Count > 0 ? this.fh.LastRecStartPos : 0;
                rh.ValueLength = Value.Length;
                rh.ValueActualLength = Value.Length;
                rh.PrimaryKey = PrimaryKey;
            }


            { // header of the file is being updated
                this.fh.LastRecStartPos = rh.Pos;   // last position is new record-position
                this.fh.Count++;
            }

            { // record is being written
                io_write_row_header(rh, true);
                io_write_row_value(rh, Value);
            }
        }
        internal void update(RowHeader rh, byte[] Value)
        {
            if (rh == null) return;

            // if there is a next record and also new value is longer than old value,
            // then delete old record and insert as new.
            if (Value.Length > rh.ValueActualLength && rh.NextPos != 0)
            {
                delete(rh);
                insert(rh.GetPrimaryKey, Value);
            }
            else
            {
                /*
                // has data been changed ?
                var has_changed = rh.ValueLength != Value.Length;
                if (!has_changed)
                {
                    var old_value = GetValue(rh.PrimaryKey);    // <= performance concern ???
                    for (int i = 0; i < Value.Length; i++)
                        if (Value[i] != old_value[i])
                        {
                            has_changed = true;
                            break;
                        }
                if (!has_changed) return;
                }
                */

                rh.ValueLength = Value.Length;
                { // record is being written
                    io_write_row_header(rh, true);
                    io_write_row_value(rh, Value);
                }
            }
        }
        internal void delete(RowHeader rh)
        {
            if (rh == null) return;

            // is there previous record ?
            if (rh.PrevPos != 0)
            {
                this.cw.fs.Seek(rh.PrevPos, SeekOrigin.Begin);
                var prev_row_header = io_read_row(this.cw.fs, rh.PrevPos, true).Key;
                prev_row_header.NextPos = rh.NextPos;
                io_write_row_header(prev_row_header);
            }
            else
            { // there is no previous record.
                this.fh.FirstRecStartPos = rh.NextPos;
            }

            // is there next record ?
            if (rh.NextPos != 0)
            {
                this.cw.fs.Seek(rh.NextPos, SeekOrigin.Begin);
                var next_row_header = io_read_row(this.cw.fs, rh.NextPos, true).Key;
                next_row_header.PrevPos = rh.PrevPos;
                io_write_row_header(next_row_header);
            }
            else
            { // there is no next record.
                this.fh.LastRecStartPos = rh.PrevPos;
            }

            this.fh.Count--;
        }

        internal FileStream reader() => new FileStream(this.FileInfo.FullName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

        private KeyValuePair<RowHeader, byte[]> io_read_row(FileStream fs, long pos, bool skipValue = false)
        {
            // row-header
            var rh = new RowHeader();
            rh.FillBytes(fs);
            rh.Pos = pos;
            // Value
            if (skipValue)
            {
                fs.Read(new byte[rh.GetValueLength]);
                return KeyValuePair.Create(rh, new byte[0]);
            }
            else
            {
                var bytes = new byte[rh.GetValueLength];
                fs.Read(bytes);
                return KeyValuePair.Create(rh, bytes);
            }
        }
        internal void io_write_row_header(RowHeader rh, bool writePrimaryKey = false)
        {
            byte[] key_bytes = null;
            if (writePrimaryKey)
            {
                key_bytes = System.Text.Encoding.UTF8.GetBytes(rh.GetPrimaryKey);
                rh.KeyActualLength = (short)key_bytes.Length;
            }

            this.cw.Write(rh.Pos, rh.ToArray());

            if (writePrimaryKey)
                this.cw.Write(rh.KeyStartPos, key_bytes);
        }
        internal void io_write_row_value(RowHeader rh, byte[] data, bool updateIndex = true) => this.cw.Write(rh.ValueStartPos, data);
        #endregion
    }
}