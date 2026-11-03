using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Tienda.Tests;

public class ModeloTests
{
    static IModel Modelo()
    {
        using var t = new TestDb(seed: false);
        using var db = t.Nueva();
        return db.Model;
    }

    static IEntityType Tipo<T>(IModel m) => m.FindEntityType(typeof(T))!;

    [Fact]
    public void T1_precio_con_precision_10_2()
    {
        var p = Tipo<Product>(Modelo()).FindProperty("Price")!;
        Assert.Equal(10, p.GetPrecision());
        Assert.Equal(2, p.GetScale());
    }

    [Fact]
    public void T2_version_es_token_de_concurrencia() =>
        Assert.True(Tipo<Product>(Modelo()).FindProperty("Version")!.IsConcurrencyToken);

    [Fact]
    public void T3_email_tiene_indice_unico() =>
        Assert.Contains(Tipo<Customer>(Modelo()).GetIndexes(),
            i => i.IsUnique && i.Properties.Select(x => x.Name).SequenceEqual(new[] { "Email" }));

    [Fact]
    public void T4_address_es_tipo_propio()
    {
        var nav = Tipo<Customer>(Modelo()).FindNavigation("Address")!;
        Assert.True(nav.ForeignKey.IsOwnership);
    }

    [Fact]
    public void T5_borrado_de_cliente_restringido()
    {
        var fk = Tipo<Order>(Modelo()).FindNavigation("Customer")!.ForeignKey;
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void T6_estado_se_guarda_como_texto()
    {
        var p = Tipo<Order>(Modelo()).FindProperty("Status")!;
        Assert.Equal(typeof(string), p.GetProviderClrType());
        Assert.Equal(20, p.GetMaxLength());
    }

    [Fact]
    public void T7_pagos_con_discriminador_Tipo()
    {
        var m = Modelo();
        var pago = Tipo<Payment>(m);
        Assert.Equal("Tipo", pago.FindDiscriminatorProperty()?.Name);
        Assert.Equal("T", Tipo<PagoTarjeta>(m).GetDiscriminatorValue());
        Assert.Equal("B", Tipo<Transferencia>(m).GetDiscriminatorValue());
    }

    [Fact]
    public void El_script_de_creacion_refleja_el_modelo()
    {
        using var t = new TestDb(seed: false);
        using var db = t.Nueva();
        var sql = db.Database.GenerateCreateScript();
        Assert.Contains("UNIQUE INDEX", sql);          // T3
        Assert.Contains("Address_Street", sql);        // T4
        Assert.Contains("Tipo", sql);                  // T7
    }

    [Fact]
    public void Email_duplicado_es_rechazado_por_la_base()
    {
        using var t = new TestDb();
        using var db = t.Nueva();
        db.Customers.Add(new Customer { Name = "Otra Ana", Email = "ana@tienda.test", City = "Madrid" });
        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void No_se_puede_borrar_un_cliente_con_pedidos()
    {
        using var t = new TestDb();
        using var db = t.Nueva();
        db.Customers.Remove(db.Customers.First(c => c.Name == "Ana"));
        Assert.Throws<DbUpdateException>(() => db.SaveChanges());
    }

    [Fact]
    public void Pagos_polimorficos_se_guardan_y_se_leen()
    {
        using var t = new TestDb();
        using (var db = t.Nueva())
        {
            var o = db.Orders.First();
            o.Payments.Add(new PagoTarjeta { Amount = 10m, LastDigits = "4242" });
            o.Payments.Add(new Transferencia { Amount = 5m, Iban = "ES00" });
            db.SaveChanges();
        }
        using var db2 = t.Nueva();
        Assert.Single(db2.Payments.OfType<PagoTarjeta>());
        Assert.Single(db2.Payments.OfType<Transferencia>());
    }

    // ---- Anotaciones, N:M y estrategias de herencia ----

    [Fact]
    public void T8_el_nombre_del_producto_tiene_longitud_maxima_120() =>
        Assert.Equal(120, Tipo<Product>(Modelo()).FindProperty("Name")!.GetMaxLength());

    [Fact]
    public void T9_relacion_N_M_con_tabla_ProductTags()
    {
        using var t = new TestDb(seed: false);
        using var db = t.Nueva();
        Assert.Contains("\"ProductTags\"", db.Database.GenerateCreateScript());
    }

    [Fact]
    public void T9_los_productos_se_etiquetan_y_se_consultan_por_etiqueta()
    {
        using var t = new TestDb();
        using (var db = t.Nueva())
        {
            var oferta = db.Tags.Single(x => x.Name == "Oferta");
            db.Products.Find(4)!.Tags.Add(oferta);              // Puzle -> Oferta
            db.SaveChanges();
        }
        using var db2 = t.Nueva();
        Assert.Equal(2, db2.Products.Count(p => p.Tags.Any(x => x.Name == "Regalo")));
        Assert.Equal(2, db2.Products.Count(p => p.Tags.Any(x => x.Name == "Oferta")));
    }

    [Fact]
    public void T10_TPT_una_tabla_por_tipo_con_tabla_base()
    {
        using var cn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        using var db = new PagosTptContext(new DbContextOptionsBuilder<PagosTptContext>().UseSqlite(cn).Options);
        var sql = db.Database.GenerateCreateScript();
        Assert.Contains("CREATE TABLE \"Payments\"", sql);
        Assert.Contains("CREATE TABLE \"PagosTarjeta\"", sql);
        Assert.Contains("CREATE TABLE \"Transferencias\"", sql);
        Assert.DoesNotContain("Discriminator", sql);
    }

    [Fact]
    public void T11_TPC_sin_tabla_base_y_columnas_repetidas()
    {
        using var cn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        using var db = new PagosTpcContext(new DbContextOptionsBuilder<PagosTpcContext>().UseSqlite(cn).Options);
        var sql = db.Database.GenerateCreateScript();
        Assert.DoesNotContain("CREATE TABLE \"Payments\"", sql);
        Assert.Contains("CREATE TABLE \"PagosTarjeta\"", sql);
        Assert.Contains("CREATE TABLE \"Transferencias\"", sql);
        // cada tabla lleva TODAS las columnas (también las de la base)
        var pagosTarjeta = sql.Substring(sql.IndexOf("CREATE TABLE \"PagosTarjeta\""));
        Assert.Contains("\"Amount\"", pagosTarjeta[..pagosTarjeta.IndexOf(");")]);
    }
}
