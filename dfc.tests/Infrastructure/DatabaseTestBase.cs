using System;
using Dfc.Data.LocalDatabase;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Dfc.Tests.Infrastructure;

/// <summary>
/// Base class for integration tests that require a database.
/// Creates an in-memory SQLite database that is isolated for each test.
/// </summary>
public abstract class DatabaseTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly DfcDbContext Context;

    protected DatabaseTestBase()
    {
        // Create and open an in-memory SQLite connection
        // The database exists only as long as the connection is open
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Configure DbContext to use the in-memory database
        var options = new DbContextOptionsBuilder<DfcDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new DfcDbContext(options);

        // Create the database schema
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
