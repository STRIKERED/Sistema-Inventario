# Sistema de Inventario

Sistema de gestión de inventario compuesto por una Web API, una app de escritorio (.NET MAUI) y una app web (ASP.NET Core Razor Pages). El dominio (entidades, DTOs y lógica de negocio) vive en un proyecto compartido para que todos los clientes lo reutilicen.

## Stack

- .NET 10
- ASP.NET Core Web API (controllers) + autenticación JWT
- ASP.NET Core Razor Pages para el cliente web, con el theme [SmartAdmin](https://smartadmin.dev/) (Bootstrap)
- Entity Framework Core (SQL Server) para la persistencia
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
| `Inventario.Infrastructure` | `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`, `QuestPDF` |
| `Inventario.UnitTests` | `Moq`, `FluentAssertions` |
| `Inventario.Web` | Ninguno adicional — usa `IHttpClientFactory` (incluido en el SDK web) y los assets estáticos del theme SmartAdmin en `wwwroot/` |

## Cómo correrlo

1. Configura la cadena de conexión en `Inventario.Api/appsettings.json` (`ConnectionStrings:DefaultConnection`) apuntando a tu instancia de SQL Server.
2. Aplica las migraciones de EF Core:

   ```bash
   dotnet ef database update --project Inventario.Infrastructure --startup-project Inventario.Api
   ```

3. Corre la API desde la raíz del repo:

   ```bash
   dotnet run --project Inventario.Api
   ```

4. Corre el cliente web (necesita la API corriendo; la URL base se configura en `Inventario.Web/appsettings.json` → `InventarioApi:BaseUrl`):

   ```bash
   dotnet run --project Inventario.Web
   ```

5. Abre `Inventario.Desktop` desde Visual Studio y ejecútalo apuntando al perfil de Windows (o Android/iOS) para probar el cliente de escritorio.

También puede abrirse `Sistema-Inventario.slnx` directamente en Visual Studio y correr `Inventario.Api` (y opcionalmente `Inventario.Web`) como proyectos de inicio.

## Pruebas

```bash
dotnet test Inventario.UnitTests
dotnet test Inventario.IntegrationTests
```
