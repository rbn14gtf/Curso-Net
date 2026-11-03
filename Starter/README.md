# Lab 1 · Tecnologías de acceso a datos (Starter)

**Duración:** 40 min (las Partes B y los retos opcionales son trabajo autónomo) · **Requisitos:** .NET 8 SDK (sin servidor de base de datos: usa SQLite)

## Objetivo

Resolver la misma consulta con **ADO.NET**, **Dapper** y **EF Core**, comparar las tres y decidir cuándo usar cada una. Aprender también el ciclo de vida correcto del `DbContext`.

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
| `Lab/Acceso.cs` | las tres implementaciones y el servicio con un contexto por operación (aquí están los `TODO`) |
| `Tests/AccesoTests.cs` | las pruebas que hacen de especificación (no se modifican) |

## Tareas

### 1. ADO.NET

**Dónde:** `Acceso.ClientesPorCiudadAdo`  
**Qué hacer / guía:** Crea el comando con `cn.CreateCommand()`, escribe el `SELECT Id, Name FROM Customers WHERE City = $c ORDER BY Id`, añade el parámetro con `Parameters.AddWithValue("$c", ciudad)` y recorre el `DataReader` con `while (rd.Read())` mapeando cada fila a un `ClienteDto`.  
**Lo valida:** `Las_tres_tecnologias_devuelven_lo_mismo`, `Ado_con_parametro_resiste_inyeccion`

### 2. Dapper

**Dónde:** `Acceso.ClientesPorCiudadDapper`  
**Qué hacer / guía:** Tú escribes el SQL, Dapper hace el mapeo: `cn.Query<ClienteDto>(sql, new { ciudad }).ToList()`. En el SQL el parámetro se llama `@ciudad` (como la propiedad del objeto anónimo).  
**Lo valida:** `Las_tres_tecnologias_devuelven_lo_mismo`

### 3. EF Core

**Dónde:** `Acceso.ClientesPorCiudadEf`  
**Qué hacer / guía:** LINQ sobre `db.Customers`: `AsNoTracking()`, `Where`, `OrderBy`, `Select` a `ClienteDto`, `ToList()`. Al ser una lectura pura no necesitamos seguimiento de cambios.  
**Lo valida:** `Las_tres_tecnologias_devuelven_lo_mismo`, `Ef_no_rastrea_las_entidades_de_una_lectura`

### 4. Un DbContext por operación

**Dónde:** `ClienteServicio.Crear` y `ClienteServicio.Contar`  
**Qué hacer / guía:** El servicio recibe un `Func<TiendaContext>` (en una app real sería un `IDbContextFactory<T>`). En cada método crea un contexto nuevo con `_factory()` dentro de un `using`: nunca guardes uno en un campo.  
**Lo valida:** `El_servicio_usa_un_contexto_por_operacion`

## Parte de diseño (sin código)

Rellena la tabla justificando tu elección en cada escenario y defiéndela en 3 minutos.

| Escenario | Tecnología elegida | Riesgo principal | Mitigación |
|---|---|---|---|
| Tienda online con catálogo, pedidos y stock |  |  |  |
| Informe nocturno con 5 millones de filas de ventas |  |  |  |
| App móvil que funciona sin conexión y sincroniza luego |  |  |  |
| Ingestión de eventos de sensores (50 000 msg/s) |  |  |  |

## Reto opcional

Añade al `AccesoTests` una cuarta variante `ClientesPorCiudadEfSql` con `FromSql($"...{ciudad}...")` y comprueba que genera un parámetro y no una concatenación.

## Entregable

Todas las pruebas en verde y una nota breve (5 líneas) con lo que más te ha sorprendido de tecnologías de acceso a datos.
