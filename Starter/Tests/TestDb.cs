using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Tienda.Tests;

/// <summary>Base de datos SQLite en memoria con datos de ejemplo. Cada Nueva() devuelve un contexto distinto sobre la misma base.</summary>
public sealed class TestDb : IDisposable
{
    readonly SqliteConnection _cn = new("Data Source=:memory:");
    readonly IInterceptor[] _extra;
    public CommandLog Log { get; } = new();
    public SqliteConnection Connection => _cn;

    public TestDb(bool seed = true, params IInterceptor[] extra)
    {
        _extra = extra;
        _cn.Open();
        using var db = Nueva();
        db.Database.EnsureCreated();
        if (seed) Seed.Cargar(db);
        Log.Clear();
    }

    public TiendaContext Nueva()
    {
        var o = new DbContextOptionsBuilder<TiendaContext>().UseSqlite(_cn)
            .AddInterceptors(Log).AddInterceptors(_extra);
        return new TiendaContext(o.Options);
    }

    public void Dispose() => _cn.Dispose();
}
