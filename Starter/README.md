# Lab 3 · Consulta de datos de entidades (Starter)

**Duración:** 45 min (las Partes B y los retos opcionales son trabajo autónomo) · **Requisitos:** .NET 8 SDK (sin servidor de base de datos: usa SQLite)

## Objetivo

Escribir consultas **LINQ** eficientes (proyecciones, carga ansiosa, agrupaciones), usar **SQL crudo seguro**, leer vistas con `SqlQuery<T>` y acceder a bajo nivel con ADO.NET. Sustituye a Entity SQL, EntityClient y procedimientos almacenados del temario clásico.

## Cómo trabajar

1. Abre esta carpeta en tu IDE (o en una terminal).
2. Ejecuta las pruebas: **fallarán** (o darán `NotImplementedException`); es lo esperado.
3. Busca los `// TODO` y resuélvelos uno a uno, en el orden de abajo.
4. Vuelve a ejecutar las pruebas hasta que todo esté en verde. **No modifiques las pruebas.**

```bash
cd Starter                       # (o Solution)
dotnet test                      # todas las pruebas
dotnet test --filter "FullyQualifiedName~Nombre"   # solo las que contengan Nombre
```

## Qué hay en esta carpeta

| Fichero | Contenido |
|---|---|
| `Lab/Consultas.cs` | las once consultas (aquí están los `TODO`) |
| `Lab/Funciones.cs` | función de base de datos mapeada con `HasDbFunction` |
| `Tests/ConsultasTests.cs` | pruebas; varias cuentan los comandos SQL ejecutados |
| `Tests/MisPruebasTests.cs` | las pruebas que escribes tú |

> **Nota:** SQLite no suma ni ordena `decimal`; estos labs guardan los importes como `double` (ver `TiendaContext.ConfigureConventions`). En SQL Server no haría falta.

## Tareas

### 1. Proyección a un DTO

**Dónde:** `UltimosPedidos`  
**Qué hacer / guía:** `Select` a `PedidoResumen` usando `o.Customer.Name` y `o.Lines.Sum(l => l.Quantity * l.UnitPrice)`; `OrderByDescending(o => o.Date).Take(n)`; `AsNoTracking`.  
**Lo valida:** `UltimosPedidos_proyecta_en_una_sola_consulta`

### 2. Carga ansiosa

**Dónde:** `PedidoCompleto`  
**Qué hacer / guía:** `Include(o => o.Customer).Include(o => o.Lines).ThenInclude(l => l.Product).FirstOrDefault(...)`.  
**Lo valida:** `PedidoCompleto_carga_el_grafo_con_una_consulta`

### 3. Agrupación en el servidor

**Dónde:** `TopProductos`  
**Qué hacer / guía:** `GroupBy(l => l.Product.Name)`, ordena por `g.Sum(l => l.Quantity)` **antes** del `Select`, `Take(n)` y proyecta a `VentaProducto`.  
**Lo valida:** `TopProductos_agrupa_en_el_servidor`

### 4. SQL crudo seguro

**Dónde:** `BuscarPorNombre`  
**Qué hacer / guía:** `var patron = $"%{texto}%"; db.Products.FromSql($"SELECT * FROM Products WHERE Name LIKE {patron}")`. La interpolación de `FromSql` se convierte en **parámetro**.  
**Lo valida:** `BuscarPorNombre_usa_parametros`

### 5. Una vista en lugar del procedimiento

**Dónde:** `CrearVistaVentas`  
**Qué hacer / guía:** `db.Database.ExecuteSqlRaw("CREATE VIEW IF NOT EXISTS VentasMes AS SELECT strftime('%Y-%m', o.Date) AS Mes, SUM(l.Quantity * l.UnitPrice) AS Importe FROM Orders o JOIN OrderLines l ON l.OrderId = o.Id GROUP BY Mes")`  
**Lo valida:** `Vista_de_ventas_se_lee_con_SqlQuery`

### 6. Leer la vista

**Dónde:** `VentasPorMes`  
**Qué hacer / guía:** `db.Database.SqlQuery<VentaMes>($"SELECT Mes, Importe FROM VentasMes ORDER BY Mes").ToList()` (EF Core 7+: tipos que no son entidades).  
**Lo valida:** `Vista_de_ventas_se_lee_con_SqlQuery`

### 7. ADO.NET sobre el contexto (antes EntityClient)

