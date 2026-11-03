using System.ComponentModel.DataAnnotations;

namespace Tienda;

public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

public interface ISoftDelete
{
    bool Deleted { get; set; }
}

public enum OrderStatus { Nuevo, Pagado, Enviado }

public class Address
{
    public string Street { get; set; } = "";
    public string PostalCode { get; set; } = "";
}

public partial class Customer : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string City { get; set; } = "";
    public Address? Address { get; set; }
    public List<Order> Orders { get; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public partial class Order : IAuditable
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public OrderStatus Status { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<OrderLine> Lines { get; } = new();
    public List<Payment> Payments { get; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public partial class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public partial class Product : ISoftDelete
{
    public int Id { get; set; }
    [MaxLength(120)]
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool Deleted { get; set; }
    /// <summary>Token de concurrencia. SQLite no tiene rowversion: TiendaContext asigna un Guid nuevo en cada escritura.</summary>
    public Guid Version { get; set; }
    public List<Tag> Tags { get; } = new();
}

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Product> Products { get; } = new();
}

public abstract class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public decimal Amount { get; set; }
}

public class PagoTarjeta : Payment { public string LastDigits { get; set; } = ""; }
public class Transferencia : Payment { public string Iban { get; set; } = ""; }
