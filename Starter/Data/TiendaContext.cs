using Microsoft.EntityFrameworkCore;

namespace Tienda;

public partial class TiendaContext : DbContext
{
    public TiendaContext(DbContextOptions<TiendaContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        ConfigureModel(b);
        ConfigureExtra(b);
    }

    /// <summary>SQLite no sabe sumar ni ordenar decimal: en estos labs se guarda como double (en SQL Server no haría falta).</summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder c)
        => c.Properties<decimal>().HaveConversion<double>();

    /// <summary>Gancho para que cada lab añada configuración propia sin tocar ModelConfig.</summary>
    partial void ConfigureExtra(ModelBuilder b);

    /// <summary>Gancho que se ejecuta justo antes de cada SaveChanges.</summary>
    partial void AntesDeGuardar();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        Preparar();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        Preparar();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    void Preparar()
    {
        AntesDeGuardar();
        // Simula rowversion: nuevo Guid en cada insert/update de un producto
        foreach (var e in ChangeTracker.Entries<Product>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
            e.Entity.Version = Guid.NewGuid();
    }
}
