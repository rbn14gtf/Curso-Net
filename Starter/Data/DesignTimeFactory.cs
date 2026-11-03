using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tienda;

/// <summary>Permite que `dotnet ef migrations add ...` cree el contexto sin una aplicación web.</summary>
public class TiendaContextFactory : IDesignTimeDbContextFactory<TiendaContext>
{
    public TiendaContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<TiendaContext>().UseSqlite("Data Source=tienda.db").Options);
}
