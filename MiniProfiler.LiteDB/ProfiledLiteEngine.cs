using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using LiteDB;
using LiteDB.Engine;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;

namespace MiniProfiler.LiteDB
{
    public class ProfiledLiteEngine : ILiteEngine
    {
        private readonly ILiteEngine _internalEngine;
        private readonly StackExchange.Profiling.MiniProfiler _profiler;
        private const string LiteDb = "litedb";

        public ProfiledLiteEngine(ILiteEngine liteEngineImplementation, StackExchange.Profiling.MiniProfiler profiler)
        {
            _internalEngine = liteEngineImplementation
                              ?? throw new ArgumentNullException(nameof(liteEngineImplementation));

            _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
        }

        public void Dispose() => _internalEngine.Dispose();

        public int Checkpoint() => _internalEngine.Checkpoint();

        public long Rebuild(RebuildOptions options) => _internalEngine.Rebuild(options);

        public bool BeginTrans() => _internalEngine.BeginTrans();

        public bool Commit() => _internalEngine.Commit();

        public bool Rollback() => _internalEngine.Rollback();

        public IBsonDataReader Query(string collection, Query query)
        {
            using (var _ = _profiler.CustomTiming(LiteDb, query.ToSQL(collection), nameof(Query)))
            {
                return _internalEngine.Query(collection, query);
            }
        }

        public int Insert(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId)
        {
            docs = docs.ToList();

            using (var _ = _profiler.CustomTiming(LiteDb, ToJson(docs), nameof(Insert)))
            {
                return _internalEngine.Insert(collection, docs, autoId);
            }
        }

        public int Update(string collection, IEnumerable<BsonDocument> docs)
        {
            docs = docs.ToList();

            using (var _ = _profiler.CustomTiming(LiteDb, ToJson(docs), nameof(Update)))
            {
                return _internalEngine.Update(collection, docs);
            }
        }

        public int UpdateMany(string collection, BsonExpression transform, BsonExpression predicate)
        {
            using (var _ = _profiler.CustomTiming(LiteDb, transform.ToString() + "WHERE" + predicate.ToString(), nameof(UpdateMany)))
            {
                return _internalEngine.UpdateMany(collection, transform, predicate);
            }
        }

        public int Upsert(string collection, IEnumerable<BsonDocument> docs, BsonAutoId autoId)
        {
            docs = docs.ToList();
            using (var _ = _profiler.CustomTiming(LiteDb, ToJson(docs), nameof(Upsert)))
            {
                return _internalEngine.Upsert(collection, docs, autoId);
            }
        }

        public int Delete(string collection, IEnumerable<BsonValue> ids)
        {
            ids = ids.ToList();
            using (var _ = _profiler.CustomTiming(LiteDb, ToJson(ids.Select(i => i.AsDocument)), nameof(Delete)))
            {
                return _internalEngine.Delete(collection, ids);
            }
        }

        public int DeleteMany(string collection, BsonExpression predicate)
        {
            using (var _ = _profiler.CustomTiming(LiteDb, predicate.ToString(), nameof(DeleteMany)))
            {
                return _internalEngine.DeleteMany(collection, predicate);
            }
        }

        public bool DropCollection(string name)
        {
            using (var _ = _profiler.CustomTiming(LiteDb, name, nameof(DropCollection)))
            {
                return _internalEngine.DropCollection(name);
            }
        }

        public bool RenameCollection(string name, string newName)
        {
            using (var _ = _profiler.CustomTiming(LiteDb, $"{name} -> {newName}", nameof(RenameCollection)))
            {
                return _internalEngine.RenameCollection(name, newName);
            }
        }

        public bool EnsureIndex(
            string collection,
            string name,
            BsonExpression expression,
            bool unique)
        {
            return _internalEngine.EnsureIndex(collection, name, expression, unique);
        }

        public bool DropIndex(string collection, string name)
        {
            return _internalEngine.DropIndex(collection, name);
        }

        public BsonValue Pragma(string name)
        {
            return _internalEngine.Pragma(name);
        }

        public bool Pragma(string name, BsonValue value)
        {
            return _internalEngine.Pragma(name, value);
        }

        private static string ToJson(IEnumerable<BsonDocument> docs)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                var jsonWriter = new JsonWriter(writer) { Pretty = true, Indent = 2 };
                BsonValue bsonValue = new BsonArray(docs);
                jsonWriter.Serialize(bsonValue);
            }

            var body = sb.ToString();
            return body;
        }
    }
}
