using Inventario.Web.Filters;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    // Deny-by-default: cualquier página nueva bajo Pages/ requiere sesión salvo que se excluya aquí
    // explícitamente. Las carpetas de abajo (fuera de Auth/Error) son la plantilla SmartAdmin
    // original (demos de UI sin datos reales) que se dejó como referencia de estilos.
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToFolder("/Auth");
    options.Conventions.AllowAnonymousToFolder("/Error");
    options.Conventions.AllowAnonymousToFolder("/Apps");
    options.Conventions.AllowAnonymousToFolder("/Docs");
    options.Conventions.AllowAnonymousToFolder("/Forms");
    options.Conventions.AllowAnonymousToFolder("/Forum");
    options.Conventions.AllowAnonymousToFolder("/Icons");
    options.Conventions.AllowAnonymousToFolder("/Tables");
    options.Conventions.AllowAnonymousToFolder("/Ui");
    options.Conventions.AllowAnonymousToFolder("/Utilities");
})
.AddMvcOptions(options =>
{
    // Filtros globales: el orden importa porque ambos son excepción/pre-ejecución, pero ninguno
    // depende del otro en este caso. Filters.Add<T>() resuelve el filtro vía DI (constructor
    // injection normal), no hace falta registrar estos tipos aparte en el contenedor.
    options.Filters.Add<RequiereInventarioSeleccionadoFilter>();
    options.Filters.Add<ManejoErroresApiFilter>();
});

builder.Services.AddHttpContextAccessor();

// Cookie de sesión del lado del servidor: guarda el JWT de Inventario.Api (y el resto de la sesión)
// cifrado/firmado en el navegador. Patrón BFF — el navegador nunca ve el JWT directamente, solo la
// Razor Pages app lo reenvía server-to-server (ver AuthHeaderHandler).
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "InventarioWeb.Auth";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<ICurrentSessionAccessor, CurrentSessionAccessor>();
builder.Services.AddScoped<ISesionAuthService, SesionAuthService>();
builder.Services.AddTransient<AuthHeaderHandler>();

void ConfigurarCliente(HttpClient client)
{
    var baseUrl = builder.Configuration["InventarioApi:BaseUrl"]
        ?? throw new InvalidOperationException("Falta configurar InventarioApi:BaseUrl en appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);
}

// Sin AuthHeaderHandler: login se llama antes de que exista una sesión/JWT que adjuntar.
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(ConfigurarCliente);

builder.Services.AddHttpClient<IProductoApiService, ProductoApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<IStockApiService, StockApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<IVentaApiService, VentaApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<ICajaApiService, CajaApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<ICotizacionApiService, CotizacionApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<IUsuarioApiService, UsuarioApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<IInventarioApiService, InventarioApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<ISucursalApiService, SucursalApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<IBackupApiService, BackupApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<IConfiguracionImpresionApiService, ConfiguracionImpresionApiService>(ConfigurarCliente).AddHttpMessageHandler<AuthHeaderHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
