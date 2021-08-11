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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static Dictionary<string, CollectionCache> caches = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal CollectionCache cache = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal System.IO.FileInfo fs_info = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal IndexerInfo indexerInfo;


        static CollectionBase()
        {
            cws = new Dictionary<string, CollectionWriter>();
            caches = new Dictionary<string, CollectionCache>();
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

            this.indexerInfo = CollectionIndexer.Get(CollectionName);

            lock (lock_ctor)
            {
                bool is_first = false;
                if (cws.ContainsKey(name) == false)
                {
                    cws.Add(name, new CollectionWriter(this.fs_info));
                    caches.Add(name, new CollectionCache());
                    is_first = true;
                }

                this.cw = cws[name];
                this.cache = caches[name];

                if (is_first)
                {
                    var fs = this.cw.dat;
                    //using (var fs = reader())
                    {
                        // file is being created newly (file-header is being initialized)
                        this.cache.FileHeader = new FileHeader();
                        if (fi_exists)
                        {
                            var bytes = new byte[this.cache.FileHeader.Size];
                            if (fs.Read(bytes) != bytes.Length)
                                throw new Exception("Invalid file header.");

                            this.cache.FileHeader.FromArray(bytes);
                        }
                        else
                        {
                            writeBegin(null);
                        }

                        // all row-headers will be cached
                        int hsiz = (new RowHeader()).Size;
                        var posx = this.cache.FileHeader.FirstRecStartPos;
                        fs.Seek(posx, SeekOrigin.Begin);
                        while (true)
                        {
                            byte[] bytes = null;
                            RowHeader row_header = null;
                            {
                                bytes = new byte[hsiz];
                                if (fs.Read(bytes) > 0)
                                {
                                    // row-header is being read
                                    row_header = new RowHeader();
                                    row_header.FromArray(bytes);
                                    row_header.Pos = posx;

                                    // indexes are being read after row-header
                                    bytes = new byte[row_header.KeyLength];
                                    if (fs.Read(bytes) > 0)
                                    {
                                        row_header.Keys = indexerInfo.DeserializeIndexValues(bytes, 0, row_header.KeyActualLength);
                                    }
                                }

                            }

                            if (row_header == null) break;
                            this.cache.Add(row_header);

                            if (row_header.NextPos == 0) break;

                            // code below takes 04.6148817 sec for 999.999 record.
                            //  fs_read.Position = row_header.NextPos;

                            //// code below takes 01.8439142 sec for 999.999 record.
                            //for (int i = 0; i < row_header.ValueLength; i++)
                            //    fs.ReadByte();

                            // code below takes 01.5439142 sec for 999.999 record.
                            fs.Read(new byte[row_header.ValueLength]);

                            posx = row_header.NextPos;
                        }
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
        public long Count => this.cache?.FileHeader.Count ?? 0;
        public bool IsOpen => this.fs_info != null;

        public IEnumerable<string> GetKeys()
        {
            if (IsOpen == false) return default;

            return cache.Keys();
        }

        /// <summary>Add an item into the collection.</summary>
        public void Add<T>(string PrimaryKey, T Value) =>
            writeBegin(() =>
            {
                var key_bytes = System.Text.Encoding.UTF8.GetBytes(PrimaryKey);
                if (key_bytes.Length == 0) throw new Exception("Key length must be at least 1 character.");
                if (this.cache.Exists(PrimaryKey)) throw new Exception("Key already exists.");

                insert(PrimaryKey, Value);
            });

        /// <summary>Updates the value of the [Key]. If [Key] does not exist, nothing updated.</summary>
        public void Update<T>(string PrimaryKey, T Value) =>
            writeBegin(() => update(this.cache.TryGet(PrimaryKey), Value));

        /// <summary>
        /// Sets the value of the [Key]. If [Key] does not exist, a new [Key] is created. If [Key] already exists in the collection, it is overwritten.
        /// <para>
        /// PrimaryKey value cannot be changed. Only can be changed other Key-Values . To clear Key-Value then set EMPTY value. To keep as is value of KEY, then leave NULL.
        /// </para>
        /// </summary>
        public void Upsert<T>(string PrimaryKey, T Value) =>
            writeBegin(() =>
            {
                var rowHeader = this.cache.TryGet(PrimaryKey);
                if (rowHeader == null)
                    insert(PrimaryKey, Value);
                else
                    update(rowHeader, Value);
            });

        /// <summary>Deletes the value of the [Key]. If [Key] does not exist, nothing deleted.</summary>
        public void Delete(string PrimaryKey) =>
            writeBegin(() => delete(this.cache.TryGet(PrimaryKey)));

        /// <summary>All records is removed from collection, and file is shrinked.</summary>
        public void Truncate() =>
            writeBegin(() =>
                {
                    this.cw.Truncate();
                    this.cache.Clear();
                    this.cache.FileHeader = new FileHeader();
                });

        public byte[] GetValue(string PrimaryKey) => GetValue(PrimaryKey, out _);
        public byte[] GetValue(string PrimaryKey, out RowHeader Header)
        {
            Header = default;
            if (IsOpen == false) return default;

            Header = this.cache.TryGet(PrimaryKey);
            using (var fs = reader())
                return io_read_row_value(fs, Header);
        }
        public T GetValue<T>(string PrimaryKey) => Serializer.FromBytes<T>(GetValue(PrimaryKey));
        public T GetValue<T>(string PrimaryKey, out RowHeader Header) => Serializer.FromBytes<T>(GetValue(PrimaryKey, out Header));

        public bool Exists(string PrimaryKey) => cache?.Exists(PrimaryKey) ?? false;

        public IEnumerable<KeyValuePair<string, T>> All<T>()
        {
            using (var fs = reader())
                foreach (var rh in this.cache.Items())
                    yield return new KeyValuePair<string, T>(rh.Key, Serializer.FromBytes<T>(io_read_row_value(fs, rh.Value)));
        }
        public IEnumerable<KeyValuePair<string, T>> FindAll<T>(Func<List<object>, bool> match)
        {
            using (var fs = reader())
                foreach (var rh in this.cache.Items())
                    if (match(rh.Value.Keys))
                        yield return new KeyValuePair<string, T>(rh.Key, Serializer.FromBytes<T>(io_read_row_value(fs, rh.Value)));
        }

        #endregion


        #region "private methods"
        internal void writeBegin(Action fn)
        {
            if (IsOpen == false) return;

            this.cw.WriteBegin((Action)(() =>
            {
                fn?.Invoke();

                var fh = this.cache.FileHeader;
                this.cw.Write(fh.Pos, fh.ToArray());
            }));
        }
        internal void insert(string PrimaryKey, object Value)
        {
            var fh = this.cache.FileHeader;
            var newPos = fh.FirstRecStartPos;
            // [NextPos] value of the [LastRecord] will be update; If any record has exists
            if (fh.Count > 0)
            {
                var lastRecHeader = this.cache.TryGet(fh.LastRecStartPos);
                if (lastRecHeader is object)
                {
                    newPos = lastRecHeader.Pos + lastRecHeader.RowSize;
                    lastRecHeader.NextPos = newPos;
                    io_write_row_header(lastRecHeader, true);
                }
            }

            var bytes = Serializer.ToBytes(Value);

            var rh = new RowHeader();
            { // header of new record
                rh.Pos = newPos;
                rh.PrevPos = fh.Count > 0 ? fh.LastRecStartPos : 0;
                rh.ValueLength = bytes.Length;
                rh.ValueActualLength = bytes.Length;
                rh.Keys = indexerInfo.CreateValues(PrimaryKey, Value);
            }


            { // header of the file is being updated
                fh.LastRecStartPos = rh.Pos;   // last position is new record-position
                fh.Count++;
            }

            { // record is being written
                io_write_row_header(rh, false, true);
                io_write_row_value(rh, bytes);
            }
        }
        internal void update(RowHeader rh, object Value)
        {
            if (rh == null) return;

            var fh = this.cache.FileHeader;
            var bytes = Serializer.ToBytes(Value);

            // if there is a next record and also new value is longer than old value,
            // then delete old record and insert as new.
            if (bytes.Length > rh.ValueActualLength && rh.NextPos != 0)
            {
                delete(rh);
                insert(rh.PrimaryKey, Value);
            }
            else
            {
                /*
                // has data been changed ?
                var has_changed = rh.ValueLength != bytes.Length;
                if (!has_changed)
                {
                    var old_value = GetValue(rh.PrimaryKey);    // <= performance concern ???
                    for (int i = 0; i < bytes.Length; i++)
                        if (bytes[i] != old_value[i])
                        {
                            has_changed = true;
                            break;
                        }
                if (!has_changed) return;
                }
                */

                rh.ValueLength = bytes.Length;
                rh.Keys = indexerInfo.CreateValues(rh.PrimaryKey, Value);
                { // record is being written
                    io_write_row_header(rh, true, true);
                    io_write_row_value(rh, bytes);
                }
            }
        }
        internal void delete(RowHeader rh)
        {
            if (rh == null) return;

            var fh = this.cache.FileHeader;
            // is there previous record ?
            if (rh.PrevPos != 0)
            {
                var prev_row_header = this.cache.TryGet(rh.PrevPos);
                prev_row_header.NextPos = rh.NextPos;
                io_write_row_header(prev_row_header, true);
            }
            else
            { // there is no previous record.
                fh.FirstRecStartPos = rh.NextPos;
            }

            // is there next record ?
            if (rh.NextPos != 0)
            {
                var next_row_header = this.cache.TryGet(rh.NextPos);
                next_row_header.PrevPos = rh.PrevPos;
                io_write_row_header(next_row_header, true);
            }
            else
            { // there is no next record.
                fh.LastRecStartPos = rh.PrevPos;
            }

            fh.Count--;

            this.cache.Remove(rh);
        }

        internal FileStream reader() => new FileStream(this.FileInfo.FullName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);

        internal void io_write_row_header(RowHeader rh, bool forUpdate, bool writeIndexInfo = false)
        {
            byte[] key_bytes = null;
            if (writeIndexInfo)
            {
                key_bytes = indexerInfo.SerializeIndexValues(rh.Keys);
                rh.KeyActualLength = (short)key_bytes.Length;
            }
            if (forUpdate)
                this.cache.Update(rh);
            else
                this.cache.Add(rh);

            this.cw.Write(rh.Pos, rh.ToArray());

            if (writeIndexInfo)
                this.cw.Write(rh.PosKeyStart, key_bytes);
        }
        internal void io_write_row_value(RowHeader rh, byte[] data, bool updateIndex = true) => this.cw.Write(rh.PosValueStart, data);
        internal byte[] io_read_row_value(FileStream fs, RowHeader rh)
        {
            if (rh == null) return default;
            if (rh.ValueLength < 1) return default;

            var retval = new byte[rh.ValueLength];
            fs.Seek(rh.PosValueStart, SeekOrigin.Begin);
            return fs.Read(retval) == 0 ? default : retval;
        }
        #endregion
    }
}