using Microsoft.EntityFrameworkCore;

namespace Tienda;

public record PedidoResumen(int Id, DateTime Fecha, string Cliente, decimal Total);
public record VentaProducto(string Producto, int Unidades, decimal Importe);
public record VentaMes
{
    public string Mes { get; init; } = "";
    public double Importe { get; init; }
}

public static class Consultas
{
    /// <summary>Los n pedidos más recientes con cliente y total, en UNA sola consulta.</summary>
    public static List<PedidoResumen> UltimosPedidos(TiendaContext db, int n)
    {
        // TODO: Proyecta con Select a PedidoResumen (cliente = o.Customer.Name, total = suma de Quantity*UnitPrice), ordena por fecha descendente y toma n. Sin seguimiento.
        throw new NotImplementedException();
    }

    /// <summary>Un pedido con su cliente, sus líneas y el producto de cada línea. Una sola consulta.</summary>
    public static Order? PedidoCompleto(TiendaContext db, int id)
    {
        // TODO: Carga ansiosa: Include(Customer) e Include(Lines).ThenInclude(Product). Devuelve null si no existe.
        throw new NotImplementedException();
    }

    /// <summary>Los n productos más vendidos (por unidades), agrupando las líneas de pedido en el servidor.</summary>
    public static List<VentaProducto> TopProductos(TiendaContext db, int n)
    {
        // TODO: GroupBy sobre db.OrderLines por l.Product.Name; unidades = Sum(Quantity), importe = Sum(Quantity*UnitPrice). Ordena (antes del Select) por unidades desc y luego por nombre, y toma n.
        throw new NotImplementedException();
    }

    /// <summary>Búsqueda por nombre con SQL crudo SEGURO (el texto del usuario va como parámetro).</summary>
    public static List<Product> BuscarPorNombre(TiendaContext db, string texto)
    {
        // TODO: FromSql con interpolación ($"...{patron}...") sobre SELECT * FROM Products WHERE Name LIKE ... y sigue con LINQ (AsNoTracking, ToList). NUNCA concatenes el texto.
        throw new NotImplementedException();
    }

    /// <summary>Crea la vista VentasMes (SQL de la base de datos, el equivalente a un procedimiento almacenado de lectura).</summary>
    public static void CrearVistaVentas(TiendaContext db)
    {
        // TODO: ExecuteSqlRaw con CREATE VIEW IF NOT EXISTS VentasMes AS SELECT strftime('%Y-%m', o.Date) AS Mes, SUM(l.Quantity * l.UnitPrice) AS Importe FROM Orders o JOIN OrderLines l ON l.OrderId = o.Id GROUP BY Mes
        throw new NotImplementedException();
    }

    /// <summary>Lee la vista con un tipo que no es una entidad.</summary>
    public static List<VentaMes> VentasPorMes(TiendaContext db)
    {
        // TODO: db.Database.SqlQuery<VentaMes>(...) sobre la vista VentasMes, ordenado por Mes.
        throw new NotImplementedException();
    }

    /// <summary>Acceso de bajo nivel (lo que antes hacía EntityClient): ADO.NET sobre la conexión del contexto.</summary>
    public static long ContarPedidosAdo(TiendaContext db)
    {
        // TODO: Usa db.Database.GetDbConnection(), ábrela si está cerrada, ejecuta SELECT COUNT(*) FROM Orders con ExecuteScalar y devuelve el valor.
        throw new NotImplementedException();
    }

    /// <summary>El SQL que EF Core generaría, sin ejecutarlo.</summary>
    public static string SqlPedidosDesde(TiendaContext db, DateTime desde)
    {
        // TODO: Construye la consulta Orders.Where(Date >= desde) y devuelve su ToQueryString().
        throw new NotImplementedException();
    }

    /// <summary>Un pedido con varias colecciones incluidas, evitando la "explosión cartesiana" de un JOIN enorme.</summary>
    public static Order? PedidoDividido(TiendaContext db, int id)
    {
        // TODO: Como PedidoCompleto pero además Include(o => o.Payments) y AsSplitQuery(): EF lanza una consulta por colección en vez de un único JOIN gigante.
        throw new NotImplementedException();
    }

    /// <summary>Carga explícita: el pedido ya está en memoria y se piden sus líneas más tarde, solo si hacen falta.</summary>
    public static int CargarLineasExplicito(TiendaContext db, int pedidoId)
    {
        // TODO: Carga el pedido SIN líneas (Find); después db.Entry(pedido).Collection(o => o.Lines).Load(); devuelve cuántas líneas tiene.
        throw new NotImplementedException();
    }

    /// <summary>Productos con el precio con IVA calculado por una FUNCIÓN DE LA BASE DE DATOS (el equivalente ligero a un procedimiento/función almacenada).</summary>
    public static List<(string Nombre, decimal ConIva)> ProductosConIva(TiendaContext db)
    {
        // TODO: Select a (p.Name, Funciones.Iva(p.Price)) ordenado por Id y .AsEnumerable().Select(...) para devolver la tupla. La función "iva" la registra el test en la conexión; el mapeo está en Funciones.cs.
        throw new NotImplementedException();
    }
}
