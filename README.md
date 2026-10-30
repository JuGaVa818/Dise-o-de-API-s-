# Sistema de parqueo

API en .NET 8 con repositorios para PostgreSQL/EF Core y JSON local.

## Ejecutar la API con JSON local

Desde la raíz del repositorio:

```powershell
dotnet run --project Parqueo\Parqueo.csproj
```

Por defecto, la API utiliza los tres JSON copiados junto al ejecutable. Las
escrituras de los repositorios JSON reemplazan el archivo de forma atómica.

## API de automóviles

Base URL local: `http://localhost:5000` (o la URL que indique `dotnet run`).

| Método | Ruta | Acción |
|---|---|---|
| GET | `/api/automoviles` | Consultar todos los automóviles |
| GET | `/api/automoviles?color=blan&anioDesde=2019&anioHasta=2024&fabricante=toy&tipo=sed` | Filtrar automóviles |
| GET | `/api/automoviles/{id}` | Consultar un automóvil por ID |
| POST | `/api/automoviles` | Crear un automóvil |
| PUT | `/api/automoviles/{id}` | Actualizar un automóvil |
| DELETE | `/api/automoviles/{id}` | Eliminar un automóvil |

El cuerpo de POST y PUT acepta `color`, `anio`, `fabricante` y `tipo`.
El tipo debe ser `sedán`, `4x4` o `moto`; el año permitido va de 1886 a 2100.
POST devuelve `201 Created`, PUT y DELETE devuelven `204 No Content` cuando se
completan, un ID inexistente devuelve `404 Not Found` y los datos inválidos
devuelven `400 Bad Request`. No se permite eliminar un automóvil con ingresos
asociados; en ese caso se devuelve `409 Conflict`.

Ejemplo de JSON:

```json
{
  "color": "Verde",
  "anio": 2024,
  "fabricante": "Honda",
  "tipo": "sedán"
}
```

Los parámetros de filtro son opcionales y se combinan con AND. Los filtros de
color, fabricante y tipo permiten coincidencias parciales sin distinguir
mayúsculas/minúsculas. `anioDesde` y `anioHasta` delimitan el intervalo inclusivo.

## API de parqueos

La tabla de la base de datos se llama `PRQ_Parqueo`; la ruta del API es
`/api/parqueos`.

| Método | Ruta | Acción |
|---|---|---|
| GET | `/api/parqueos` | Consultar todos los parqueos |
| GET | `/api/parqueos?provincia=san%20jos&nombre=central&precioMinimo=500&precioMaximo=1500` | Filtrar parqueos |
| GET | `/api/parqueos/{id}` | Consultar un parqueo por ID |
| POST | `/api/parqueos` | Crear un parqueo |
| PUT | `/api/parqueos/{id}` | Actualizar un parqueo |
| DELETE | `/api/parqueos/{id}` | Eliminar un parqueo |

El cuerpo de POST y PUT acepta `nombreProvincia`, `nombre` y `precioPorHora`.
Los nombres respetan los máximos definidos por la tabla; el precio debe estar
entre `0` y `99999999.99`, con hasta dos decimales. POST devuelve `201 Created`,
PUT y DELETE devuelven `204 No Content` cuando se completan, un ID inexistente
devuelve `404 Not Found` y los datos inválidos devuelven `400 Bad Request`. No se
puede eliminar un parqueo con ingresos asociados; en ese caso se devuelve
`409 Conflict` para respetar la clave foránea.

Ejemplo de JSON:

```json
{
  "nombreProvincia": "Heredia",
  "nombre": "Parqueo Central",
  "precioPorHora": 850.00
}
```

Los filtros son opcionales y se combinan con AND. Provincia y nombre permiten
coincidencias parciales sin distinguir mayúsculas/minúsculas; el rango de precios
es inclusivo.

## API de ingresos

La tabla de la base de datos se llama `PRQ_Ingreso_Automoviles`; la ruta del API
es `/api/ingresos`.

| Método | Ruta | Acción |
|---|---|---|
| GET | `/api/ingresos` | Consultar todos los ingresos |
| GET | `/api/ingresos/{consecutivo}` | Consultar por consecutivo |
| GET | `/api/ingresos/por-tipo?tipo=sed&desde=2026-10-01T00:00:00&hasta=2026-10-31T23:59:59` | Filtrar por tipo y fechas |
| GET | `/api/ingresos/por-provincia?provincia=san%20jos&desde=2026-10-01T00:00:00&hasta=2026-10-31T23:59:59` | Filtrar por provincia y fechas |
| POST | `/api/ingresos` | Registrar un ingreso |
| PUT | `/api/ingresos/{consecutivo}` | Actualizar un ingreso |
| DELETE | `/api/ingresos/{consecutivo}` | Eliminar un ingreso |

