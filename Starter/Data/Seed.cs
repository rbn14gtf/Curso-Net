namespace Tienda;

/// <summary>Datos de ejemplo. Ids: clientes 1-4, productos 1-6, pedidos 1-5.</summary>
public static class Seed
{
    public static void Cargar(TiendaContext db)
    {
        var ana = new Customer { Name = "Ana", Email = "ana@tienda.test", City = "Madrid", Address = new() { Street = "Gran Vía 1", PostalCode = "28013" } };
        var luis = new Customer { Name = "Luis", Email = "luis@tienda.test", City = "Madrid" };
        var marta = new Customer { Name = "Marta", Email = "marta@tienda.test", City = "Sevilla" };
        var pedro = new Customer { Name = "Pedro", Email = "pedro@tienda.test", City = "Bilbao" };
        db.Customers.AddRange(ana, luis, marta, pedro);

        var libroA = new Product { Name = "Novela", Category = "Libros", Price = 12.50m, Stock = 20 };
        var libroB = new Product { Name = "Ensayo", Category = "Libros", Price = 18.00m, Stock = 10 };
        var disco = new Product { Name = "Disco", Category = "Música", Price = 15.00m, Stock = 5 };
        var juegoA = new Product { Name = "Puzle", Category = "Juegos", Price = 22.00m, Stock = 8 };
        var juegoB = new Product { Name = "Ajedrez", Category = "Juegos", Price = 30.00m, Stock = 3 };
        var lampara = new Product { Name = "Lámpara", Category = "Hogar", Price = 40.00m, Stock = 0 };
        db.Products.AddRange(libroA, libroB, disco, juegoA, juegoB, lampara);
        db.Tags.AddRange(new Tag { Name = "Regalo", Products = { libroA, juegoB } }, new Tag { Name = "Oferta", Products = { disco } });

        Order Pedido(Customer c, int dia, OrderStatus s, params (Product p, int q)[] lineas)
        {
            var o = new Order { Customer = c, Date = new DateTime(2024, 1, dia), Status = s };
            foreach (var (p, q) in lineas) o.Lines.Add(new OrderLine { Product = p, Quantity = q, UnitPrice = p.Price });
            return o;
        }
        db.Orders.AddRange(
            Pedido(ana, 5, OrderStatus.Enviado, (libroA, 2), (disco, 1)),     // 40,00
            Pedido(ana, 12, OrderStatus.Pagado, (juegoA, 1)),                 // 22,00
            Pedido(luis, 15, OrderStatus.Nuevo, (libroB, 1), (libroA, 1)),    // 30,50
            Pedido(marta, 20, OrderStatus.Enviado, (juegoB, 2)),              // 60,00
            Pedido(marta, 28, OrderStatus.Nuevo, (libroA, 3)));               // 37,50
        db.SaveChanges();
    }
}
