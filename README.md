# Sistema de Inventario

Sistema de gestión de inventario compuesto por una Web API, una app de escritorio (.NET MAUI) y una app web (ASP.NET Core Razor Pages). El dominio (entidades, DTOs y lógica de negocio) vive en un proyecto compartido para que todos los clientes lo reutilicen.

## Stack

- .NET 10
- ASP.NET Core Web API (controllers) + autenticación JWT
- ASP.NET Core Razor Pages para el cliente web, con el theme [SmartAdmin](https://smartadmin.dev/) (Bootstrap)
- Entity Framework Core (SQLite) para la persistencia — cada sucursal tiene su propio archivo local, no hay servidor de base de datos compartido
- QuestPDF para la generación de reportes/documentos en PDF
- .NET MAUI para el cliente de escritorio (Windows / Android / iOS / Mac Catalyst)
- MSTest + Moq + FluentAssertions para las pruebas

## Estructura de la solución

| Proyecto | Tipo | Responsabilidad |
|---|---|---|
| `Inventario.Api` | ASP.NET Core Web API | Controllers, autenticación JWT, punto de entrada HTTP |
| `Inventario.Core` | Biblioteca de clases | Entidades de dominio, modelos/DTOs y servicios — sin dependencias a otros proyectos |
| `Inventario.Infrastructure` | Biblioteca de clases | `DbContext` (EF Core), repositorios y generación de PDF (QuestPDF) |
| `Inventario.Desktop` | .NET MAUI App | Cliente de escritorio/móvil |
| `Inventario.UnitTests` | MSTest | Pruebas unitarias de `Core` e `Infrastructure` |
| `Inventario.IntegrationTests` | MSTest | Pruebas de integración contra `Inventario.Api` |
| `Inventario.Web` | ASP.NET Core Razor Pages | Cliente web (theme SmartAdmin), consume `Inventario.Api` por HTTP |

### Referencias entre proyectos

```
Inventario.Api ─────► Inventario.Core
             └───────► Inventario.Infrastructure ──► Inventario.Core
Inventario.Desktop ──► Inventario.Core
Inventario.Web ──────► Inventario.Core (modelos compartidos) + HttpClient → Inventario.Api
Inventario.UnitTests ───► Inventario.Core, Inventario.Infrastructure
Inventario.IntegrationTests ► Inventario.Api
```

`Inventario.Core` es el centro del dominio y no depende de ningún otro proyecto de la solución.
`Inventario.Web` no referencia `Inventario.Infrastructure`: consume los datos exclusivamente a través de la API vía `HttpClient`, para no duplicar la capa de acceso a datos fuera de `Inventario.Api`.

### Paquetes NuGet principales

| Proyecto | Paquetes |
|---|---|
| `Inventario.Api` | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| `Inventario.Infrastructure` | `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`, `QuestPDF` |
| `Inventario.UnitTests` | `Moq`, `FluentAssertions` |
| `Inventario.Web` | Ninguno adicional — usa `IHttpClientFactory` (incluido en el SDK web) y los assets estáticos del theme SmartAdmin en `wwwroot/` |

## Cómo correrlo

Cada sucursal es autónoma: su propia base SQLite local (`%AppData%\InventarioApp\inventario.db`), su propia `Inventario.Api`, y `Inventario.Web`/`Inventario.Desktop` apuntando solo a esa API en `localhost`. No hay servidor central ni se expone nada a la red.

1. Corre la API desde la raíz del repo — al arrancar aplica las migraciones pendientes automáticamente (crea la base la primera vez) y siembra una sucursal inicial si no hay ninguna:

   ```bash
   dotnet run --project Inventario.Api
   ```

   `ConnectionStrings:DefaultConnection` en `appsettings.json` normalmente se deja vacío (usa el `%AppData%` de arriba); solo se define para apuntar a otra ruta.

2. La primera vez, abre `Inventario.Desktop` (o pega la URL de la Api) y usa la pantalla de login para dar de alta el primer Administrador (`POST /api/auth/registro-inicial`, sin autenticación mientras no exista ningún usuario).

3. Corre el cliente web (necesita la API corriendo; la URL base se configura en `Inventario.Web/appsettings.json` → `InventarioApi:BaseUrl`, por defecto `http://localhost:5025`):

   ```bash
   dotnet run --project Inventario.Web
   ```

4. Abre `Inventario.Desktop` desde Visual Studio y ejecútalo apuntando al perfil de Windows (o Android/iOS) para probar el cliente de escritorio.

También puede abrirse `Sistema-Inventario.slnx` directamente en Visual Studio y correr `Inventario.Api` (y opcionalmente `Inventario.Web`) como proyectos de inicio.

### Instalar la Api como servicio en una sucursal

Para producción, `Inventario.Api` corre como Windows Service (independiente del Desktop) en la PC de cada sucursal:

```powershell
scripts\install-api-service.ps1
```

Publica la Api en `C:\InventarioApp\Api`, crea/actualiza el servicio `InventarioApi` (arranque automático) y lo deja escuchando en `http://localhost:5025` (ver `Inventario.Api/appsettings.json` → `Urls`). Ver los comentarios del script para parámetros (`-ServiceName`, `-InstallPath`, `-SelfContained`).

### Respaldo entre sucursales

Un Administrador puede exportar/importar la base completa desde `Inventario.Desktop` → menú "Respaldo" (o directamente `GET`/`POST /api/backup/exportar` y `/api/backup/importar`). Exportar genera un `.zip` con `inventario.db` + un manifiesto de versión de esquema; importar rechaza respaldos de un esquema distinto salvo que se confirme "forzar", y siempre deja una copia `.bak` del archivo reemplazado.

## Pruebas

```bash
dotnet test Inventario.UnitTests
dotnet test Inventario.IntegrationTests
```
