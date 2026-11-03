using Microsoft.EntityFrameworkCore;

namespace Tienda;

// Dos contextos mínimos que mapean la MISMA jerarquía de pagos de dos formas distintas.
// (El modelo "real" de la tienda usa TPH; aquí se comparan las otras dos estrategias.)

/// <summary>TPT: una tabla para la base (Payments) y una por cada subtipo.</summary>
public class PagosTptContext : DbContext
{
    public PagosTptContext(DbContextOptions<PagosTptContext> o) : base(o) { }
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Customer>().OwnsOne(c => c.Address);                      // (ya hecho)
        // TODO: TPT: b.Entity<Payment>().UseTptMappingStrategy(); y una tabla por subtipo: PagoTarjeta -> "PagosTarjeta", Transferencia -> "Transferencias" (ToTable).

    }

    protected override void ConfigureConventions(ModelConfigurationBuilder c)
        => c.Properties<decimal>().HaveConversion<double>();
}

/// <summary>TPC: SIN tabla para la base; cada subtipo concreto tiene una tabla con todas sus columnas.</summary>
public class PagosTpcContext : DbContext
{
    public PagosTpcContext(DbContextOptions<PagosTpcContext> o) : base(o) { }
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Customer>().OwnsOne(c => c.Address);                      // (ya hecho)
        // TODO: TPC (EF Core 7+): b.Entity<Payment>().UseTpcMappingStrategy(); y una tabla por subtipo concreto: "PagosTarjeta" y "Transferencias".

    }

    protected override void ConfigureConventions(ModelConfigurationBuilder c)
        => c.Properties<decimal>().HaveConversion<double>();
}
