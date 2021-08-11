using System.Collections.Generic;
using System.Diagnostics;

namespace KeyValue
{
    internal class CollectionCache
    {
        internal FileHeader FileHeader;
        internal int InstanceCount = 0;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Dictionary<long, RowHeader> row_headers_by_pos = null;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Dictionary<string, RowHeader> row_headers_by_key = null;

        internal CollectionCache()
        {
            row_headers_by_pos = new Dictionary<long, RowHeader>(10000);
            row_headers_by_key = new Dictionary<string, RowHeader>(10000);
        }
        internal void Clear()
        {
            FileHeader = null;
            //file_header = null;
            row_headers_by_pos.Clear();
            row_headers_by_pos.EnsureCapacity(10000);
            row_headers_by_key.Clear();
            row_headers_by_key.EnsureCapacity(10000);
        }


        internal RowHeader Add(RowHeader value)
        {
            this.row_headers_by_pos.Add(value.Pos, value);
            this.row_headers_by_key.Add(value.PrimaryKey, value);
            return value;
        }
        internal RowHeader Update(RowHeader value)
        {
            this.row_headers_by_pos[value.Pos] = value;
            this.row_headers_by_key[value.PrimaryKey] = value;
            return value;
        }

        internal RowHeader AddOrUpdate(RowHeader value)
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

        internal void Remove(RowHeader rowHeader)
        {
            this.row_headers_by_pos.Remove(rowHeader.Pos);
            this.row_headers_by_key.Remove(rowHeader.PrimaryKey);
        }

        internal IEnumerable<string> Keys() =>
            this.row_headers_by_key.Keys;

        internal Dictionary<string, RowHeader> Items() =>
            this.row_headers_by_key;

        internal bool Exists(string key) =>
            this.row_headers_by_key.ContainsKey(key);

        internal RowHeader TryGet(string key) =>
            this.row_headers_by_key.TryGetValue(key, out RowHeader retval) ? retval : default;

        internal RowHeader TryGet(long position) =>
            this.row_headers_by_pos.TryGetValue(position, out RowHeader retval) ? retval : default;

    }
}