El cuerpo de POST y PUT acepta `idParqueo`, `idAutomovil`,
`fechaHoraEntrada` y `fechaHoraSalida` (opcional). Las fechas se envían en
formato ISO 8601. Ambos IDs deben existir; el API responde `400 Bad Request`
cuando una referencia no existe o la fecha de salida precede a la entrada. POST
devuelve `201 Created`, PUT y DELETE devuelven `204 No Content` cuando se
completan y un consecutivo inexistente devuelve `404 Not Found`.

Las respuestas GET incluyen los objetos relacionados `parqueo` y `automovil`,
además de duración y monto cuando existe una salida. Por ejemplo:

```json
{
  "idParqueo": 1,
  "idAutomovil": 1,
  "fechaHoraEntrada": "2026-10-30T08:00:00",
  "fechaHoraSalida": "2026-10-30T09:30:00"
}
```

Los filtros de tipo y provincia son aproximados, ignoran mayúsculas/minúsculas y
usan un intervalo inclusivo sobre `fecha_hora_entrada`. Ambos requieren `desde` y
`hasta` en formato ISO 8601.

## User Secrets y PostgreSQL

El proyecto tiene un `UserSecretsId` y lee la cadena de conexión desde
`ConnectionStrings:Parqueo`. Configúrala localmente con el usuario y la contraseña
asignados por el profesor; no pongas esos valores en código ni en Git:

```powershell
powershell -ExecutionPolicy Bypass -File .\Parqueo\configurar-conexion.ps1
dotnet run --project Parqueo\Parqueo.csproj -- --Storage:Mode postgres
```

El script pide la contraseña de forma oculta y la guarda en User Secrets, fuera
del repositorio. Sus valores predeterminados son el host
`postgresql-hecferme.postgres.database.azure.com`, la base
`advanced_programming` y el usuario `user_03`; puedes cambiarlos en los prompts.
El script [diseño-bd.sql](./Parqueo/dise%C3%B1o-bd.sql) crea las tablas e índices
de forma idempotente en el esquema `user_03`. Puedes ejecutarlo con `psql`
después de configurar el cliente:

```powershell
psql -h postgresql-hecferme.postgres.database.azure.com -U user_03 -d advanced_programming -W -f Parqueo\diseño-bd.sql
```

La contraseña se solicita de forma interactiva; no la incluyas en el comando.
El script no crea procedimientos almacenados: el CRUD del API usa EF Core y los
repositorios existentes, por lo que esos procedimientos no serían utilizados.
El modo PostgreSQL no crea tablas automáticamente; primero debe ejecutarse el
script y el usuario debe poder acceder al esquema `user_03`.

## Scaffold de las tablas PRQ

La herramienta local `dotnet-ef` se instala desde el manifiesto de herramientas
del repositorio. Desde la raíz, ejecuta:

```powershell
dotnet tool restore
dotnet ef dbcontext scaffold "Name=ConnectionStrings:Parqueo" Npgsql.EntityFrameworkCore.PostgreSQL --project Parqueo\Parqueo.csproj --startup-project Parqueo\Parqueo.csproj --output-dir Models\Scaffolded --context-dir Data\Scaffolded --context ParqueoScaffoldContext --schema public --table PRQ_Automoviles --table PRQ_Parqueo --table PRQ_Ingreso_Automoviles --no-onconfiguring --force
```

El comando limita el scaffold a las tres tablas indicadas. Usa `--schema` con el
esquema que tenga asignado la instancia si no es `public`. El usuario de PostgreSQL
necesita permiso para acceder a ese esquema y leer sus tablas. El acceso MCP
configurado en este equipo no pudo leer el esquema (`permission denied`), por lo
que el scaffold real queda pendiente de permisos y de que las tablas PRQ estén
disponibles en la instancia.

## Repositorios y consultas

- `IRepository<TEntity>` expone lectura por llave primaria y CRUD.
- Las interfaces específicas agregan filtros parciales para automóviles y parqueos.
- Ingresos incluyen búsquedas por tipo/provincia y rango inclusivo de fecha de
  entrada, el precio por hora del parqueo, duración y monto.
- Las duraciones y el monto son `null` mientras no haya fecha de salida.
- En modo JSON, los archivos son `automoviles.json`, `parqueo.json` e
  `ingreso_automoviles.json`. En PostgreSQL se usan los nombres exactos `PRQ_*`.
