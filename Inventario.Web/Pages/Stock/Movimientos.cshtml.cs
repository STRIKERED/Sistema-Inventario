using System.ComponentModel.DataAnnotations;
using Inventario.Core.Dtos;
using Inventario.Core.Enums;
using Inventario.Web.Services.Api;
using Inventario.Web.Services.Http;
using Inventario.Web.Services.Sesion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Inventario.Web.Pages.Stock
{
    public class MovimientosModel : PageModel
    {
        private readonly IStockApiService _stockApiService;
        private readonly ICurrentSessionAccessor _sesionActual;

        public MovimientosModel(IStockApiService stockApiService, ICurrentSessionAccessor sesionActual)
        {
            _stockApiService = stockApiService;
            _sesionActual = sesionActual;
        }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? Desde { get; set; }

        [BindProperty(SupportsGet = true)]
        [DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }

        [BindProperty(SupportsGet = true)]
        public TipoMovimientoInventario? Tipo { get; set; }

        public IReadOnlyList<MovimientoInventarioDto> Movimientos { get; private set; } = [];
        public string? ErrorMensaje { get; private set; }

        public async Task OnGetAsync()
        {
            var inventarioId = _sesionActual.InventarioOperativoId!.Value;

            try
            {
                Movimientos = await _stockApiService.ObtenerMovimientosPorInventarioAsync(inventarioId, Desde, Hasta, Tipo);
            }
            catch (ApiException ex) when (ex.StatusCode != 401)
            {
                ErrorMensaje = ex.Message;
            }
        }
    }
}
