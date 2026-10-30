using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Parqueo.Repositories;

namespace Parqueo.Controllers;

[ApiController]
[Route("api/automoviles")]
public sealed class AutomovilesController(IAutomovilRepository automoviles) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<Automovil>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Automovil>>> ObtenerTodos(
        [FromQuery] string? color,
        [FromQuery] int? anioDesde,
        [FromQuery] int? anioHasta,
        [FromQuery] string? fabricante,
        [FromQuery] string? tipo,
        CancellationToken cancellationToken)
    {
        if (anioDesde > anioHasta)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Intervalo de años inválido.",
                Detail = "anioDesde debe ser menor o igual que anioHasta."
            });
        }

        return Ok(await automoviles.BuscarAsync(
            color, anioDesde, anioHasta, fabricante, tipo, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<Automovil>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Automovil>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var automovil = await automoviles.ObtenerPorIdAsync(id, cancellationToken);
        return automovil is null ? NotFound() : Ok(automovil);
    }

    [HttpPost]
    [ProducesResponseType<Automovil>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Automovil>> Crear(
        [FromBody] AutomovilRequest request,
        CancellationToken cancellationToken)
    {
        var automovil = request.ToEntity();
        await automoviles.CrearAsync(automovil, cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = automovil.Id },
            automovil);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        int id,
        [FromBody] AutomovilRequest request,
        CancellationToken cancellationToken)
    {
        var actualizado = await automoviles.ActualizarAsync(
            request.ToEntity(id),
            cancellationToken);
        return actualizado ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            return await automoviles.EliminarAsync(id, cancellationToken)
                ? NoContent()
                : NotFound();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "No se puede eliminar el automóvil.",
                Detail = exception.Message
            });
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation
            })
        {
            return Conflict(new ProblemDetails
            {
                Title = "No se puede eliminar el automóvil.",
                Detail = "El automóvil tiene ingresos asociados."
            });
        }
    }
}
