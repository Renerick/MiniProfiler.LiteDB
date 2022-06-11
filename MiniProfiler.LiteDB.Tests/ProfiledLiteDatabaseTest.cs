using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using LiteDB;
using Xunit;

namespace MiniProfiler.LiteDB.Tests;

public class ProfiledLiteDatabaseTest
{
    class TestCollection
    {
        public string Test { get; set; } = string.Empty;
    }

    private readonly StackExchange.Profiling.MiniProfiler _profiler;
    private readonly ILiteDatabase _database;

    public ProfiledLiteDatabaseTest()
    {
        _profiler = StackExchange.Profiling.MiniProfiler.StartNew();
        _database = ProfiledLiteDatabaseFactory.New(new MemoryStream(1024), _profiler);
    }

    [Fact]
    public void ProfiledDatabase_CreatesCustomTimings()
    {
        _database.GetCollection<TestCollection>().Insert(new TestCollection() { Test = "Test" });
        _database.GetCollection<TestCollection>().Query().ToList();

        _profiler.Root.CustomTimings.Should().HaveCount(1);
        _profiler.Root.CustomTimings.First().Key.Should().Be("litedb");
        _profiler.Root.CustomTimings.First().Value.Should().HaveCount(2);
    }

    [Fact]
    public void ProfiledDatabase_Insert_AddsCorrectTimings()
    {
        _database.GetCollection<TestCollection>().Insert(new TestCollection() { Test = "Test" });

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("Insert");
    }

    [Fact]
    public void ProfiledDatabase_Query_AddsCorrectTimings()
    {
        _database.GetCollection<TestCollection>().Query().ToList();

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("Query");
    }

    [Fact]
    public void ProfiledDatabase_Update_AddsCorrectTimings()
    {
        _database.GetCollection<TestCollection>().Update(new TestCollection());

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("Update");
    }

    [Fact]
    public void ProfiledDatabase_UpdateMany_AddsCorrectTimings()
    {
        _database.GetCollection<TestCollection>().UpdateMany(collection => new TestCollection() {Test = "Value"}, collection => true);

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("UpdateMany");
    }

    [Fact]
    public void ProfiledDatabase_Upsert_AddsCorrectTimings()
    {
        _database.GetCollection<TestCollection>().Upsert(new TestCollection());

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("Upsert");
    }

    [Fact]
    public void ProfiledDatabase_Delete_AddsCorrectTimings()
    {
        _database.GetCollection<TestCollection>().Delete(new ObjectId());

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("Delete");
    }

    [Fact]
    public void ProfiledDatabase_DeleteMany_AddsCorrectTimings()
    {
        _database.GetCollection<TestCollection>().DeleteMany(x => true);

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("DeleteMany");
    }

    [Fact]
    public void ProfiledDatabase_RenameCollection_AddsCorrectTimings()
    {
        _database.RenameCollection("Test", "Test1");

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("RenameCollection");
    }

    [Fact]
    public void ProfiledDatabase_DropCollection_AddsCorrectTimings()
    {
        _database.DropCollection("Test");

        var timing = _profiler.Root.CustomTimings.First().Value.First();

        timing.ExecuteType.Should().Be("DropCollection");
    }
}
