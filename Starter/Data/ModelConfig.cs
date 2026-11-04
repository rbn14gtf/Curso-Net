using Microsoft.EntityFrameworkCore;

namespace Tienda;

public partial class TiendaContext
{
    void ConfigureModel(ModelBuilder b)
    {
        b.Entity<Product>().Property(p => p.Price).HasPrecision(10, 2);
        b.Entity<Product>().Property(p => p.Version).IsConcurrencyToken();
        b.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
        b.Entity<Customer>().OwnsOne(c => c.Address);
        b.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Order>().Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        b.Entity<Payment>().HasDiscriminator<string>("Tipo")
            .HasValue<PagoTarjeta>("T").HasValue<Transferencia>("B");
    }
}
