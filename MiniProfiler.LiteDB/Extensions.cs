using System;
using LiteDB;
using LiteDB.Engine;

namespace MiniProfiler.LiteDB
{
    internal static class Extensions
    {
        public static ProfiledLiteEngine CreateProfiledEngine(
            this ConnectionString connectionString,
            StackExchange.Profiling.MiniProfiler profiler)
        {
            EngineSettings settings = new EngineSettings()
            {
                Filename = connectionString.Filename,
                Password = connectionString.Password,
                InitialSize = connectionString.InitialSize,
                ReadOnly = connectionString.ReadOnly,
                Collation = connectionString.Collation
            };
            if (connectionString.Connection == ConnectionType.Direct)
                return new ProfiledLiteEngine(new LiteEngine(settings), profiler);
            if (connectionString.Connection == ConnectionType.Shared)
                return new ProfiledLiteEngine(new SharedEngine(settings), profiler);
            throw new NotImplementedException();
        }
    }
}
