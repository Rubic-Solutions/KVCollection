using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace KeyValue
{
    public class CollectionIndexer
    {
        private static ConcurrentDictionary<string, IndexerInfo> index_defs;

        static CollectionIndexer() =>
            index_defs = new ConcurrentDictionary<string, IndexerInfo>();

        public static Indexer<T> Define<T>(string collectionName) =>
            new Indexer<T>(Get(collectionName));

        internal static IndexerInfo Get(string collectionName) =>
            index_defs.GetOrAdd(collectionName, new IndexerInfo());

        internal static Dictionary<Type, Func<JsonElement, object>> index_json2object_fns = new Dictionary<Type, Func<JsonElement, object>>()
        {
            {typeof(bool), (e) => e.GetBoolean() },
            {typeof(byte),(e) => e.GetByte()},
            {typeof(byte[]),(e) => e.GetBytesFromBase64()},
            {typeof(DateTime),(e) => e.GetDateTime()},
            {typeof(DateTimeOffset),(e) => e.GetDateTimeOffset()},
            {typeof(decimal),(e) => e.GetDecimal()},
            {typeof(double),(e) => e.GetDouble()},
            {typeof(Guid),(e) => e.GetGuid()},
            {typeof(short),(e) => e.GetInt16()},
            {typeof(int),(e) => e.GetInt32()},
            {typeof(long),(e) => e.GetInt64()},
            {typeof(sbyte),(e) => e.GetSByte()},
            {typeof(float),(e) => e.GetSingle()},
            {typeof(ushort),(e) => e.GetUInt16()},
            {typeof(uint),(e) => e.GetUInt32()},
            {typeof(ulong),(e) => e.GetUInt64()},
            {typeof(string),(e) => e.GetString()}
        };
    }

    public class IndexerInfo
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal List<Func<object, object>> index_value_getter_fns = new List<Func<object, object>>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal List<Func<JsonElement, object>> index_json2object_fns = new List<Func<JsonElement, object>>();

        public void Clear()
        {
            index_value_getter_fns.Clear();
            index_json2object_fns.Clear();
        }

        public List<object> CreateValues<T>(string PrimaryKey, T obj)
        {
            var retval = new List<object>();
            retval.Add(PrimaryKey);
            retval.AddRange(from fn in index_value_getter_fns select fn(obj));
            return retval;
        }
        public byte[] SerializeIndexValues(List<object> values) => Serializer.ToBytes(values);
        public List<object> DeserializeIndexValues(byte[] data) => DeserializeIndexValues(data, 0, data.Length);
        public List<object> DeserializeIndexValues(byte[] data, int index, int count)
        {
            var retval = new List<object>();
            var elems = Serializer.FromBytes<JsonElement[]>(data, index, count);

            retval.Add(elems[0].ToString());
            if (index_json2object_fns.Count == 0)
            {
                for (int i = 1; i < elems.Length; i++)
                    retval.Add(elems[i].ToString());
            }
            else
            {
                for (int i = 1; i < elems.Length; i++)
                    retval.Add(index_json2object_fns[i - 1](elems[i]));
            }
            return retval;
        }
        //public List<object> Deserialize(byte[] data, int index, int count)
        //{
        //    var retval = new List<object>();
        //    var elems = Serializer.FromBytes<JsonElement[]>(data, index, count);

        //    retval.Add(elems[0].ToString());
        //    for (int i = 0; i < index_json2object_fns.Count; i++)
        //        retval.Add(index_json2object_fns[i](elems[i + 1]));
        //    return retval;
        //}

    }
    public class Indexer<T>
    {
        private readonly IndexerInfo indexerInfo;
        public Indexer(IndexerInfo indexerInfo) =>
            this.indexerInfo = indexerInfo;

        public Indexer<T> Clear()
        {
            this.indexerInfo.Clear();
            return this;
        }

        public Indexer<T> EnsureIndex<K>(Expression<Func<T, K>> fn)
        {
            if (fn.NodeType != ExpressionType.Lambda)
                throw new Exception("Only one [field] or [property], is allowed to be specified.");

            var exp = fn.Body;

            if (exp.NodeType != ExpressionType.MemberAccess)
                throw new Exception("Only one [field] or [property], is allowed to be specified.");

            var member = ((MemberExpression)exp).Member;

            if (member.DeclaringType != typeof(T))
                throw new Exception("Only [fields] or [properties] in type of <T>, is allowed to be specified.");

            if (typeof(K).IsValueType == false &&
                typeof(K) != typeof(string))
                throw new Exception("Only primitive types (such as int16/32/64, double, single, decimal, string, datetype), is allowed to be specified.");

            //if (index.NodeType != ExpressionType.MemberAccess)
            //    throw new Exception("Only serializable [field] or [property] with output, is allowed to be specified.");
            var fnc = fn.Compile();
            this.indexerInfo.index_value_getter_fns.Add((o) => fnc((T)o));

            //this.indexerInfo.index_json2object_fns.Add(this.indexerInfo.json_to_object_fn<K>());
            this.indexerInfo.index_json2object_fns.Add(CollectionIndexer.index_json2object_fns[typeof(K)]);

            return this;
        }
    }
}