using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace KV
{
    public class Collection : IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string dir;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string colName;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string name;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal CollectionReader reader = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static object lock_ctor = new object();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static Dictionary<string, CollectionWriter> cws = null;   // CollectionWriters
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal CollectionWriter cw = null;                               // CollectionWriter

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal static Dictionary<string, FileHeader> fhs = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] internal FileHeader fh = null;

        static Collection()
        {
            cws = new Dictionary<string, CollectionWriter>();
            fhs = new Dictionary<string, FileHeader>();
        }

        public void Dispose() => Close();


        #region "Open/Close"
        public string Directory => dir;
        public string CollectionName => colName;
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
            this.dir = Directory;
            this.colName = CollectionName;
            this.name = name;
            this.reader = new CollectionReader(this);

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
                    is_first = true;
                }

                this.cw = cws[name];
                this.fh = fhs[name];

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
                        if (this.fh.State == FileState.Shrink)
                            this.Shrink();  // resume the shrink
                    }
                    else
                    {
                        WriterBegin(null);
                    }
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
                }
                this.name = null;
            }
        }
        #endregion

        #region "Public Operation Methods"
        public long Count => this.fh.Count;

        public IndexerInfo GetIndexerInfo<T>()
        {
            var retval = CollectionIndexer.Get<T>();
            if (retval == null) throw new Exception("There is no collection index information for " + typeof(T).FullName + ".");
            if (retval.index_value_getter_fns.Count == 0) throw new Exception("At least one index info must be specified by " + nameof(Indexer<T>.EnsureIndex) + " for " + typeof(T).FullName + ".");
            return retval;
        }

        #region "Add"
        /// <summary>Add an item into the collection.</summary>
        public RowHeader AddRaw(byte[] Value, IEnumerable<object> IndexValues = default) =>
            writer_multi(mc => mc.Add(Value, IndexValues));
        /// <summary>Add an item into the collection.</summary>
        public RowHeader Add<T>(T Item) => AddMany(new[] { Item });
        /// <summary>Add an items into the collection. Return RowHeader for the last item has appended.</summary>
        public RowHeader AddMany<T>(IEnumerable<T> Items)
        {
            var inx_info = GetIndexerInfo<T>();
            return writer_multi(mc =>
            {
                foreach (var item in Items)
                    mc.Add(Serializer.GetBytes(item), inx_info.CreateValues(item));
            });
        }
        #endregion

        #region "Update"
        /// <summary>Updates the item by the [Row-ID]. If the item does not exist, exception occured.</summary>
        public RowHeader UpdateRaw(int Id, byte[] Value, IEnumerable<object> IndexValues = default) =>
            writer_multi(mc => mc.Update(rh => rh.Id == Id, Value, IndexValues));
        /// <summary>Updates the item by the [first index value]. If the item does not exist, exception occured.</summary>
        public RowHeader UpdateRaw(byte[] Value, IEnumerable<object> IndexValues = default) =>
             writer_multi(mc => mc.Update(rh => rh.IndexValues[0].Equals(IndexValues.FirstOrDefault()), Value, IndexValues));
        /// <summary>Updates the item by the [Row-ID]. If the item does not exist, exception occured.</summary>
        public RowHeader Update<T>(int Id, T Value)
        {
            var inx_info = GetIndexerInfo<T>();
            return writer_multi(mc => mc.Update(rh => rh.Id == Id, Serializer.GetBytes(Value), inx_info.CreateValues(Value)));
        }
        /// <summary>Updates the items by the [Row-ID]. If the item does not exist, exception occured.</summary>
        public RowHeader UpdateMany<T>(IEnumerable<(int Id, T Value)> Items)
        {
            var inx_info = GetIndexerInfo<T>();
            return writer_multi(mc =>
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
            return writer_multi(mc =>
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
            writer_multi(mc => mc.Upsert(rh => rh.Id == Id, Value, IndexValues));
        /// <summary>Sets the item by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public RowHeader UpsertRaw(byte[] Value, IEnumerable<object> IndexValues = default) =>
             writer_multi(mc => mc.Upsert(rh => rh.IndexValues[0].Equals(IndexValues.FirstOrDefault()), Value, IndexValues));
        /// <summary>Sets the item by [Row-ID]. If [Row-ID] does not exist, a new item is created. If [Row-ID] already exists in the collection, it is overwritten.</summary>
        public RowHeader Upsert<T>(int Id, T Value)
        {
            var inx_info = GetIndexerInfo<T>();
            return writer_multi(mc => mc.Upsert(rh => rh.Id == Id, Serializer.GetBytes(Value), inx_info.CreateValues(Value)));
        }
        /// <summary>Sets the item by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public RowHeader Upsert<T>(T Item) => UpsertMany(new[] { Item });
        /// <summary>Sets the items by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public RowHeader UpsertMany<T>(IEnumerable<T> Items)
        {
            var inx_info = GetIndexerInfo<T>();
            return writer_multi(mc =>
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
        public void DeleteMany(IEnumerable<int> Ids) => writer_multi(mc =>
        {
            foreach (var id in Ids)
                mc.Delete(rh => rh.Id == id);
        });
        /// <summary>Deletes the item by the [Row-ID]. If the item does not exist, nothing deleted.</summary>
        public void Delete(long Position) => DeleteMany(new[] { Position });
        /// <summary>Deletes the items by the [Row-ID]. If the item does not exist, nothing deleted.</summary>
        public void DeleteMany(IEnumerable<long> Positions) => writer_multi(mc =>
        {
            foreach (var Position in Positions)
                WriterBegin(() => writer_delete(Position));
        });
        /// <summary>Deletes the item by the [first index value]. If the item does not exist, nothing deleted.</summary>
        public void DeleteByKey(object FirstIndexValue) => DeleteMany(new[] { FirstIndexValue });
        /// <summary>Deletes the item by the [first index value]. If the item does not exist, nothing deleted.</summary>
        public void DeleteByKey(IEnumerable<object> FirstIndexValues) => writer_multi(mc =>
        {
            foreach (var FirstIndexValue in FirstIndexValues)
                mc.Delete(rh => rh.IndexValues[0].Equals(FirstIndexValue));
        });
        /// <summary>Deletes the item by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public void Delete<T>(T Item) => DeleteMany(new[] { Item });
        /// <summary>Deletes the items by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public void DeleteMany<T>(IEnumerable<T> Items) => writer_multi(mc =>
        {
            var inx_info = GetIndexerInfo<T>();
            foreach (var item in Items)
                mc.Delete(rh => rh.IndexValues[0].Equals(inx_info.CreateValues(item).FirstOrDefault()));
        });
        #endregion

        #region "Truncate"
        /// <summary>All records is removed from collection, and file is shrinked.</summary>
        public void Truncate() =>
            WriterBegin(() =>
            {
                this.cw.Truncate();
                this.fh = new FileHeader();
            });
        public void Shrink()
        {
            WriterBegin(() => this.fh.State = FileState.Shrink, CheckState: false);
            var fi = new System.IO.FileInfo(name);
            var inx_file = this.IndexFile;
            var dat_file = this.DataFile;

            {
                //var inx_pos = FileHeader.Size;
                var inx_pos = FileHeader.Size;
                var dat_pos = 0;
                var last_id = -1;
                bool has_deleted = false;
                foreach (var item in this.reader.Create<byte[]>(reverse: false, readValue: true, readDeleted: true, CheckState: false))
                {

                    // if the shrink operation aborted, is continued
                    // then the row with same ID can be iterate
                    if (last_id == item.Header.Id) continue;
                    last_id = item.Header.Id;

                    if (item.Header.IsDeleted)
                    {
                        has_deleted = true;
                        continue;
                    }

                    if (has_deleted)
                        WriterBegin(() =>
                        {
                            item.Header.Pos = inx_pos;
                            item.Header.ValueSize = item.Data.Length;
                            item.Header.ValueActualSize = item.Data.Length;
                            item.Header.ValuePos = dat_pos;

                            writer_set_row_header(item.Header);
                            writer_set_row_value(item.Header, item.Data);
                        }, CheckState: false);

                    inx_pos += RowHeader.Size;
                    dat_pos += item.Data.Length;
                }
                this.cw.fs_inx.SetLength(inx_pos);
                this.cw.fs_inx.Flush();

                this.cw.fs_dat.SetLength(dat_pos);
                this.cw.fs_dat.Flush();
            }
            WriterBegin(() => this.fh.State = FileState.Normal, CheckState: false);

            //// collection writer re-initialize for new pointers.
            //cws[name].Close();
            //cws[name]= new CollectionWriter(Directory, CollectionName);
            //this.cw = cws[name];
        }
        #endregion

        #region "GET Methods"

        #region "GetByIndex"
        /// <summary>retreives the item by the first IndexValue.</summary>
        /// <param name="FirstIndexValue">is the first IndexValue to be searched.</param>
        public Row<T> GetByIndex<T>(object FirstIndexValue, bool Reverse = false) =>
            FirstIndexValue == null ? default : this.reader.Create<T>(reverse: Reverse, match: rh => rh.IndexValues[0].Equals(FirstIndexValue), readValue: true).FirstOrDefault();
        #endregion

        #region "GetById"
        /// <summary>retreives the item by Row-Id.</summary>
        public Row<T> Get<T>(int Id, bool Reverse = false) =>
            Id == 0 ? default : this.reader.Create<T>(reverse: Reverse, match: rh => rh.Id == Id, readValue: true).FirstOrDefault();

        /// <summary>retreives the item by RowHeader.</summary>
        public Row<T> Get<T>(RowHeader rowHeader, bool Reverse = false) =>
            rowHeader == null ? default : GetByPos<T>(rowHeader.Pos, Reverse);

        /// <summary>retreives the item by searching on indexValues.</summary>
        public Row<T> Get<T>(Predicate<object[]> match, bool Reverse = false) =>
            match == null ? default : this.reader.Create<T>(reverse: Reverse, match: rh => match(rh.IndexValues), readValue: true).FirstOrDefault();
        #endregion

        #region "GetByPos"
        /// <summary>retreives the item by the position of the item.</summary>
        /// <param name="Pos">is the position of the item.</param>
        public Row<T> GetByPos<T>(long Pos, bool Reverse = false) =>
            Pos == 0 ? default : this.reader.Create<T>(reverse: Reverse, StartPos: Pos, readValue: true).FirstOrDefault();
        #endregion


        #region "GetFirst"
        public Row<T> GetFirst<T>() => GetByPos<T>(FileHeader.Size);
        public Row<T> GetLast<T>() => GetByPos<T>(this.cw.fs_inx.Length - RowHeader.Size);
        #endregion
        #region "GetAll"
        /// <summary>Retrieves all the elements.</summary>
        public IEnumerable<Row<T>> GetAll<T>(bool Reverse = false) =>
            this.reader.Create<T>(reverse: Reverse, readValue: true);

        /// <summary>Retrieves all the elements by searching on indexValues.</summary>
        public IEnumerable<Row<T>> GetAll<T>(Predicate<object[]> match, bool Reverse = false) =>
            this.reader.Create<T>(reverse: Reverse, readValue: true, match: rh => match(rh.IndexValues));
        #endregion

        #region "GetHeader(s)"
        public IEnumerable<RowHeader> GetHeaders(bool Reverse = false) => from x in this.reader.Create<object>(reverse: Reverse) select x.Header;

        public RowHeader GetHeader(int Id) => GetHeaders().FirstOrDefault(row => row.Id == Id);
        public RowHeader GetHeaderByPos(long Pos) => GetHeaders().FirstOrDefault(row => row.Pos == Pos);
        public RowHeader GetHeaderByIndex(object FirstIndexValue) => FirstIndexValue == null ? null : GetHeaders().FirstOrDefault(row => row.IndexValues[0].Equals(FirstIndexValue));
        #endregion

        #region "Exists"
        public bool Exists(int Id) => GetHeader(Id) is object;
        public bool ExistsByPos(long Pos) => GetHeaderByPos(Pos) is object;
        public bool ExistsByKey(object FirstIndexValue) => GetHeaderByIndex(FirstIndexValue) is object;
        #endregion

        #endregion
        #endregion


        #region "private methods for readers/writers"
        internal void checkFileState(FileAccess Access, FileState state)
        {
            if (this.fh.State != state)
                throw new Exception("KVCollection [" + Access.ToString() + "] Error. Operation is aborted, because of collection's state is [" + this.fh.State.ToString() + "].");
        }
        internal bool isLastRow(RowHeader rh) => (rh.Pos + RowHeader.Size) == this.cw.fs_inx.Length;

        internal bool isPosCorrect(long pos) =>
            ((pos - FileHeader.Size) % RowHeader.Size) == 0;
        #endregion

        #region "private methods for writing"
        private void WriterBegin(Action fn, bool CheckState = true)
        {
            this.cw?.WriteBegin((Action)(() =>
            {
                if (CheckState) checkFileState(FileAccess.Write, FileState.Normal);
                fn?.Invoke();
                // file header is being updated...
                this.cw.Write(this.fh.Pos, this.fh.ToArray(), 0);
            }));
        }
        private RowHeader writer_insert(byte[] Value, IEnumerable<object> IndexValues)
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
                writer_set_row_header(rh);
                writer_set_row_value(rh, Value);
            }

            return rh;
        }
        private RowHeader writer_update(RowHeader rh, byte[] Value, IEnumerable<object> IndexValues)
        {
            if (rh == null) throw new Exception("Update error. RowHeader must be specifed on update.");

            // if there is a next record and also new value is longer than old value,
            // then delete old record and insert as new.
            if ((Value.Length > rh.ValueActualSize) && isLastRow(rh) == false)
            {
                writer_delete(rh.Pos);
                return writer_insert(Value, IndexValues);
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
                    writer_set_row_header(rh);
                    writer_set_row_value(rh, Value);
                }
                return rh;
            }
        }
        private void writer_delete(long pos)
        {
            if (isPosCorrect(pos) == false) return;

            var rh = new RowHeader();
            rh.Pos = pos;
            writer_set_row_header(rh);
            this.fh.Count--;
        }

        protected class writer_multi_items
        {
            public List<writer_multi_item> Items = new List<writer_multi_item>();

            public void Add(byte[] Value, IEnumerable<object> IndexValues) =>
                Items.Add(new writer_multi_item() { IsAdd = true, Value = Value, IndexValues = IndexValues });

            public void Update(Predicate<RowHeader> match, byte[] Value, IEnumerable<object> IndexValues) =>
                Items.Add(new writer_multi_item() { IsUpdate = true, match = match, Value = Value, IndexValues = IndexValues });

            public void Upsert(Predicate<RowHeader> match, byte[] Value, IEnumerable<object> IndexValues) =>
                Items.Add(new writer_multi_item() { IsUpsert = true, match = match, Value = Value, IndexValues = IndexValues });

            public void Delete(Predicate<RowHeader> match) =>
                Items.Add(new writer_multi_item() { IsDelete = true, match = match });

            public bool HasExecutable => Items.Exists(x => x.IsExec == false);
        }
        protected class writer_multi_item
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
        protected RowHeader writer_multi(Action<writer_multi_items> fn)
        {
            RowHeader retval = null;
            var mc = new writer_multi_items();
            fn(mc);
            if (mc.Items.Count == 0) return retval;

            // operates first [ADD] items.
            foreach (var item in mc.Items)
                if (item.IsAdd)
                {
                    WriterBegin(() => retval = writer_insert(item.Value, item.IndexValues));
                    item.IsExec = true;
                }
            if (mc.HasExecutable == false) return retval;

            // if there are items other than [ADD], then operates [UPDATE / UPSERT / DELETE] items.
            foreach (var row in this.reader.Create<object>(reverse: false, FileHeader.Size, null, false))  // iterate all rows
                foreach (var item in mc.Items)
                    if (item.IsExec == false && item.match(row.Header))            // check the row
                    {
                        if (item.IsUpdate || item.IsUpsert)
                            WriterBegin(() => retval = writer_update(row.Header, item.Value, item.IndexValues));
                        else if (item.IsDelete)
                            WriterBegin(() => writer_delete(row.Header.Pos));
                        //WriteBegin(() => delete(row.Key));

                        item.IsExec = true;
                        if (mc.HasExecutable == false) break;
                    }
            if (mc.HasExecutable == false) return retval;

            // operates last [UPSERT] items not found.
            foreach (var item in mc.Items)
                if (item.IsExec == false && item.IsUpsert)
                    WriterBegin(() => retval = writer_insert(item.Value, item.IndexValues));


            return retval;
        }

        internal void writer_set_row_header(RowHeader rh) =>
            this.cw.Write(rh.Pos, rh.ToArray(), 0);

        internal void writer_set_row_value(RowHeader rh, byte[] data) =>
            this.cw.Write(rh.ValuePos, data, 1);
        #endregion
    }
}