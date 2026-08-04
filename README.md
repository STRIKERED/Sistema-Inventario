# Sistema de Inventario

Sistema de gestión de inventario compuesto por una Web API, una app de escritorio (.NET MAUI) y, más adelante, una app web (Blazor). El dominio (entidades, DTOs y lógica de negocio) vive en un proyecto compartido para que todos los clientes lo reutilicen.

## Stack

- .NET 10
- ASP.NET Core Web API (controllers) + autenticación JWT
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
| `Inventario.Web` *(próximamente)* | Blazor Web App | Cliente web |

### Referencias entre proyectos

```
Inventario.Api ────────► Inventario.Core
                └───────► Inventario.Infrastructure ──► Inventario.Core
Inventario.Desktop ─────► Inventario.Core
Inventario.Web (futuro) ► Inventario.Core
Inventario.UnitTests ───► Inventario.Core, Inventario.Infrastructure
Inventario.IntegrationTests ► Inventario.Api
```

`Inventario.Core` es el centro del dominio y no depende de ningún otro proyecto de la solución.

### Paquetes NuGet principales

| Proyecto | Paquetes |
|---|---|
| `Inventario.Api` | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| `Inventario.Infrastructure` | `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`, `QuestPDF` |
| `Inventario.UnitTests` | `Moq`, `FluentAssertions` |
| `Inventario.Web` *(cuando se cree)* | `Microsoft.AspNetCore.Components.WebAssembly` (si es Blazor WASM) |

> **Nota de licencia:** se fijó `FluentAssertions` en `7.2.2` a propósito — a partir de la v8 el paquete requiere licencia comercial para uso en empresas. La serie 7.x es la última sin esa restricción. Evitar actualizar a v8+ sin antes revisar [xceed.com/fluent-assertions](https://xceed.com/products/unit-testing/fluent-assertions/).

## Cómo correrlo

1. Configura la cadena de conexión en `Inventario.Api/appsettings.json` (`ConnectionStrings:DefaultConnection`) apuntando a tu instancia de SQL Server.
2. Desde `Inventario.Infrastructure`, genera y aplica las migraciones de EF Core (una vez que exista el `DbContext`):

   ```bash
   dotnet ef migrations add Initial --project Inventario.Infrastructure --startup-project Inventario.Api
   dotnet ef database update --project Inventario.Infrastructure --startup-project Inventario.Api
   ```

3. Corre la API desde la raíz del repo:

   ```bash
   dotnet run --project Inventario.Api
   ```

4. Abre `Inventario.Desktop` desde Visual Studio y ejecútalo apuntando al perfil de Windows (o Android/iOS) para probar el cliente de escritorio.

También puede abrirse `Sistema-Inventario.slnx` directamente en Visual Studio y correr el proyecto `Inventario.Api` como proyecto de inicio.

## Pruebas

```bash
dotnet test Inventario.UnitTests
dotnet test Inventario.IntegrationTests
```
