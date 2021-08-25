using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace KeyValue
{
    public class CollectionIndexer
    {
        private static ConcurrentDictionary<Type, IndexerInfo> index_defs;

        static CollectionIndexer() =>
            index_defs = new ConcurrentDictionary<Type, IndexerInfo>();

        public static Indexer<T> Define<T>() =>
            new Indexer<T>(Get<T>());

        internal static IndexerInfo Get<T>() =>
            index_defs.GetOrAdd(typeof(T), new IndexerInfo());
    }

    public class IndexerInfo
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal List<Func<object, object>> index_value_getter_fns = new List<Func<object, object>>();

        public IEnumerable<object> CreateValues<T>(T obj) =>
            from fn in index_value_getter_fns select fn(obj);
    }
    public class Indexer<T>
    {
        private readonly IndexerInfo indexerInfo;
        public Indexer(IndexerInfo indexerInfo) =>
            this.indexerInfo = indexerInfo;

        public Indexer<T> EnsureIndex(Expression<Func<T, string>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, bool>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, Int16>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, UInt16>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, Int32>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, UInt32>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, Int64>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, UInt64>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, Single>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, double>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, decimal>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, DateTime>> fn) => EnsureIndex(fn);
        public Indexer<T> EnsureIndex(Expression<Func<T, byte[]>> fn) => EnsureIndex(fn);

        private Indexer<T> EnsureIndex<K>(Expression<Func<T, K>> fn)
        {
            //if (fn.NodeType != ExpressionType.Lambda)
            //    throw new Exception("Only one [field] or [property], is allowed to be specified.");

            //var exp = fn.Body;

            //if (exp.NodeType != ExpressionType.MemberAccess)
            //    throw new Exception("Only one [field] or [property], is allowed to be specified.");

            //var member = ((MemberExpression)exp).Member;

            //if (member.DeclaringType != typeof(T))
            //    throw new Exception("Only [fields] or [properties] in type of <T>, is allowed to be specified.");

            var fnc = fn.Compile();
            this.indexerInfo.index_value_getter_fns.Add((o) => fnc((T)o) ?? throw new Exception(nameof(EnsureIndex) + " return value cannot be NULL."));

            return this;
        }
    }
}