using System;
using System.Collections.Generic;
using System.Linq;

namespace KeyValue
{
    public class Collection<T> : CollectionBase
    {
        private IndexerInfo inx_info;
        public Collection()
        {
            inx_info = CollectionIndexer.Get<T>();
        }
        #region "Public Operation Methods"
        /// <summary>Add an item into the collection.</summary>
        public RowHeader Add(T Value) => base.Add(Serializer.GetBytes(Value), inx_info.CreateValues(Value));

        /// <summary>Updates the item by the [Row-ID]. If the item does not exist, exception occured.</summary>
        public RowHeader Update(int Id, T Value) => base.Update(Id, Serializer.GetBytes(Value), inx_info.CreateValues(Value));
        /// <summary>Updates the item by the [first index value]. If the item does not exist, exception occured.</summary>
        public RowHeader Update(T Value) => base.Update(Serializer.GetBytes(Value), inx_info.CreateValues(Value));
        /// <summary>Updates the item by the [Row-Header]. If the item does not exist, exception occured.</summary>
        public RowHeader Update(RowHeader rowHeader, T Value) => base.Update(rowHeader, Serializer.GetBytes(Value), inx_info.CreateValues(Value));

        /// <summary>Sets the item by [Row-ID]. If [Row-ID] does not exist, a new item is created. If [Row-ID] already exists in the collection, it is overwritten.</summary>
        public RowHeader Upsert(int Id, T Value) => base.Upsert(Id, Serializer.GetBytes(Value), inx_info.CreateValues(Value));
        /// <summary>Sets the item by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public RowHeader Upsert(T Value) => base.Upsert(Serializer.GetBytes(Value), inx_info.CreateValues(Value));
        /// <summary>Sets the item by [Row-Header]. If [Row-Header] does not exist, a new item is created. If [Row-Header] already exists in the collection, it is overwritten.</summary>
        public RowHeader Upsert(RowHeader rowHeader, T Value) => base.Upsert(rowHeader, Serializer.GetBytes(Value), inx_info.CreateValues(Value));

        /// <summary>Deletes the item by [first index value]. If [first index value] does not exist, a new item is created. If [first index value] already exists in the collection, it is overwritten.</summary>
        public void Delete(T Value) => base.Delete(inx_info.CreateValues(Value).FirstOrDefault());

        public new KeyValuePair<RowHeader, T> GetFirst()
        {
            var row = base.GetFirst();
            if (row.Key == null) return default;
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }
        public new KeyValuePair<RowHeader, T> GetLast()
        {
            var row = base.GetLast();
            if (row.Key == null) return default;
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }

        /// <summary>retreives the item by the position of the item.</summary>
        /// <param name="Pos">is the position of the item.</param>
        public new KeyValuePair<RowHeader, T> GetByPos(long Pos)
        {
            var row = base.GetByPos(Pos);
            if (row.Key == null) return default;
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }
        /// <summary>retreives the item by the first IndexValue.</summary>
        /// <param name="FirstIndexValue">is the first IndexValue to be searched.</param>
        public new KeyValuePair<RowHeader, T> GetByKey(object FirstIndexValue)
        {
            var row = base.GetByKey(FirstIndexValue);
            if (row.Key == null) return default;
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }
        public new KeyValuePair<RowHeader, T> Get(int Id)
        {
            var row = base.Get(Id);
            if (row.Key == null) return default;
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }
        public new KeyValuePair<RowHeader, T> Get(RowHeader rowHeader)
        {
            var row = base.Get(rowHeader);
            if (row.Key == null) return default;
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }

        /// <summary>Retrieves all the elements. </summary>
        public new IEnumerable<KeyValuePair<RowHeader, T>> All() => base.All<T>();

        /// <summary>Retrieves all the elements by searching on indexValues.</summary>
        public new IEnumerable<KeyValuePair<RowHeader, T>> FindAll(Predicate<object[]> match) => base.FindAll<T>(match);
        #endregion
    }
}