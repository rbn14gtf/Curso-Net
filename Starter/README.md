# Lab 5 · Concurrencia y transacciones (Starter)

**Duración:** 45 min (las Partes B y los retos opcionales son trabajo autónomo) · **Requisitos:** .NET 8 SDK (sin servidor de base de datos: usa SQLite)

## Objetivo

Resolver conflictos de **concurrencia optimista** con tres políticas (gana la BD, gana el cliente, mezclar) y escribir una **transacción** que se deshace por completo cuando falla.

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
| `Lab/Concurrencia.cs` | `GuardarConPolitica` y `ComprarAsync` |
| `Tests/ConcurrenciaTests.cs` | dos contextos simulan a dos usuarios |

## Tareas

### 1. Resolver el conflicto

**Dónde:** `GuardarConPolitica`  
**Qué hacer / guía:** Captura `DbUpdateConcurrencyException`. Por cada `ex.Entries`: `GetDatabaseValuesAsync()`; si es `null` la fila se borró (`State = Detached`). **GanaBD**: `ReloadAsync()`. **GanaCliente**: `entry.OriginalValues.SetValues(bd)` y guardar de nuevo. **Mezclar**: para cada propiedad no modificada y que no sea el token, `CurrentValue = bd[prop.Metadata]`; luego `OriginalValues.SetValues(bd)` y guardar. Devuelve `true` si hubo conflicto.  
**Lo valida:** `Sin_conflicto_guarda_con_normalidad`, `GanaBD_...`, `GanaCliente_...`, `Mezclar_...`, `Fila_borrada_por_otro_...`

### 2. Transacción con rollback

**Dónde:** `ComprarAsync`  
**Qué hacer / guía:** Dentro de `db.Database.CreateExecutionStrategy().ExecuteAsync(...)`: `BeginTransactionAsync`; `FindAsync` del producto; guarda el pedido con su línea; resta `Stock` y guarda; si `Stock < 0` lanza `InvalidOperationException("Stock insuficiente")` **sin** `CommitAsync`; si todo va bien, `CommitAsync` y devuelve el Id.  
**Lo valida:** `Comprar_con_stock_crea_pedido_y_descuenta`, `Comprar_sin_stock_deshace_todo`

## Reto opcional

Añade la política `Preguntar`: devuelve al llamador los valores de la BD y los locales (un `record Conflicto`) en lugar de resolver automáticamente.

## Entregable

Todas las pruebas en verde y una nota breve (5 líneas) con lo que más te ha sorprendido de concurrencia y transacciones.
