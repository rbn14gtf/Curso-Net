using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Tienda.Tests;

/// <summary>Registra el SQL que ejecuta EF Core (ignora PRAGMA).</summary>
public class CommandLog : DbCommandInterceptor
{
    readonly List<string> _sql = new();
    public IReadOnlyList<string> Sql => _sql;
    public int Count => _sql.Count;
    public void Clear() => _sql.Clear();

    void Add(DbCommand c) { if (!c.CommandText.StartsWith("PRAGMA", StringComparison.OrdinalIgnoreCase)) _sql.Add(c.CommandText); }
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r) { Add(c); return r; }
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r, CancellationToken ct = default) { Add(c); return new(r); }
    public override InterceptionResult<int> NonQueryExecuting(DbCommand c, CommandEventData e, InterceptionResult<int> r) { Add(c); return r; }
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<int> r, CancellationToken ct = default) { Add(c); return new(r); }
    public override InterceptionResult<object> ScalarExecuting(DbCommand c, CommandEventData e, InterceptionResult<object> r) { Add(c); return r; }
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<object> r, CancellationToken ct = default) { Add(c); return new(r); }
}
