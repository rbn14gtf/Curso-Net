using Xunit;

namespace Tienda.Tests;

public class AccesoTests
{
    [Theory]
    [InlineData("Madrid", 2)]
    [InlineData("Sevilla", 1)]
    [InlineData("Valencia", 0)]
    public void Las_tres_tecnologias_devuelven_lo_mismo(string ciudad, int esperados)
    {
        using var t = new TestDb();
        using var db = t.Nueva();

        var ado = Acceso.ClientesPorCiudadAdo(t.Connection, ciudad);
        var dapper = Acceso.ClientesPorCiudadDapper(t.Connection, ciudad);
        var ef = Acceso.ClientesPorCiudadEf(db, ciudad);

        Assert.Equal(esperados, ado.Count);
        Assert.Equal(ado, dapper);
        Assert.Equal(ado, ef);
    }

    [Fact]
    public void Ado_con_parametro_resiste_inyeccion()
    {
        using var t = new TestDb();
        Assert.Empty(Acceso.ClientesPorCiudadAdo(t.Connection, "x' OR '1'='1"));
    }

    [Fact]
    public void Ef_no_rastrea_las_entidades_de_una_lectura()
    {
        using var t = new TestDb();
        using var db = t.Nueva();
        Acceso.ClientesPorCiudadEf(db, "Madrid");
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [Fact]
    public void El_servicio_usa_un_contexto_por_operacion()
    {
        using var t = new TestDb();
        var creados = new List<TiendaContext>();
        var servicio = new ClienteServicio(() => { var c = t.Nueva(); creados.Add(c); return c; });

        var id = servicio.Crear("Eva", "León");
        var total = servicio.Contar();

        Assert.True(id > 0);
        Assert.Equal(5, total);
        Assert.Equal(2, creados.Distinct().Count());   // un contexto por operación, no uno compartido
    }
}
