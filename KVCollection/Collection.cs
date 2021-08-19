using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KeyValue
{
    public class Collection : IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string name;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static object lock_ctor = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static Dictionary<string, CollectionWriter> cws = null;   // CollectionWriters
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal CollectionWriter cw = null;                               // CollectionWriter

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static Dictionary<string, FileHeader> fhs = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal FileHeader fh = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static Dictionary<string, HashSet<string>> hss = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal HashSet<string> hs = null;

        static Collection()
        {
            cws = new Dictionary<string, CollectionWriter>();
            fhs = new Dictionary<string, FileHeader>();
            hss = new Dictionary<string, HashSet<string>>();
        }

        public void Dispose() => Close();


        #region "Open/Close"
        public string Name => name;
        public string IndexFile => name + ".inx";
        public string DataFile => name + ".dat";
        public bool IsOpen => this.name != null;
        /// <summary>
        /// Opens the collection specified in a directory. 
        /// </summary>
        /// <param name="Directory">The directory path that the collection is in.</param>
        /// <param name="CollectionName">Collection name without extension.</param>
        public void Open(string Directory, string CollectionName)
        {
            var name = System.IO.Path.Combine(Directory, CollectionName);

            if (this.name == name) return;
            if (this.name != null) this.Close();
            this.name = name;

            // index and data files are must be exist at same time.
            var fi_exists_inx = System.IO.File.Exists(this.IndexFile);
            var fi_exists_dat = System.IO.File.Exists(this.DataFile);

            if (fi_exists_inx || fi_exists_dat)
            {
                if (fi_exists_inx == false)
                    throw new Exception("Index file is not exists. (File=" + this.IndexFile + ")");

                if (fi_exists_dat == false)
                    throw new Exception("Data file is not exists. (File=" + this.DataFile + ")");
            }

            lock (lock_ctor)
            {
                bool is_first = false;
                if (cws.ContainsKey(name) == false)
                {
                    cws.Add(name, new CollectionWriter(Directory, CollectionName));
                    fhs.Add(name, new FileHeader());
                    hss.Add(name, new HashSet<string>());
                    is_first = true;
                }

                this.cw = cws[name];
                this.fh = fhs[name];
                this.hs = hss[name];

                if (is_first)
                {
                    var fs = this.cw.fs_inx;
                    // file is being created newly (file-header is being initialized)
                    if (fi_exists_inx && fs.Length > 0)
                    {
                        fs.Position = 0;
                        var bytes = new byte[FileHeader.Size];
                        if (fs.Read(bytes) != bytes.Length)
                            throw new Exception("Invalid file header.");

                        this.fh.FromArray(bytes);
                    }
                    else
                    {
                        WriteBegin(null);
                    }

                    //var headers = GetHeaders().ToList();
                    //if (headers.Count > 0)
                    //{
                    //    foreach (var rh in headers)
                    //        hs.Add(rh.PrimaryKey);

                    //}
                }
            }

            this.cw.InstanceCount++;
        }
        public void Close()
        {
            lock (lock_ctor)
            {
                if (this.name == null) return;
                this.cw.InstanceCount--;

                if (this.cw.InstanceCount == 0)
                {
                    this.cw.Close();
                    cws.Remove(this.name);
                    fhs.Remove(this.name);
                    hss.Remove(this.name);
                }
                this.name = null;
            }
        }
        #endregion

        #region "Public Operation Methods"
        public long Count => this.fh.Count;

        #region "Add"
        /// <summary>Add an item into the collection.</summary>
        public RowHeader AddRaw(byte[] Value, IEnumerable<object> IndexValues = default) =>
            multi_op(mc => mc.Add(Value, IndexValues));
        /// <summary>Add an item into the collection.</summary>
        public RowHeader Add<T>(T Item) => AddMany(new[] { Item });
        /// <summary>Add an items into the collection. Return RowHeader for the last item has appended.</summary>
        public RowHeader AddMany<T>(IEnumerable<T> Items)
        {
            var inx_info = GetIndexerInfo<T>();
            return multi_op(mc =>
            {
                foreach (var item in Items)
                    mc.Add(Serializer.GetBytes(item), inx_info.CreateValues(item));
            });
        }
        #endregion

        #region "Update"
        /// <summary>Updates the item by the [Row-ID]. If the item does not exist, exception occured.</summary>
        public RowHeader UpdateRaw(int Id, byte[] Value, IEnumerable<object> IndexValues = default) =>
            multi_op(mc => mc.Update(rh => rh.Id == Id, Value, IndexValues));
        /// <summary>Updates the item by the [first index value]. If the item does not exist, exception occured.</summary>
        public RowHeader UpdateRaw(byte[] Value, IEnumerable<object> IndexValues = default) =>
             multi_op(mc => mc.Update(rh => rh.IndexValues[0].Equals(IndexValues.FirstOrDefault()), Value, IndexValues));
        /// <summary>Updates the item by the [Row-ID]. If the item does not exist, exception occured.</summary>
        public RowHeader Update<T>(int Id, T Value)
        {
            var inx_info = GetIndexerInfo<T>();
            return multi_op(mc => mc.Update(rh => rh.Id == Id, Serializer.GetBytes(Value), inx_info.CreateValues(Value)));
        }
        /// <summary>Updates the items by the [Row-ID]. If the item does not exist, exception occured.</summary>
        public RowHeader UpdateMany<T>(IEnumerable<(int Id, T Value)> Items)
        {
            var inx_info = GetIndexerInfo<T>();
            return multi_op(mc =>
            {
                foreach (var item in Items)
                    mc.Update(rh => rh.Id == item.Id, Serializer.GetBytes(item), inx_info.CreateValues(item));
            });
        }
        /// <summary>Updates the item by the [first index value]. If the item does not exist, exception occured.</summary>
        public RowHeader Update<T>(T Item) => UpdateMany(new[] { Item });
        /// <summary>Updates the items by the [first index value]. If the item does not exist, exception occured.</summary>
        public RowHeader UpdateMany<T>(IEnumerable<T> Items)
        {
            var inx_info = GetIndexerInfo<T>();
            return multi_op(mc =>
            {
                foreach (var item in Items)
                {
                    var indexValues = inx_info.CreateValues(item).ToArray();
                    mc.Update(rh => rh.IndexValues[0].Equals(indexValues[0]), Serializer.GetBytes(item), indexValues);
                }
            });
        }
        #endregion

        #region "Upsert"
        /// <summary>Sets the item by [Row-ID]. If [Row-ID] does not exist, a new item is created. If [Row-ID] already exists in the collection, it is overwritten.</summary>
        public RowHeader UpsertRaw(int Id, byte[] Value, IEnumerable<object> IndexValues = default) =>
            multi_op(mc => mc.Upsert(rh => rh.Id == Id, Value, IndexValues));
        /// <summary>Sets the item by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public RowHeader UpsertRaw(byte[] Value, IEnumerable<object> IndexValues = default) =>
             multi_op(mc => mc.Upsert(rh => rh.IndexValues[0].Equals(IndexValues.FirstOrDefault()), Value, IndexValues));
        /// <summary>Sets the item by [Row-ID]. If [Row-ID] does not exist, a new item is created. If [Row-ID] already exists in the collection, it is overwritten.</summary>
        public RowHeader Upsert<T>(int Id, T Value)
        {
            var inx_info = GetIndexerInfo<T>();
            return multi_op(mc => mc.Upsert(rh => rh.Id == Id, Serializer.GetBytes(Value), inx_info.CreateValues(Value)));
        }
        /// <summary>Sets the item by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public RowHeader Upsert<T>(T Item) => UpsertMany(new[] { Item });
        /// <summary>Sets the items by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public RowHeader UpsertMany<T>(IEnumerable<T> Items)
        {
            var inx_info = GetIndexerInfo<T>();
            return multi_op(mc =>
            {
                foreach (var item in Items)
                {
                    var indexValues = inx_info.CreateValues(item).ToArray();
                    mc.Upsert(rh => rh.IndexValues[0].Equals(indexValues[0]), Serializer.GetBytes(item), indexValues);
                }
            });
        }
        //base.Upsert(Serializer.GetBytes(Value), inx_info.CreateValues(Value));
        #endregion

        #region "Delete"
        /// <summary>Deletes the item by the [Row-ID]. If the item does not exist, nothing deleted.</summary>
        public void Delete(int Id) => DeleteMany(new[] { Id });
        /// <summary>Deletes the items by the [Row-ID]. If the item does not exist, nothing deleted.</summary>
        public void DeleteMany(IEnumerable<int> Ids) => multi_op(mc =>
        {
            foreach (var id in Ids)
                mc.Delete(rh => rh.Id == id);
        });
        /// <summary>Deletes the item by the [first index value]. If the item does not exist, nothing deleted.</summary>
        public void DeleteByKey(object FirstIndexValue) => DeleteMany(new[] { FirstIndexValue });
        /// <summary>Deletes the item by the [first index value]. If the item does not exist, nothing deleted.</summary>
        public void DeleteByKey(IEnumerable<object> FirstIndexValues) => multi_op(mc =>
        {
            foreach (var FirstIndexValue in FirstIndexValues)
                mc.Delete(rh => rh.IndexValues[0].Equals(FirstIndexValue));
        });
        /// <summary>Deletes the item by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public void Delete<T>(T Item) => DeleteMany(new[] { Item });
        /// <summary>Deletes the items by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public void DeleteMany<T>(IEnumerable<T> Items) => multi_op(mc =>
        {
            var inx_info = GetIndexerInfo<T>();
            foreach (var item in Items)
                mc.Delete(rh => rh.IndexValues[0].Equals(inx_info.CreateValues(item).FirstOrDefault()));
        });
        #endregion

        #region "Truncate"
        /// <summary>All records is removed from collection, and file is shrinked.</summary>
        public void Truncate() =>
            WriteBegin(() =>
            {
                this.cw.Truncate();
                this.fh = new FileHeader();

                hs.Clear();
            });
        #endregion

        #region "GET Methods"
        private KeyValuePair<RowHeader, T> toType<T>(KeyValuePair<RowHeader, byte[]> row)
        {
            if (row.Value == null) return default;
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }

        #region "GetById"
        public KeyValuePair<RowHeader, byte[]> GetRaw(int Id) => io_read_forward(FileHeader.Size, (rh) => rh.Id == Id).FirstOrDefault();
        public KeyValuePair<RowHeader, T> Get<T>(int Id) => toType<T>(GetRaw(Id));
        #endregion

        #region "GetByPos"
        /// <summary>retreives the item by the position of the item.</summary>
        /// <param name="Pos">is the position of the item.</param>
        public KeyValuePair<RowHeader, byte[]> GetRawByPos(long Pos) => io_read_forward(Pos, (x) => true).FirstOrDefault();
        /// <summary>retreives the item by the position of the item.</summary>
        /// <param name="Pos">is the position of the item.</param>
        public KeyValuePair<RowHeader, T> GetByPos<T>(long Pos) => toType<T>(GetRawByPos(Pos));
        #endregion

        #region "GetByRowHeader"
        public KeyValuePair<RowHeader, byte[]> GetRaw(RowHeader rowHeader) => GetRawByPos(rowHeader?.Pos ?? 0);
        public KeyValuePair<RowHeader, T> Get<T>(RowHeader rowHeader) => GetByPos<T>(rowHeader?.Pos ?? 0);
        #endregion

        #region "GetFirst"
        public KeyValuePair<RowHeader, byte[]> GetRawFirst() => GetRawByPos(FileHeader.Size);
        public KeyValuePair<RowHeader, T> GetFirst<T>() => GetByPos<T>(FileHeader.Size);
        #endregion

        #region "GetLast"
        public KeyValuePair<RowHeader, byte[]> GetRawLast() => GetRawByPos(this.cw.fs_inx.Length - RowHeader.Size);
        public KeyValuePair<RowHeader, T> GetLast<T>() => GetByPos<T>(this.cw.fs_inx.Length - RowHeader.Size);
        #endregion

        #region "GetHeader(s)"
        public IEnumerable<RowHeader> GetHeaders()
        {
            foreach (var row in io_read_forward(FileHeader.Size, (rh) => false))
                yield return row.Key;
        }
        public RowHeader GetHeader(int Id) => GetHeaders().FirstOrDefault(row => row.Id == Id);
        public RowHeader GetHeaderByPos(long Pos) => GetHeaders().FirstOrDefault(row => row.Pos == Pos);
        public RowHeader GetHeaderByKey(object FirstIndexValue) => FirstIndexValue == null ? null : GetHeaders().FirstOrDefault(row => row.IndexValues[0].Equals(FirstIndexValue));
        #endregion

        #region "Exists"
        public bool Exists(int Id) => GetHeader(Id) is object;
        public bool ExistsByPos(long Pos) => GetHeaderByPos(Pos) is object;
        public bool ExistsByKey(object FirstIndexValue) => GetHeaderByKey(FirstIndexValue) is object;
        #endregion

        #region "GetByKey"
        /// <summary>retreives the item by the first IndexValue.</summary>
        /// <param name="FirstIndexValue">is the first IndexValue to be searched.</param>
        public KeyValuePair<RowHeader, byte[]> GetRawByIndex(object FirstIndexValue) => io_read_forward(FileHeader.Size, (rh) => rh.IndexValues[0].Equals(FirstIndexValue)).FirstOrDefault();
        /// <summary>retreives the item by the first IndexValue.</summary>
        /// <param name="FirstIndexValue">is the first IndexValue to be searched.</param>
        public KeyValuePair<RowHeader, T> GetByIndex<T>(object FirstIndexValue) => toType<T>(GetRawByIndex(FirstIndexValue));
        #endregion

        #region "GetAll"
        /// <summary>Retrieves all the elements.</summary>
        public IEnumerable<KeyValuePair<RowHeader, byte[]>> GetRawAll() => io_read_forward(FileHeader.Size, (x) => true);
        /// <summary>Retrieves all the elements.</summary>
        public IEnumerable<KeyValuePair<RowHeader, T>> GetAll<T>()
        {
            foreach (var row in GetRawAll())
                yield return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }
        /// <summary>Retrieves all the elements by searching on indexValues.</summary>
        public IEnumerable<KeyValuePair<RowHeader, byte[]>> GetRawAll(Predicate<object[]> match)
        {
            foreach (var row in io_read_forward(FileHeader.Size, (rh) => match(rh.IndexValues)))
                if (row.Value != null)
                    yield return KeyValuePair.Create(row.Key, row.Value);
        }
        /// <summary>Retrieves all the elements by searching on indexValues.</summary>
        public IEnumerable<KeyValuePair<RowHeader, T>> GetAll<T>(Predicate<object[]> match)
        {
            foreach (var row in GetRawAll(match))
                yield return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }
        #endregion
        #endregion
        #endregion


        #region "private methods"
        private void WriteBegin(Action fn)
        {
            this.cw?.WriteBegin((Action)(() =>
            {
                fn?.Invoke();
                // file header is being updated...
                this.cw.Write(this.fh.Pos, this.fh.ToArray(), 0);
            }));
        }
        private IndexerInfo GetIndexerInfo<T>()
        {
            var retval = CollectionIndexer.Get<T>();
            if (retval == null) throw new Exception("There is no collection index information for " + typeof(T).FullName + ".");
            if (retval.index_value_getter_fns.Count == 0) throw new Exception("At least one index info must be specified by " + nameof(Indexer<T>.EnsureIndex) + " for " + typeof(T).FullName + ".");
            return retval;
        }
        private RowHeader insert(byte[] Value, IEnumerable<object> IndexValues)
        {
            var rh = new RowHeader();
            { // header of new record
                rh.Id = this.fh.LastId + 1;
                rh.Pos = this.cw.fs_inx.Length;
                rh.ValuePos = this.cw.fs_dat.Length;
                rh.ValueSize = Value.Length;
                rh.ValueActualSize = Value.Length;
                rh.IndexValues = IndexValues.ToArray();
            }

            { // header of the file is being updated
                this.fh.LastId = rh.Id;
                this.fh.Count++;
            }

            { // record is being written
                io_write_row_header(rh);
                io_write_row_value(rh, Value);
            }

            return rh;
        }
        private RowHeader update(RowHeader rh, byte[] Value, IEnumerable<object> IndexValues)
        {
            if (rh == null) throw new Exception("Update error. RowHeader must be specifed on update.");

            // if there is a next record and also new value is longer than old value,
            // then delete old record and insert as new.
            if ((Value.Length > rh.ValueActualSize) && io_is_last(rh) == false)
            {
                delete(rh);
                return insert(Value, IndexValues);
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

                rh.ValueActualSize = Value.Length;
                rh.IndexValues = IndexValues.ToArray();
                { // record is being written
                    io_write_row_header(rh);
                    io_write_row_value(rh, Value);
                }
                return rh;
            }
        }
        private void delete(RowHeader rh)
        {
            if (rh == null) return;
            rh.SetDeleted();
            io_write_row_header(rh);
            this.fh.Count--;
        }

        protected class multi_crud
        {
            public List<multi_crud_item> Items = new List<multi_crud_item>();

            public void Add(byte[] Value, IEnumerable<object> IndexValues) =>
                Items.Add(new multi_crud_item() { IsAdd = true, Value = Value, IndexValues = IndexValues });

            public void Update(Predicate<RowHeader> match, byte[] Value, IEnumerable<object> IndexValues) =>
                Items.Add(new multi_crud_item() { IsUpdate = true, match = match, Value = Value, IndexValues = IndexValues });

            public void Upsert(Predicate<RowHeader> match, byte[] Value, IEnumerable<object> IndexValues) =>
                Items.Add(new multi_crud_item() { IsUpsert = true, match = match, Value = Value, IndexValues = IndexValues });

            public void Delete(Predicate<RowHeader> match) =>
                Items.Add(new multi_crud_item() { IsDelete = true, match = match });

            public bool HasExecutable => Items.Exists(x => x.IsExec == false);
        }
        protected class multi_crud_item
        {
            public bool IsAdd;
            public bool IsUpdate;
            public bool IsUpsert;
            public bool IsDelete;
            public Predicate<RowHeader> match;
            public byte[] Value;
            public IEnumerable<object> IndexValues;
            public bool IsExec;
        }
        protected RowHeader multi_op(Action<multi_crud> fn)
        {
            RowHeader retval = null;
            var mc = new multi_crud();
            fn(mc);
            if (mc.Items.Count == 0) return retval;

            // operates first [ADD] items.
            foreach (var item in mc.Items)
                if (item.IsAdd)
                {
                    WriteBegin(() => retval = insert(item.Value, item.IndexValues));
                    item.IsExec = true;
                }
            if (mc.HasExecutable == false) return retval;

            // if there are items other than [ADD], then operates [UPDATE / UPSERT / DELETE] items.
            foreach (var row in io_read_forward(FileHeader.Size, (rh) => false))
                foreach (var item in mc.Items)
                    if (item.IsExec == false && item.match(row.Key))
                    {
                        if (item.IsUpdate || item.IsUpsert)
                            WriteBegin(() => retval = update(row.Key, item.Value, item.IndexValues));
                        else if (item.IsDelete)
                            WriteBegin(() => delete(row.Key));

                        item.IsExec = true;
                        if (mc.HasExecutable == false) break;
                    }
            if (mc.HasExecutable == false) return retval;

            // operates last [UPSERT] items not found.
            foreach (var item in mc.Items)
                if (item.IsExec == false && item.IsUpsert)
                    WriteBegin(() => retval = insert(item.Value, item.IndexValues));


            return retval;
        }

        private bool io_is_last(RowHeader rh) => (rh.Pos + RowHeader.Size) == this.cw.fs_inx.Length;
        private IEnumerable<KeyValuePair<RowHeader, byte[]>> io_read_forward(long StartPos, Func<RowHeader, bool> readValue = null)
        {
            if (IsOpen == false || StartPos < FileHeader.Size) yield break;

            FileStream new_fs(string nam) =>
                new FileStream(nam, FileMode.OpenOrCreate,
                FileAccess.Read, FileShare.ReadWrite,
                4096 * 4, FileOptions.SequentialScan);

            using (var fs_inx = new_fs(this.IndexFile))
            using (var fs_dat = new_fs(this.DataFile))
            {
                int rowCount = (int)((fs_inx.Length - StartPos) / RowHeader.Size);
                if (rowCount < 1) yield break;

                fs_inx.Seek(StartPos, SeekOrigin.Begin);
                //bool valuePosNotSet = true;
                for (int i = 0; i < rowCount; i++)
                {
                    // row-header bytes are being read
                    var bytes = new byte[RowHeader.Size];
                    var retval = fs_inx.Read(bytes);
                    if (retval != RowHeader.Size) yield break;

                    // row-header is being initialized from bytes read.
                    var rh = new RowHeader();
                    rh.FromArray(bytes, 0);

                    if (rh.Id == 0) continue; // is deleted.
                    rh.Pos = StartPos;

                    // Value
                    byte[] valueBytes = null;
                    if (readValue != null && readValue(rh))
                    {
                        //if (valuePosNotSet)
                        if (fs_dat.Position != rh.ValuePos)
                        {
                            fs_dat.Position = rh.ValuePos;
                            //valuePosNotSet = false;
                        }

                        valueBytes = new byte[rh.ValueActualSize];
                        fs_dat.Read(valueBytes);
                    }

                    yield return KeyValuePair.Create(rh, valueBytes);


                    StartPos += RowHeader.Size;
                }
            }
        }
        //private IEnumerable<RowHeader> io_read_backward(long StartPos = -1, bool readValue = false)
        //{
        //    if (this.fh.Count == 0) yield break;
        //    if (StartPos == -1) StartPos = this.cw.fs_inx.Length;
        //    if (StartPos <= FileHeader.Size) yield break;

        //    int rowCount = (int)((StartPos - FileHeader.Size) / RowHeader.Size);
        //    if (rowCount < 1) yield break;

        //    var bufferRow = Math.Min(10, rowCount);
        //    while (bufferRow > 0)
        //    {
        //        var bufferLen = RowHeader.Size * bufferRow;

        //        this.cw.fs_inx.Position = StartPos - bufferLen;

        //        var bytes = new byte[bufferLen];
        //        this.cw.fs_inx.Read(bytes);

        //        for (int i = bufferRow; i > 0; i--)
        //        {
        //            var bytePos = (bufferRow - 1) * RowHeader.Size;
        //            if (bytes[bytePos] == 0) continue;  // is deleted

        //            var retval = new RowHeader();
        //            retval.FromArray(bytes, bytePos);
        //            yield return retval;
        //        }
        //        rowCount -= bufferRow;
        //    }
        //}

        internal void io_write_row_header(RowHeader rh) =>
            this.cw.Write(rh.Pos, rh.ToArray(true), 0);

        internal void io_write_row_value(RowHeader rh, byte[] data) =>
            this.cw.Write(rh.ValuePos, data, 1);
        #endregion
    }
}