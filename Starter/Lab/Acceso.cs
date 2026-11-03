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
        // TODO: Con ADO.NET: SqlCommand/SqliteCommand con un parámetro $c, ordenado por Id, mapeando a mano cada fila a ClienteDto.
        throw new NotImplementedException();
    }

    public static List<ClienteDto> ClientesPorCiudadDapper(SqliteConnection cn, string ciudad)
    {
        // TODO: Con Dapper: cn.Query<ClienteDto>(sql, parametros). El SQL es tuyo; Dapper solo mapea.
        throw new NotImplementedException();
    }

    public static List<ClienteDto> ClientesPorCiudadEf(TiendaContext db, string ciudad)
    {
        // TODO: Con EF Core: LINQ sobre db.Customers, sin seguimiento (AsNoTracking), ordenado por Id y proyectado a ClienteDto con Select.
        throw new NotImplementedException();
    }
}

/// <summary>Servicio de escritorio / segundo plano: no puede tener un DbContext vivo para siempre.</summary>
public class ClienteServicio
{
    readonly Func<TiendaContext> _factory;
    public ClienteServicio(Func<TiendaContext> factory) => _factory = factory;

    public int Crear(string nombre, string ciudad)
    {
        // TODO: Crea un contexto NUEVO con _factory() dentro de un using, añade el cliente, guarda y devuelve su Id.
        throw new NotImplementedException();
    }

    public int Contar()
    {
        // TODO: Otro contexto nuevo (uno por operación): devuelve cuántos clientes hay.
        throw new NotImplementedException();
    }
}
