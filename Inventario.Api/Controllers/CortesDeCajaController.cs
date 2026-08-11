using Inventario.Core.Dtos;
using Inventario.Core.Entities;
using Inventario.Core.Enums;
using Inventario.Core.Interfaces;
using Inventario.Core.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    public async Task<ActionResult<CorteDeCajaDto>> ObtenerPorId(int id)
    {
        var corte = await _corteRepository.ObtenerPorIdAsync(id);
        if (corte is null)
        {
            return NotFound();
        }

        return Ok(corte.ToDto());
    }

    [HttpGet("caja/{cajaId:int}")]
    public async Task<ActionResult<IEnumerable<CorteDeCajaDto>>> ObtenerPorCaja(int cajaId)
    {
        var cortes = await _corteRepository.ObtenerPorCajaAsync(cajaId);
        return Ok(cortes.ToDto());
    }

    [HttpGet("caja/{cajaId:int}/abierto")]
    public async Task<ActionResult<CorteDeCajaDto>> ObtenerAbiertoPorCaja(int cajaId)
    {
        var corte = await _corteRepository.ObtenerAbiertoPorCajaAsync(cajaId);
        if (corte is null)
        {
            return NotFound();
        }

        return Ok(corte.ToDto());
    }

    [HttpPost("abrir")]
    [Authorize(Roles = "Administrador,Gerente,Cajero")]
    public async Task<ActionResult<CorteDeCajaDto>> Abrir(AbrirCorteRequest request)
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
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado.ToDto());
    }

    [HttpPut("{id:int}/cerrar")]
    [Authorize(Roles = "Administrador,Gerente,Cajero")]
    public async Task<ActionResult<CorteDeCajaDto>> Cerrar(int id, CerrarCorteRequest request)
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
        return Ok(corte.ToDto());
    }
}
