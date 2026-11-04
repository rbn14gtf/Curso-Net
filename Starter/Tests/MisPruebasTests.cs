using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tienda.Tests;

// ESTE archivo es el ejercicio "pruebas unitarias del acceso a datos": las escribes tú.
public class MisPruebasTests
{
    [Fact]
    public void Un_email_repetido_falla_con_SQLite_en_memoria()
    {
        // TODO: Con un TestDb: añade un Customer con el email "ana@tienda.test" (ya existe) y comprueba con Assert.Throws<DbUpdateException>(() => db.SaveChanges()) que la base de datos lo rechaza.
        throw new NotImplementedException();
    }

    [Fact]
    public void El_proveedor_InMemory_NO_comprueba_la_unicidad()
    {
        // TODO: Crea un contexto con UseInMemoryDatabase(Guid.NewGuid().ToString()) y EnsureCreated; añade DOS clientes con el mismo email, guarda SIN excepción y comprueba que hay 2 (esto demuestra por qué InMemory puede ocultar errores reales).
        throw new NotImplementedException();
    }

    [Fact]
    public void AsNoTracking_no_deja_entidades_rastreadas()
    {
        // TODO: Con un TestDb: lee los productos con seguimiento y comprueba db.ChangeTracker.Entries().Count() == 6; en OTRO contexto léelos con AsNoTracking y comprueba que no hay ninguna entrada.
        throw new NotImplementedException();
    }
}
