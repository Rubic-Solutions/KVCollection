using System;
using System.Collections.Generic;
namespace KeyValue
{
    public class Collection<T> : CollectionBase
    {
        #region "Public Operation Methods"
        /// <summary>Add an item into the collection.</summary>
        public void Add(string PrimaryKey, T Value) => base.Add<T>(PrimaryKey, Value);

        /// <summary>Updates the value of the [Key]. If [Key] does not exist, nothing updated.</summary>
        public void Update(string PrimaryKey, T Value) => base.Update<T>(PrimaryKey, Value);

        /// <summary>
        /// Sets the value of the [Key]. If [Key] does not exist, a new [Key] is created. If [Key] already exists in the collection, it is overwritten.
        /// <para>
        /// PrimaryKey value cannot be changed. Only can be changed other Key-Values . To clear Key-Value then set EMPTY value. To keep as is value of KEY, then leave NULL.
        /// </para>
        /// </summary>
        public void Upsert(string PrimaryKey, T Value) => base.Upsert<T>(PrimaryKey, Value);

        public new T GetValue(string PrimaryKey) => base.GetValue<T>(PrimaryKey);
        public new T GetValue(string PrimaryKey, out RowHeader Header) => base.GetValue<T>(PrimaryKey, out Header);

        public IEnumerable<KeyValuePair<string, T>> All() => base.All<T>();

        /// <summary>Finds all records by filtering Indexed key-values.</summary>
        public IEnumerable<KeyValuePair<string, T>> FindAll(Func<List<object>, bool> match) => base.FindAll<T>(match);
        #endregion

    }
}