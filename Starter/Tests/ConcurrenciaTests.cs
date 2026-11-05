using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tienda.Tests;

public class ConcurrenciaTests
{
    /// <summary>A y B cargan el producto 1; B guarda primero un cambio; A queda con datos antiguos.</summary>
    static (TiendaContext a, Product pa) Conflicto(TestDb t, Action<Product> cambioDeB)
    {
        var a = t.Nueva();
        var pa = a.Products.Find(1)!;
        using (var b = t.Nueva())
        {
            var pb = b.Products.Find(1)!;
            cambioDeB(pb);
            b.SaveChanges();
        }
        return (a, pa);
    }

    [Fact]
    public void Sin_politica_el_conflicto_lanza_excepcion()
    {
        using var t = new TestDb();
        var (a, pa) = Conflicto(t, p => p.Price = 20m);
        using (a)
        {
            pa.Price = 15m;
            Assert.Throws<DbUpdateConcurrencyException>(() => a.SaveChanges());
        }
    }

    [Fact]
    public async Task Sin_conflicto_guarda_con_normalidad()
    {
        using var t = new TestDb();
        using var db = t.Nueva();
        db.Products.Find(1)!.Price = 13m;
        Assert.False(await Concurrencia.GuardarConPolitica(db, Politica.GanaBD));
        using var db2 = t.Nueva();
        Assert.Equal(13m, db2.Products.Find(1)!.Price);
    }

    [Fact]
    public async Task GanaBD_descarta_los_cambios_locales()
    {
        using var t = new TestDb();
        var (a, pa) = Conflicto(t, p => p.Price = 20m);
        using (a)
        {
            pa.Price = 15m;
            Assert.True(await Concurrencia.GuardarConPolitica(a, Politica.GanaBD));
            Assert.Equal(20m, pa.Price);                          // la entidad local se refrescó
        }
        using var db2 = t.Nueva();
        Assert.Equal(20m, db2.Products.Find(1)!.Price);
    }

    [Fact]
    public async Task GanaCliente_pisa_a_la_base_de_datos()
    {
        using var t = new TestDb();
        var (a, pa) = Conflicto(t, p => p.Price = 20m);
        using (a)
        {
            pa.Price = 15m;
            Assert.True(await Concurrencia.GuardarConPolitica(a, Politica.GanaCliente));
        }
        using var db2 = t.Nueva();
        Assert.Equal(15m, db2.Products.Find(1)!.Price);
    }

    [Fact]
    public async Task Mezclar_conserva_lo_que_cambio_cada_usuario()
    {
        using var t = new TestDb();
        var (a, pa) = Conflicto(t, p => p.Stock = 7);             // B cambia el stock
        using (a)
        {
            pa.Price = 15m;                                        // A cambia el precio
            Assert.True(await Concurrencia.GuardarConPolitica(a, Politica.Mezclar));
        }
        using var db2 = t.Nueva();
        var p = db2.Products.Find(1)!;
        Assert.Equal(15m, p.Price);                                // de A
        Assert.Equal(7, p.Stock);                                  // de B
    }

    [Fact]
    public async Task Fila_borrada_por_otro_no_rompe_la_resolucion()
    {
        using var t = new TestDb();
        using var a = t.Nueva();
        var pa = a.Products.Find(6)!;                              // Lámpara: no tiene líneas de pedido
        using (var b = t.Nueva()) { b.Products.Remove(b.Products.Find(6)!); b.SaveChanges(); }
        pa.Price = 1m;
        Assert.True(await Concurrencia.GuardarConPolitica(a, Politica.GanaCliente));
        using var db2 = t.Nueva();
        Assert.Null(db2.Products.Find(6));
    }

    [Fact]
    public async Task Comprar_con_stock_crea_pedido_y_descuenta()
    {
        using var t = new TestDb();
        int id;
        using (var db = t.Nueva()) id = await Concurrencia.ComprarAsync(db, 1, 1, 4);
        using var db2 = t.Nueva();
        Assert.Equal(16, db2.Products.Find(1)!.Stock);
        Assert.Equal(6, db2.Orders.Count());
        Assert.Equal(4, db2.OrderLines.Single(l => l.OrderId == id).Quantity);
    }

    [Fact]
    public async Task Comprar_sin_stock_deshace_todo()
    {
        using var t = new TestDb();
        using (var db = t.Nueva())
            await Assert.ThrowsAsync<InvalidOperationException>(() => Concurrencia.ComprarAsync(db, 1, 5, 4)); // Ajedrez: stock 3
        using var db2 = t.Nueva();
        Assert.Equal(3, db2.Products.Find(5)!.Stock);              // el stock no cambió
        Assert.Equal(5, db2.Orders.Count());                       // y no quedó ningún pedido a medias
    }
}
