# Lab 1 · Tecnologías de acceso a datos (Solution)

**Duración:** 40 min (las Partes B y los retos opcionales son trabajo autónomo) · **Requisitos:** .NET 8 SDK (sin servidor de base de datos: usa SQLite)

## Objetivo

Resolver la misma consulta con **ADO.NET**, **Dapper** y **EF Core**, comparar las tres y decidir cuándo usar cada una. Aprender también el ciclo de vida correcto del `DbContext`.

Solución completa del lab. Los `TODO` del Starter están resueltos y comentados abajo. Las pruebas son las mismas: **todas deben pasar**.

```bash
cd Starter                       # (o Solution)
dotnet test                      # todas las pruebas
dotnet test --filter "FullyQualifiedName~Nombre"   # solo las que contengan Nombre
```

## Qué hay en esta carpeta

| Fichero | Contenido |
|---|---|
| `Lab/Acceso.cs` | las tres implementaciones y el servicio con un contexto por operación (aquí están los `TODO`) |
| `Tests/AccesoTests.cs` | las pruebas que hacen de especificación (no se modifican) |

## Tareas

### 1. ADO.NET

**Dónde:** `Acceso.ClientesPorCiudadAdo`  
**Qué hacer / guía:** Crea el comando con `cn.CreateCommand()`, escribe el `SELECT Id, Name FROM Customers WHERE City = $c ORDER BY Id`, añade el parámetro con `Parameters.AddWithValue("$c", ciudad)` y recorre el `DataReader` con `while (rd.Read())` mapeando cada fila a un `ClienteDto`.  
**Lo valida:** `Las_tres_tecnologias_devuelven_lo_mismo`, `Ado_con_parametro_resiste_inyeccion`

**Por qué así:** Mucho código para una consulta simple, pero es la base de todo lo demás. El parámetro hace que el valor nunca se interprete como SQL.

### 2. Dapper

**Dónde:** `Acceso.ClientesPorCiudadDapper`  
**Qué hacer / guía:** Tú escribes el SQL, Dapper hace el mapeo: `cn.Query<ClienteDto>(sql, new { ciudad }).ToList()`. En el SQL el parámetro se llama `@ciudad` (como la propiedad del objeto anónimo).  
**Lo valida:** `Las_tres_tecnologias_devuelven_lo_mismo`

**Por qué así:** Una línea de código real. El SQL sigue en una cadena: si renombras la columna `City`, falla en ejecución, no al compilar.

### 3. EF Core

**Dónde:** `Acceso.ClientesPorCiudadEf`  
**Qué hacer / guía:** LINQ sobre `db.Customers`: `AsNoTracking()`, `Where`, `OrderBy`, `Select` a `ClienteDto`, `ToList()`. Al ser una lectura pura no necesitamos seguimiento de cambios.  
**Lo valida:** `Las_tres_tecnologias_devuelven_lo_mismo`, `Ef_no_rastrea_las_entidades_de_una_lectura`

**Por qué así:** LINQ tipado: el compilador protege los nombres de propiedades. El SQL lo genera EF Core (actívalo con `.LogTo(Console.WriteLine)` para verlo).

### 4. Un DbContext por operación

**Dónde:** `ClienteServicio.Crear` y `ClienteServicio.Contar`  
**Qué hacer / guía:** El servicio recibe un `Func<TiendaContext>` (en una app real sería un `IDbContextFactory<T>`). En cada método crea un contexto nuevo con `_factory()` dentro de un `using`: nunca guardes uno en un campo.  
**Lo valida:** `El_servicio_usa_un_contexto_por_operacion`

**Por qué así:** El contexto es ligero y no es seguro entre hilos: uno por unidad de trabajo. En ASP.NET Core lo hace `AddDbContext` (scoped por petición).

## Parte de diseño (sin código)

Rellena la tabla justificando tu elección en cada escenario y defiéndela en 3 minutos.

| Escenario | Tecnología elegida | Riesgo principal | Mitigación |
|---|---|---|---|
| Tienda online con catálogo, pedidos y stock | EF Core | N+1 y contexto compartido | `Include`/proyecciones, contexto scoped por petición, migraciones |
| Informe nocturno con 5 millones de filas de ventas | Dapper / SQL (o EF Core con `AsNoTracking` + streaming) | Memoria al materializar | Streaming y agregación en la base de datos |
| App móvil que funciona sin conexión y sincroniza luego | EF Core + SQLite local | Conflictos al sincronizar | Token de concurrencia y política de resolución (módulo 5) |
| Ingestión de eventos de sensores (50 000 msg/s) | ADO.NET / bulk copy o almacén específico | El ORM no está pensado para ese volumen | Lotes con `SqlBulkCopy` o cola + escritor dedicado |

> Orientativo: lo importante es la justificación, no coincidir con esta tabla.

## Reto opcional

Añade al `AccesoTests` una cuarta variante `ClientesPorCiudadEfSql` con `FromSql($"...{ciudad}...")` y comprueba que genera un parámetro y no una concatenación.

## Notas para el profesor

Pide a los alumnos que comparen líneas de código y que **renombren la columna `City`** en `Entities.cs`: ¿qué versión falla al compilar y cuál al ejecutar? Es la mejor forma de ver el valor del tipado de EF Core. En la parte de diseño, no hay una respuesta única: valora la justificación.

### Errores frecuentes

- Usar `new TiendaContext(...)` una sola vez al arrancar y guardarlo: provoca estado sucio y fallos de concurrencia.
- Concatenar `ciudad` en el SQL de ADO.NET o Dapper (inyección SQL).
