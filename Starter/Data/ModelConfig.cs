using Microsoft.EntityFrameworkCore;

namespace Tienda;

public partial class TiendaContext
{
    void ConfigureModel(ModelBuilder b)
    {
        // TODO: 1. Product.Price: precisión 10 y escala 2 (HasPrecision).


        // TODO: 2. Product.Version: token de concurrencia (IsConcurrencyToken).


        // TODO: 3. Customer.Email: índice ÚNICO (HasIndex + IsUnique).


        // TODO: 4. Customer.Address: tipo propio (OwnsOne) para que sus columnas vayan en la tabla Customers.


        // TODO: 5. Order -> Customer: relación 1:N con borrado RESTRINGIDO (no se puede borrar un cliente con pedidos).


        // TODO: 6. Order.Status: guárdalo como texto (HasConversion<string>) con longitud máxima 20.


        // TODO: 7. Payment: herencia TPH con discriminador "Tipo" (string): PagoTarjeta = "T", Transferencia = "B".


        // TODO: 9. Relación N:M Product <-> Tag con la tabla intermedia llamada "ProductTags" (HasMany/WithMany/UsingEntity + ToTable).

    }
}
