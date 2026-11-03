# Lab 2 · Construcción de modelos (Starter)

**Duración:** 50 min (las Partes B y los retos opcionales son trabajo autónomo) · **Requisitos:** .NET 8 SDK (sin servidor de base de datos: usa SQLite)

## Objetivo

Configurar un modelo con **Fluent API** (precisión, índices, relaciones, tipos propios, conversores y herencia) y gestionar su evolución con **migraciones** y **scaffolding**.

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
| `Data/ModelConfig.cs` | la configuración del modelo: ocho `TODO` (1-7 y N:M) |
| `Lab/Jerarquias.cs` | dos contextos mínimos para comparar TPT y TPC |
| `Data/DesignTimeFactory.cs` | permite ejecutar `dotnet ef` sin aplicación web (ya hecho) |
| `Model/Entities.cs` | las entidades de la tienda (aquí va la anotación `[MaxLength]`; puedes añadir `Customer.Phone` para la Parte B) |
| `Tests/ModeloTests.cs` | pruebas que inspeccionan el modelo y el esquema generado |

## Tareas

### 1. Precisión de decimales

**Dónde:** `ModelConfig.cs`, `Product.Price`  
**Qué hacer / guía:** `b.Entity<Product>().Property(p => p.Price).HasPrecision(10, 2);`  
**Lo valida:** `T1_precio_con_precision_10_2`

### 2. Token de concurrencia

**Dónde:** `Product.Version`  
**Qué hacer / guía:** `.Property(p => p.Version).IsConcurrencyToken()`  
**Lo valida:** `T2_version_es_token_de_concurrencia`

### 3. Índice único

**Dónde:** `Customer.Email`  
**Qué hacer / guía:** `HasIndex(c => c.Email).IsUnique()`  
**Lo valida:** `T3_email_tiene_indice_unico`, `Email_duplicado_es_rechazado_por_la_base`

### 4. Tipo propio (owned)

**Dónde:** `Customer.Address`  
**Qué hacer / guía:** `OwnsOne(c => c.Address)`: las columnas `Address_Street` y `Address_PostalCode` van en la tabla `Customers`.  
**Lo valida:** `T4_address_es_tipo_propio`, `El_script_de_creacion_refleja_el_modelo`

### 5. Relación con borrado restringido

**Dónde:** `Order.Customer`  
**Qué hacer / guía:** `HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict)`  
**Lo valida:** `T5_borrado_de_cliente_restringido`, `No_se_puede_borrar_un_cliente_con_pedidos`

### 6. Enum como texto

**Dónde:** `Order.Status`  
**Qué hacer / guía:** `HasConversion<string>().HasMaxLength(20)`  
**Lo valida:** `T6_estado_se_guarda_como_texto`

### 7. Herencia TPH

**Dónde:** `Payment`  
**Qué hacer / guía:** `HasDiscriminator<string>("Tipo").HasValue<PagoTarjeta>("T").HasValue<Transferencia>("B")`  
**Lo valida:** `T7_pagos_con_discriminador_Tipo`, `Pagos_polimorficos_se_guardan_y_se_leen`

### 8. Anotación de datos

**Dónde:** `Model/Entities.cs`, `Product.Name`  
**Qué hacer / guía:** Añade `[MaxLength(120)]` sobre la propiedad `Name` (el `TODO` está dentro de la clase).  
**Lo valida:** `T8_el_nombre_del_producto_tiene_longitud_maxima_120`

### 9. Relación N:M con tabla propia

**Dónde:** `ModelConfig.cs`  
**Qué hacer / guía:** `b.Entity<Product>().HasMany(p => p.Tags).WithMany(t => t.Products).UsingEntity(j => j.ToTable("ProductTags"));`  
**Lo valida:** `T9_relacion_N_M_con_tabla_ProductTags`, `T9_los_productos_se_etiquetan_y_se_consultan_por_etiqueta`

### 10. TPT: una tabla por tipo

**Dónde:** `Lab/Jerarquias.cs`, `PagosTptContext`  
**Qué hacer / guía:** `UseTptMappingStrategy()` sobre `Payment` y `ToTable("PagosTarjeta")` / `ToTable("Transferencias")` para los subtipos.  
**Lo valida:** `T10_TPT_una_tabla_por_tipo_con_tabla_base`

### 11. TPC: una tabla por tipo concreto

**Dónde:** `Lab/Jerarquias.cs`, `PagosTpcContext`  
**Qué hacer / guía:** `UseTpcMappingStrategy()` sobre `Payment` y las mismas tablas para los subtipos.  
**Lo valida:** `T11_TPC_sin_tabla_base_y_columnas_repetidas`

## Parte B · Migraciones y scaffolding (sin pruebas automáticas)

Con las siete tareas hechas, ejecuta en la carpeta del lab:

```bash
dotnet tool install --global dotnet-ef --version 8.*                      # una sola vez
dotnet ef migrations add Inicial                     # genera la carpeta Migrations/
dotnet ef database update                            # crea tienda.db
```

1. Abre `Migrations/*_Inicial.cs` y localiza el índice único del email, la columna `Address_Street` y la columna `Tipo`.
2. Añade `public string? Phone { get; set; }` a `Customer`, crea la migración `AnadirTelefono` y mira su `Up`/`Down`.
3. Genera el script SQL con `dotnet ef migrations script -o deploy.sql` (en SQL Server añadirías `--idempotent`; **SQLite no lo soporta**).
4. Revierte con `dotnet ef database update Inicial` y elimina la última con `dotnet ef migrations remove`.
5. Scaffolding inverso: `dotnet ef dbcontext scaffold "Data Source=tienda.db" Microsoft.EntityFrameworkCore.Sqlite --output-dir Scaffold --context ScaffoldContext --force`. Compara las entidades generadas con las tuyas: ¿qué convenciones coinciden y cuáles no?

## Reto opcional

Añade un enum `Estado` a `Payment` guardado como texto y configura **todos** los enums del modelo a la vez con `ConfigureConventions` (`c.Properties<Enum>().HaveConversion<string>()`).

## Entregable

Todas las pruebas en verde y una nota breve (5 líneas) con lo que más te ha sorprendido de construcción de modelos.
