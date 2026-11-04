using Xunit;

namespace Tienda.Tests;

public class ConsultasTests
{
    [Fact]
    public void UltimosPedidos_proyecta_en_una_sola_consulta()
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        var r = Consultas.UltimosPedidos(db, 2);

        Assert.Equal(1, t.Log.Count);
        Assert.Equal(new[] { 5, 4 }, r.Select(x => x.Id));
        Assert.Equal("Marta", r[0].Cliente);
        Assert.Equal(37.50m, r[0].Total);
        Assert.Equal(60.00m, r[1].Total);
        Assert.Empty(db.ChangeTracker.Entries());      // una proyección no rastrea nada
    }

    [Fact]
    public void PedidoCompleto_carga_el_grafo_con_una_consulta()
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        var o = Consultas.PedidoCompleto(db, 1);

        Assert.Equal(1, t.Log.Count);
        Assert.NotNull(o);
        Assert.Equal("Ana", o!.Customer.Name);
        Assert.Equal(2, o.Lines.Count);
        Assert.All(o.Lines, l => Assert.NotNull(l.Product));
        Assert.Null(Consultas.PedidoCompleto(db, 999));
    }

    [Fact]
    public void TopProductos_agrupa_en_el_servidor()
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        var top = Consultas.TopProductos(db, 2);

        Assert.Equal(1, t.Log.Count);
        Assert.Equal("Novela", top[0].Producto);
        Assert.Equal(6, top[0].Unidades);
        Assert.Equal(75.00m, top[0].Importe);
        Assert.Equal("Ajedrez", top[1].Producto);
        Assert.Equal(2, top[1].Unidades);
    }

    [Fact]
    public void BuscarPorNombre_usa_parametros()
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        Assert.Equal(3, Consultas.BuscarPorNombre(db, "o").Count);       // Novela, Ensayo, Disco
        Assert.Empty(Consultas.BuscarPorNombre(db, "' OR 1=1 --"));       // la inyección no funciona
        Assert.DoesNotContain("1=1", t.Log.Sql.Last());                   // el texto viaja como parámetro
    }

    [Fact]
    public void Vista_de_ventas_se_lee_con_SqlQuery()
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        Consultas.CrearVistaVentas(db);
        var meses = Consultas.VentasPorMes(db);

        var m = Assert.Single(meses);
        Assert.Equal("2024-01", m.Mes);
        Assert.Equal(190.0, m.Importe, 2);
    }

    [Fact]
    public void ContarPedidos_con_ADO_NET_sobre_la_conexion_del_contexto()
    {
        using var t = new TestDb();
        using var db = t.Nueva();
        Assert.Equal(5, Consultas.ContarPedidosAdo(db));
    }

    [Fact]
    public void ToQueryString_muestra_el_SQL_sin_ejecutarlo()
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        var sql = Consultas.SqlPedidosDesde(db, new DateTime(2024, 1, 15));

        Assert.Contains("WHERE", sql);
        Assert.Contains("FROM \"Orders\"", sql);
        Assert.Equal(0, t.Log.Count);
    }

    [Fact]
    public void PedidoDividido_usa_una_consulta_por_coleccion()
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        var o = Consultas.PedidoDividido(db, 1);

        Assert.NotNull(o);
        Assert.Equal(3, t.Log.Count);                  // principal (+Customer) + Lines(+Product) + Payments
        Assert.Equal(2, o!.Lines.Count);
        Assert.Empty(o.Payments);
    }

    [Fact]
    public void Carga_explicita_hace_una_consulta_mas_cuando_se_pide()
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        var n = Consultas.CargarLineasExplicito(db, 3);

        Assert.Equal(2, n);
        Assert.Equal(2, t.Log.Count);                  // Find + Load de las líneas
    }

    [Fact]
    public void Funcion_de_base_de_datos_mapeada_con_HasDbFunction()
    {
        using var t = new TestDb();
        t.Connection.CreateFunction("iva", (double p) => p * 1.21);     // la "función almacenada"
        using var db = t.Nueva();

        var l = Consultas.ProductosConIva(db);

        Assert.Equal(6, l.Count);
        Assert.Equal("Novela", l[0].Nombre);
        Assert.Equal(15.125m, Math.Round(l[0].ConIva, 3));              // 12,50 * 1,21
        Assert.Contains("\"iva\"(", t.Log.Sql.Last());                     // se calculó en la base de datos
    }
}