**Dónde:** `ContarPedidosAdo`  
**Qué hacer / guía:** `db.Database.GetDbConnection()`, ábrela si está cerrada, `CreateCommand()`, `SELECT COUNT(*) FROM Orders` y `ExecuteScalar()`.  
**Lo valida:** `ContarPedidos_con_ADO_NET_sobre_la_conexion_del_contexto`

### 8. Ver el SQL sin ejecutarlo

**Dónde:** `SqlPedidosDesde`  
**Qué hacer / guía:** `db.Orders.Where(o => o.Date >= desde).ToQueryString()`  
**Lo valida:** `ToQueryString_muestra_el_SQL_sin_ejecutarlo`

### 9. Una consulta por colección (AsSplitQuery)

**Dónde:** `PedidoDividido`  
**Qué hacer / guía:** Como `PedidoCompleto` pero añade `Include(o => o.Payments)` y `AsSplitQuery()`.  
**Lo valida:** `PedidoDividido_usa_una_consulta_por_coleccion`

### 10. Carga explícita

**Dónde:** `CargarLineasExplicito`  
**Qué hacer / guía:** `db.Orders.Find(id)` y después `db.Entry(pedido).Collection(o => o.Lines).Load()`; devuelve `pedido.Lines.Count`.  
**Lo valida:** `Carga_explicita_hace_una_consulta_mas_cuando_se_pide`

### 11. Función de base de datos (HasDbFunction)

**Dónde:** `Funciones.cs` y `ProductosConIva`  
**Qué hacer / guía:** En `ConfigureExtra`: `b.HasDbFunction(typeof(Funciones).GetMethod(nameof(Funciones.Iva))!).HasName("iva")`. Después `Select(p => new { p.Name, ConIva = Funciones.Iva(p.Price) })`. El test registra la función `iva` en SQLite.  
**Lo valida:** `Funcion_de_base_de_datos_mapeada_con_HasDbFunction`

### 12. Escribir tus propias pruebas

**Dónde:** `Tests/MisPruebasTests.cs` (**este archivo es tuyo**)  
**Qué hacer / guía:** Cada prueba tiene un `TODO` que describe qué comprobar: email repetido con SQLite (falla), el mismo caso con `UseInMemoryDatabase` (no falla) y `AsNoTracking`.  
**Lo valida:** `MisPruebasTests` (3 pruebas)

## Parte B · Procedimientos almacenados (SQL Server, opcional — no verificado aquí)

SQLite no tiene procedimientos almacenados, por eso las tareas 5-6 usan una vista y la 9 una **función de base de datos**. Si dispones de SQL Server (LocalDB o Docker `mcr.microsoft.com/mssql/server`), cambia `UseSqlite` por `UseSqlServer` y prueba:

```sql
CREATE PROCEDURE dbo.TopProductos @n int AS
  SELECT TOP (@n) p.* FROM Products p ORDER BY p.Stock DESC;
GO
CREATE PROCEDURE dbo.ContarPedidos @clienteId int, @total int OUTPUT AS
  SELECT @total = COUNT(*) FROM Orders WHERE CustomerId = @clienteId;
```

```csharp
// Lectura: devuelve entidades (NO se puede seguir componiendo con LINQ tras EXEC)
var top = await db.Products.FromSql($"EXEC dbo.TopProductos {n}").ToListAsync();

// Parámetro de salida
var total = new SqlParameter("@total", SqlDbType.Int) { Direction = ParameterDirection.Output };
await db.Database.ExecuteSqlRawAsync("EXEC dbo.ContarPedidos @clienteId, @total OUT", new SqlParameter("@clienteId", 1), total);

// EF Core 7+: insertar/actualizar/borrar con procedimientos (equivalente al mapeo de SP del EDMX)
b.Entity<Product>().InsertUsingStoredProcedure("Product_Insert", sp => sp
    .HasParameter(p => p.Id, p => p.IsOutput()).HasParameter(p => p.Name).HasParameter(p => p.Price));
```

## Reto opcional

Escribe una consulta dinámica `Buscar(db, ciudad?, categoria?)` que componga un `IQueryable` añadiendo `Where` solo para los filtros informados (el sustituto de Entity SQL dinámico).

## Entregable

Todas las pruebas en verde y una nota breve (5 líneas) con lo que más te ha sorprendido de consulta de datos de entidades.
