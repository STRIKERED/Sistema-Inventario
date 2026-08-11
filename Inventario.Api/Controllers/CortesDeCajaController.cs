using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CortesDeCajaController : ControllerBase
{
    private readonly ICorteDeCajaRepository _corteRepository;
    private readonly IVentaRepository _ventaRepository;

    public CortesDeCajaController(ICorteDeCajaRepository corteRepository, IVentaRepository ventaRepository)
    {
        _corteRepository = corteRepository;
        _ventaRepository = ventaRepository;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CorteDeCaja>> ObtenerPorId(int id)
    {
        var corte = await _corteRepository.ObtenerPorIdAsync(id);
        if (corte is null)
        {
            return NotFound();
        }

        return Ok(corte);
    }

    [HttpGet("caja/{cajaId:int}")]
    public async Task<ActionResult<IEnumerable<CorteDeCaja>>> ObtenerPorCaja(int cajaId)
    {
        var cortes = await _corteRepository.ObtenerPorCajaAsync(cajaId);
        return Ok(cortes);
    }

    [HttpGet("caja/{cajaId:int}/abierto")]
    public async Task<ActionResult<CorteDeCaja>> ObtenerAbiertoPorCaja(int cajaId)
    {
        var corte = await _corteRepository.ObtenerAbiertoPorCajaAsync(cajaId);
        if (corte is null)
        {
            return NotFound();
        }

        return Ok(corte);
    }

    [HttpPost("abrir")]
    public async Task<ActionResult<CorteDeCaja>> Abrir(AbrirCorteRequest request)
    {
        var abierto = await _corteRepository.ObtenerAbiertoPorCajaAsync(request.CajaId);
        if (abierto is not null)
        {
            return Conflict($"La caja {request.CajaId} ya tiene un corte abierto (Id {abierto.Id}).");
        }

        var corte = new CorteDeCaja
        {
            CajaId = request.CajaId,
            UsuarioId = request.UsuarioId,
            MontoInicial = request.MontoInicial,
            Estado = EstadoCorteDeCaja.Abierto,
            FechaApertura = DateTime.UtcNow
        };

        var creado = await _corteRepository.CrearAsync(corte);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}/cerrar")]
    public async Task<ActionResult<CorteDeCaja>> Cerrar(int id, CerrarCorteRequest request)
    {
        var corte = await _corteRepository.ObtenerPorIdAsync(id);
        if (corte is null)
        {
            return NotFound();
        }

        if (corte.Estado != EstadoCorteDeCaja.Abierto)
        {
            return BadRequest("Este corte de caja ya está cerrado.");
        }

        var ventas = await _ventaRepository.ObtenerPorCorteDeCajaAsync(id);
        var totalEfectivo = ventas.Where(v => v.MetodoPago == MetodoPago.Efectivo).Sum(v => v.Total);

        corte.MontoFinalSistema = corte.MontoInicial + totalEfectivo;
        corte.MontoFinalContado = request.MontoFinalContado;
        corte.Diferencia = corte.MontoFinalContado - corte.MontoFinalSistema;
        corte.Estado = EstadoCorteDeCaja.Cerrado;
        corte.FechaCierre = DateTime.UtcNow;

        await _corteRepository.ActualizarAsync(corte);
        return Ok(corte);
    }

    public record AbrirCorteRequest(int CajaId, int UsuarioId, decimal MontoInicial);

    public record CerrarCorteRequest(decimal MontoFinalContado);
}
