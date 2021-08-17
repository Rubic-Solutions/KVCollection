using System;
using System.Collections.Generic;
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
        public void Add(T Value) => base.Add( Serializer.GetBytes(Value), inx_info.CreateValues(Value));

        /// <summary>Updates the value of the [Key]. If [Key] does not exist, nothing updated.</summary>
        public void Update(int Id, T Value) => base.Update(Id, Serializer.GetBytes(Value), inx_info.CreateValues(Value));

        /// <summary>
        /// Sets the value of the [Key]. If [Key] does not exist, a new [Key] is created. If [Key] already exists in the collection, it is overwritten.
        /// <para>
        /// PrimaryKey value cannot be changed. Only can be changed other Key-Values . To clear Key-Value then set EMPTY value. To keep as is value of KEY, then leave NULL.
        /// </para>
        /// </summary>
        public void Upsert(int Id, T Value) => base.Upsert(Id, Serializer.GetBytes(Value), inx_info.CreateValues(Value));

        public new KeyValuePair<RowHeader, T> GetFirst()
        {
            var row = base.GetFirst();
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }
        public new KeyValuePair<RowHeader, T> GetLast()
        {
            var row = base.GetLast();
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }

        public new KeyValuePair<RowHeader, T> GetValue(long Pos)
        {
            var row = base.GetByPos(Pos);
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }
        public new KeyValuePair<RowHeader, T> GetValue(int Id)
        {
            var row = base.Get(Id);
            return KeyValuePair.Create(row.Key, Serializer.GetObject<T>(row.Value));
        }

        public new IEnumerable<KeyValuePair<RowHeader, T>> All() => base.All<T>();

        #endregion
    }
}