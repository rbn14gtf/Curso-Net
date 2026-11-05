using Microsoft.EntityFrameworkCore;

namespace Tienda;

public enum Politica
{
    /// <summary>Se descartan los cambios locales y se queda lo que hay en la base de datos.</summary>
    GanaBD,
    /// <summary>Los cambios locales pisan lo que hay en la base de datos.</summary>
    GanaCliente,
    /// <summary>Se conservan los campos que cambió el cliente y se toma de la BD el resto.</summary>
    Mezclar
}

public static class Concurrencia
{
    /// <summary>Guarda los cambios pendientes. Si otro usuario modificó la fila, resuelve según la política. Devuelve true si hubo conflicto.</summary>
    public static async Task<bool> GuardarConPolitica(TiendaContext db, Politica politica)
    {
        // TODO: Captura DbUpdateConcurrencyException. Por cada ex.Entries: lee GetDatabaseValuesAsync(); si es null (la borraron) haz entry.State = Detached y sigue; si no, aplica la política (ver la guía). Tras resolver, vuelve a guardar si procede. Devuelve true si hubo conflicto.
        throw new NotImplementedException();
    }

    /// <summary>Crea un pedido y descuenta el stock en UNA transacción. Si no hay existencias, no queda nada guardado.</summary>
    public static async Task<int> ComprarAsync(TiendaContext db, int clienteId, int productoId, int cantidad)
    {
        // TODO: Dentro de db.Database.CreateExecutionStrategy().ExecuteAsync(...): BeginTransactionAsync; busca el producto; guarda el pedido con su línea; resta el stock y guarda; si Stock < 0 lanza InvalidOperationException("Stock insuficiente") SIN confirmar (el using de la transacción hace el rollback); si todo va bien CommitAsync y devuelve el Id del pedido.
        throw new NotImplementedException();
    }
}
