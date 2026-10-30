# Sistema de parqueo

Aplicación de consola en .NET 8 con repositorios para PostgreSQL/EF Core y JSON local.

## Ejecutar con JSON local

Desde la raíz del repositorio:

```powershell
dotnet run --project Parqueo\Parqueo.csproj
```

Por defecto se leen los tres JSON junto al ejecutable y se imprimen sus cantidades.
Los repositorios JSON implementan búsquedas parciales, llave primaria y operaciones
CRUD. Las escrituras reemplazan el archivo de forma atómica.

## User Secrets y PostgreSQL

El proyecto tiene un `UserSecretsId` y lee la cadena de conexión desde
`ConnectionStrings:Parqueo`. Configúrala localmente con el usuario y la contraseña
asignados por el profesor; no pongas esos valores en código ni en Git:

```powershell
dotnet user-secrets set "ConnectionStrings:Parqueo" "Host=<servidor>;Port=5432;Database=<base>;Username=<usuario>;Password=<contraseña>;SSL Mode=Require;Trust Server Certificate=true" --project Parqueo\Parqueo.csproj
dotnet run --project Parqueo\Parqueo.csproj -- --Storage:Mode postgres
```

Los secretos se guardan fuera del repositorio, en el perfil de usuario de .NET.
El modo PostgreSQL no crea ni modifica tablas automáticamente; el esquema debe
estar disponible antes de usar las consultas y operaciones CRUD.

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
