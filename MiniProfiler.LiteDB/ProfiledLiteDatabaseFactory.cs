using System;
using System.IO;
using LiteDB;
using LiteDB.Engine;

namespace MiniProfiler.LiteDB
{
    public static class ProfiledLiteDatabaseFactory
    {
        /// <summary>
        /// Starts Profiled LiteDB database using a connection string for file system database
        /// </summary>
        public static LiteDatabase New(
            string connectionString,
            StackExchange.Profiling.MiniProfiler profiler,
            BsonMapper mapper = null)
        {
            return New(new ConnectionString(connectionString), profiler, mapper);
        }

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public static LiteDatabase New(
            ConnectionString connectionString,
            StackExchange.Profiling.MiniProfiler profiler,
            BsonMapper mapper = null)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));
            if (connectionString.Upgrade)
                LiteEngine.Upgrade(connectionString.Filename, connectionString.Password, connectionString.Collation);
            var engine = connectionString.CreateProfiledEngine(profiler);
            return new LiteDatabase(engine, mapper);
        }

        /// <summary>
        /// Starts LiteDB database with profiled engine using a generic Stream implementation (mostly MemoryStream).
        /// </summary>
        /// <param name="stream">DataStream reference </param>
        /// <param name="profiler">MiniProfiler instance</param>
        /// <param name="mapper">BsonMapper mapper reference</param>
        /// <param name="logStream">LogStream reference </param>
        public static LiteDatabase New(
            Stream stream,
            StackExchange.Profiling.MiniProfiler profiler,
            BsonMapper mapper = null,
            Stream logStream = null)
        {
            var settings = new EngineSettings
            {
                DataStream = stream ?? throw new ArgumentNullException(nameof(stream)),
                LogStream = logStream
            };
            return new LiteDatabase(new ProfiledLiteEngine(new LiteEngine(settings), profiler), mapper);
        }

        /// <summary>
        /// Start LiteDB database using a pre-exiting engine. The engine will be automatically wrapped into profiling wrapper.
        /// When LiteDatabase instance dispose engine instance will be disposed too
        /// </summary>
        public static LiteDatabase New(
            ILiteEngine engine,
            StackExchange.Profiling.MiniProfiler profiler,
            BsonMapper mapper = null,
            bool disposeOnClose = true)
        {
            return new LiteDatabase(
                new ProfiledLiteEngine(engine, profiler),
                mapper,
                disposeOnClose
            );
        }
    }
}
