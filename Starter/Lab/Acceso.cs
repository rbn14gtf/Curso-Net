using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tienda;

public record ClienteDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
}

/// <summary>La misma consulta ("clientes de una ciudad") con tres tecnologías de acceso a datos.</summary>
public static class Acceso
{
    public static List<ClienteDto> ClientesPorCiudadAdo(SqliteConnection cn, string ciudad)
    {
        var lista = new List<ClienteDto>();
        using var cmd = cn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name FROM Customers WHERE City = $c ORDER BY Id";
        cmd.Parameters.AddWithValue("$c", ciudad);
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
            lista.Add(new ClienteDto { Id = rd.GetInt32(0), Name = rd.GetString(1) });
        return lista;
    }

    public static List<ClienteDto> ClientesPorCiudadDapper(SqliteConnection cn, string ciudad)
    {
        return cn.Query<ClienteDto>(
            "SELECT Id, Name FROM Customers WHERE City = @ciudad ORDER BY Id", new { ciudad }).ToList();
    }

    public static List<ClienteDto> ClientesPorCiudadEf(TiendaContext db, string ciudad)
    {
        return db.Customers.AsNoTracking()
            .Where(c => c.City == ciudad)
            .OrderBy(c => c.Id)
            .Select(c => new ClienteDto { Id = c.Id, Name = c.Name })
            .ToList();
    }
}

/// <summary>Servicio de escritorio / segundo plano: no puede tener un DbContext vivo para siempre.</summary>
public class ClienteServicio
{
    readonly Func<TiendaContext> _factory;
    public ClienteServicio(Func<TiendaContext> factory) => _factory = factory;

    public int Crear(string nombre, string ciudad)
    {
        using var db = _factory();
        var c = new Customer { Name = nombre, City = ciudad };
        db.Customers.Add(c);
        db.SaveChanges();
        return c.Id;
    }

    public int Contar()
    {
        using var db = _factory();
        return db.Customers.Count();
    }
}
