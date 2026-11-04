using Microsoft.EntityFrameworkCore;

namespace Tienda;

/// <summary>Método .NET que EF Core traduce a una función SQL (aquí, la función "iva" registrada en SQLite).</summary>
public static class Funciones
{
    public static decimal Iva(decimal precio) => throw new NotSupportedException("Solo se puede usar dentro de una consulta LINQ.");
}

public partial class TiendaContext
{
    partial void ConfigureExtra(ModelBuilder b)
    {
        // TODO: Mapea Funciones.Iva a la función SQL "iva": b.HasDbFunction(typeof(Funciones).GetMethod(nameof(Funciones.Iva))!).HasName("iva");

    }
}
