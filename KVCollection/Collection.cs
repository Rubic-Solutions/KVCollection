using System;
using System.Collections.Generic;
namespace KeyValue
{
    public class Collection<T> : CollectionBase
    {
        #region "Public Operation Methods"
        /// <summary>Add an item into the collection.</summary>
        public void Add(string PrimaryKey, T Value) => base.Add(PrimaryKey, Serializer.ToBytes(Value));

        /// <summary>Updates the value of the [Key]. If [Key] does not exist, nothing updated.</summary>
        public void Update(string PrimaryKey, T Value) => base.Update(PrimaryKey, Serializer.ToBytes(Value));

        /// <summary>
        /// Sets the value of the [Key]. If [Key] does not exist, a new [Key] is created. If [Key] already exists in the collection, it is overwritten.
        /// <para>
        /// PrimaryKey value cannot be changed. Only can be changed other Key-Values . To clear Key-Value then set EMPTY value. To keep as is value of KEY, then leave NULL.
        /// </para>
        /// </summary>
        public void Upsert(string PrimaryKey, T Value) => base.Upsert(PrimaryKey, Serializer.ToBytes(Value));

        public new KeyValuePair<RowHeader, T> GetFirst()
        {
            var row = base.GetFirst();
            return KeyValuePair.Create(row.Key, Serializer.FromBytes<T>(row.Value));
        }
        public new KeyValuePair<RowHeader, T> GetLast()
        {
            var row = base.GetLast();
            return KeyValuePair.Create(row.Key, Serializer.FromBytes<T>(row.Value));
        }

        public new T GetValue(string PrimaryKey) => Serializer.FromBytes<T>(base.GetValue(PrimaryKey));

        public new IEnumerable<KeyValuePair<RowHeader, T>> All() => base.All<T>();

        #endregion
    }
}